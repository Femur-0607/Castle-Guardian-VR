using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;



public class Enemy : LivingEntity
{
    public enum EnemyState
    {
        Idle,      // 초기 상태. NavMeshAgent가 활성화되기 전에는 아무 행동도 하지 않음.
        Chasing, // 추적
        Attacking, // 공격
        Dead // 사망
    }

    #region 필드 변수

    [Header("참조")]
    private IObjectPool<Enemy> pool;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    public Transform target;        // 성문
    public EnemyData enemyData;     // 'Enemy'의 스테이터스를 담당
    public SpawnManager spawnManager;

    [Header("모델 프리팹")]
    [SerializeField] private GameObject normalModel;
    [SerializeField] private GameObject archerModel;
    [SerializeField] private GameObject scoutModel;
    [SerializeField] private GameObject tankerModel;

    private float lastAttackTime;
    private EnemyState currentState;    // 현재 상태 알려주는 이넘 변수

    #endregion

    #region 유니티 이벤트 함수 및 상속 관련

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentsInChildren<Animator>()[0];
    }
    protected override void Start()
    {
        // 먼저 스크립터블 오브젝트 불러오기
        if (enemyData != null)
        {
            startingHealth = enemyData.startingHealth;
            agent.speed = enemyData.moveSpeed;
            agent.stoppingDistance = enemyData.attackRange;
        }

        base.Start();   // 초기화 진행

        currentState = EnemyState.Idle;
    }

    void Update()
    {
        if (!isAlive) return;

        // FSM 구현
        switch (currentState)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Chasing:
                Chase();
                break;
            case EnemyState.Attacking:
                Attack();
                break;
            case EnemyState.Dead:
                // 사망 상태에서는 행동 안함.
                break;
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (!isAlive) return;

        // 50% 확률로 히트 모션 재생 (과도한 히트 모션 방지)
        if (Random.value > 0.5f) animator.SetTrigger("hit");
    }

    #endregion

    #region 오브젝트 풀 관련

    public void SetPoolReference(IObjectPool<Enemy> poolRef)
    {
        pool = poolRef;
    }

    /// <summary>
    /// Pool에서 Get될 때마다 호출될 초기화 로직
    /// </summary>
    public void ResetState()
    {
        // LivingEntity에 체력 초기값이 있다면 여기서 재설정
        currentHealth = startingHealth;
        isAlive = true;

        currentState = EnemyState.Idle;

        ActivateModelByType(enemyData.enemyType);

        // NavMeshAgent를 딜레이 후 활성화하여 추적 시작
        StartCoroutine(DelayedAgentActivation(1f));
    }

    private IEnumerator DelayedAgentActivation(float delay)
    {
        agent.enabled = false;

        yield return new WaitForSeconds(delay);

        agent.enabled = true;

        BeginChase();
    }

    #endregion

    #region FSM (Finite State Machine) 유한상태기계

    /// <summary>
    /// 적의 추적을 시작하는 메서드
    /// NavMeshAgent가 활성화되고 유효한 NavMesh 위에 배치되었음을 전제로 함.
    /// </summary>
    public void BeginChase()
    {
        if (target != null && agent.isOnNavMesh)
        {
            currentState = EnemyState.Chasing;
            agent.isStopped = false;
        }
    }

    /// <summary>
    /// 추적 상태
    /// </summary>
    void Chase()
    {
        if (target != null && animator != null)
        {
            agent.SetDestination(target.position); // 성문으로 이동

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) // 공격범위안에 성문이 들어오면
            {
                currentState = EnemyState.Attacking; // 공격상태로 전환
                agent.isStopped = true; // 공격 시에는 이동을 멈춤
            }

            animator.SetBool("isChasing", true);
        }
    }

    /// <summary>
    /// 공격 상태
    /// </summary>
    void Attack()
    {
        if (target != null && animator != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > agent.stoppingDistance)
            {
                // 목표가 범위를 벗어나면 다시 추격 상태로 전환
                currentState = EnemyState.Chasing;
                agent.isStopped = false;
                return;
            }

            // 현재시간에서 마지막시간을 뺀 값이 쿨타임보다 커질경우 공격 실행
            if (Time.time - lastAttackTime >= enemyData.attackCooldown)
            {
                // 리빙 엔티티 스크립트가 있어야지 공격 가능
                LivingEntity castle = target.GetComponent<LivingEntity>();

                if (castle != null)
                {
                    castle.TakeDamage(enemyData.attackDamage);
                }

                // 현재 시간을 할당
                lastAttackTime = Time.time;
            }

            animator.SetBool("isChasing", false);

            // 공격 애니메이션 랜덤 재생
            int attackPattern = Random.Range(0, 1);
            animator.SetInteger("attackPattern", attackPattern);
            animator.SetTrigger("attack");
        }
    }

    /// <summary>
    /// 사망 상태
    /// </summary>
    public override void Die()
    {
        base.Die();

        currentState = EnemyState.Dead;

        spawnManager.EnemyDied(this);

        pool?.Release(this);

        // 사망 시 게임매니저에게 골드 전달 (프로퍼티 사용을 위해 AddMoney 메서드 호출)
        GameManager.Instance.AddMoney((int)enemyData.goldDropAmount);

        animator.SetBool("isDead", true);
    }

    #endregion

    #region 상태이상

    public void ApplySlowEffect(float duration, float slowAmount)
    {
        // 이미 실행 중인 슬로우 코루틴 중지
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }

        // 새 슬로우 코루틴 시작
        slowCoroutine = StartCoroutine(SlowEffectRoutine(duration, slowAmount));
    }

    private Coroutine slowCoroutine;

    private IEnumerator SlowEffectRoutine(float duration, float slowAmount)
    {
        // 원래 속도 저장
        float originalSpeed = agent.speed;

        // 속도 감소
        agent.speed = originalSpeed * (1f - slowAmount);

        // 지속 시간 대기
        yield return new WaitForSeconds(duration);

        // 속도 복원
        agent.speed = originalSpeed;

        slowCoroutine = null;
    }

    #endregion

    #region 모델링 관련

    public void ActivateModelByType(EnemyType type)
    {
        // 모든 모델 비활성화
        normalModel.SetActive(false);
        archerModel.SetActive(false);
        scoutModel.SetActive(false);
        tankerModel.SetActive(false);
        
        // 해당 타입의 모델만 활성화
        switch (type)
        {
            case EnemyType.Normal:
                normalModel.SetActive(true);
                break;
            case EnemyType.Archer:
                archerModel.SetActive(true);
                break;
            case EnemyType.Scout:
                scoutModel.SetActive(true);
                break;
            case EnemyType.Tanker:
                tankerModel.SetActive(true);
                break;
        }
        
        // 애니메이터 재설정 (모델 변경 후 애니메이터 참조 필요)
        UpdateAnimatorReference();
    }
    
    private void UpdateAnimatorReference()
    {
        // 활성화된 모델의 애니메이터 컴포넌트 찾기
        animator = GetComponentInChildren<Animator>();
    }

    #endregion
}
