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
    [SerializeField] private EnemyState currentState;    // 현재 상태 알려주는 이넘 변수
    private bool isDialogueActive = false;  // 다이얼로그 표시 중인지 여부

    public FormationManager formationManager;
    private Vector3? assignedPosition;
    private bool hasFormationPosition;
    private bool isInPosition = false;  // 포지션에 도달했는지 체크하는 변수 추가

    [SerializeField] private float deathDelay = 2.0f;

    // 죽었을 때 생성할 파티클 키
    private const string DEATH_POOF_KEY = "Loot_Poof_Variant";
    private const string SOUL_BEAM_KEY = "LootBeam_Generic_Epic_Variant";


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

    private void OnEnable()
    {
        EventManager.Instance.OnDialogueStarted += HandleDialogueStarted;
        EventManager.Instance.OnDialogueEnded += HandleDialogueEnded;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnDialogueStarted -= HandleDialogueStarted;
        EventManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
    }

    // 다이얼로그 시작 시 호출
    private void HandleDialogueStarted(EventManager.DialogueType type)
    {
        if (type == EventManager.DialogueType.Tutorial)
        {
            isDialogueActive = true;
            
            // 네비게이션 일시 중지
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
            }
            
            // 애니메이션 중지 (Idle 상태로)
            if (animator != null)
            {
                animator.SetBool("isChasing", false);
            }
        }
    }

    // 다이얼로그 종료 시 호출
    private void HandleDialogueEnded(EventManager.DialogueType type)
    {
        if (type == EventManager.DialogueType.Tutorial)
        {
                isDialogueActive = false;
                
            // 네비게이션 재개
            if (agent != null && agent.enabled && currentState == EnemyState.Chasing)
            {
                agent.isStopped = false;
            }
        }
    }

    void Update()
    {
        if (!isAlive) return;
        
        // 다이얼로그 중이면 행동 중지
        if (isDialogueActive) return;

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
        if (!isAlive) return;

        base.TakeDamage(damage);

        if (currentHealth <= 0)
        {
            return; // 체력이 0이하면 히트 모션 재생 안함
        }
        else
        {
            // 50% 확률로 히트 모션 재생 (과도한 히트 모션 방지)
            if (Random.value > 0.5f) animator.SetTrigger("hit");
        }
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
        if (target == null || animator == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        // 아직 포메이션 포지션이 없고, 성문 근처에 왔을 때
        if (!hasFormationPosition && distanceToTarget <= agent.stoppingDistance * 2f)
        {
            assignedPosition = formationManager.GetFormationPosition(this, target);
            if (assignedPosition.HasValue)
            {
                hasFormationPosition = true;
                agent.stoppingDistance = 0.1f;
            }
        }

        // 포메이션 포지션이 할당된 경우
        if (hasFormationPosition && assignedPosition.HasValue)
        {
            // 할당된 포지션으로 이동
            agent.SetDestination(assignedPosition.Value);
            
            // 포지션에 도달했는지 체크
            float distanceToPosition = Vector3.Distance(transform.position, assignedPosition.Value);
            if (distanceToPosition < 0.2f && !isInPosition)
            {
                isInPosition = true;
                currentState = EnemyState.Attacking;
                agent.isStopped = true;
                transform.LookAt(target);
                animator.SetBool("isChasing", false);
                
                // 첫 공격을 바로 실행하기 위해 lastAttackTime 초기화
                lastAttackTime = Time.time - enemyData.attackCooldown;
                return;
            }
        }
        else
        {
            // 포지션이 없는 경우 기본 추적
            agent.SetDestination(target.position);
            
            // 기존 공격 범위 체크
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                currentState = EnemyState.Attacking;
                agent.isStopped = true;
                animator.SetBool("isChasing", false);
                return;
            }
        }

        animator.SetBool("isChasing", true);
    }

    /// <summary>
    /// 공격 상태
    /// </summary>
    void Attack()
    {
        if (target != null && animator != null)
        {
            // 현재시간에서 마지막시간을 뺀 값이 쿨타임보다 커질경우 공격 실행
            if (Time.time - lastAttackTime >= enemyData.attackCooldown)
            {
                // 공격 애니메이션 먼저 실행
                animator.SetBool("isChasing", false);
                animator.SetInteger("attackType", Random.Range(0, 2));
                animator.SetTrigger("attack");

                // 데미지 적용
                Debug.Log($"공격 시도: target={target.name}, attackDamage={enemyData.attackDamage}");
                IDamageable castle = target.GetComponent<IDamageable>();
                if (castle != null)
                {
                    Debug.Log("Castle 컴포넌트를 찾았습니다. 데미지를 가합니다.");
                    castle.TakeDamage(enemyData.attackDamage);
                }
                else
                {
                    Debug.LogError("Castle 컴포넌트를 찾을 수 없습니다!");
                }

                // 현재 시간을 할당
                lastAttackTime = Time.time;
            }
        }
        else
        {
            if (target == null) Debug.LogError("target이 null입니다!");
            if (animator == null) Debug.LogError("animator가 null입니다!");
        }
    }

    /// <summary>
    /// 사망 상태
    /// </summary>
    public override void Die()
    {
        if (hasFormationPosition)
        {
            formationManager.ReleasePosition(this);
            hasFormationPosition = false;
            isInPosition = false;
        }

        base.Die();

        currentState = EnemyState.Dead;

        // 사망 시 게임매니저에게 골드 전달 (프로퍼티 사용을 위해 AddMoney 메서드 호출)
        GameManager.Instance.AddMoney((int)enemyData.goldDropAmount);

        animator.SetTrigger("isDead");
        
        // 일정 시간 후 파티클 효과 및 오브젝트 비활성화
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // 사망 모션을 위한 대기 시간
        yield return new WaitForSeconds(deathDelay);

        // 파티클 효과 생성
        Vector3 particlePosition = transform.position + Vector3.up * 0.5f; // 바닥보다 약간 위에서 생성

        // 1. 일시적인 파티클 효과 (Loot_Poof)
        ParticlePoolManager.Instance.GetParticle(DEATH_POOF_KEY, particlePosition, Quaternion.identity);

        // 2. 지속적인 파티클 효과 (LootBeam)
        ParticlePoolManager.Instance.GetParticle(SOUL_BEAM_KEY, particlePosition, Quaternion.identity, true);

        // 오브젝트 풀에 반환 (비활성화)
        pool?.Release(this);
        
        // 스폰 매니저한테 죽음을 알림 (웨이브 체크를 위해)
        spawnManager.EnemyDied(this);
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
