using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;



public class Enemy : LivingEntity
{
    private enum EnemyState
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

    private float lastAttackTime;                       // 공격 쿨타임 변수
    private float originalStoppingDistance;             // 공격 범위 저장용 변수
    [SerializeField] private EnemyState currentState;   // 현재 상태 알려주는 이넘 변수
    private bool isDialogueActive = false;              // 다이얼로그 표시 중인지 여부

    public FormationManager formationManager;
    private Vector3? assignedPosition;
    private bool hasFormationPosition;
    private bool isInPosition = false;  // 포지션에 도달했는지 체크하는 변수 추가

    [SerializeField] private float deathDelay = 2.0f;
    
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
            originalStoppingDistance = enemyData.attackRange;  // 원래 값 저장
        }

        base.Start();   // 초기화 진행

        currentState = EnemyState.Idle;
    }

    private void OnEnable()
    {
        EventManager.Instance.OnDialogueStarted += HandleDialogueStarted;
        EventManager.Instance.OnDialogueEnded += HandleDialogueEnded;
        EventManager.Instance.OnWaveStart += UpdateEnemyHealthForWave;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnDialogueStarted -= HandleDialogueStarted;
        EventManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
        EventManager.Instance.OnWaveStart -= UpdateEnemyHealthForWave;
    }

    void Update()
    {
        if (!isAlive) return;
        
        // 다이얼로그 중이면 행동 중지
        if (isDialogueActive) return;
        
        // 타겟을 향해 y축 회전만 적용
        if (target != null)
        {
            // 타겟 방향 계산
            Vector3 direction = target.position - transform.position;
            direction.y = 0; // y축 성분 제거 (수직 회전 방지)
            
            // 현재 오일러 각도 유지
            Vector3 currentRotation = transform.eulerAngles;
        
            // 타겟 방향으로 부드럽게 회전
            if (direction != Vector3.zero)
            {
                // 타겟 방향에 대한 Y축 각도만 계산
                float targetYRotation = Quaternion.LookRotation(direction).eulerAngles.y;
            
                // Y축만 부드럽게 회전
                float newYRotation = Mathf.LerpAngle(currentRotation.y, targetYRotation, Time.deltaTime * 5f);
            
                // 새 회전 적용 (X, Z는 유지)
                transform.eulerAngles = new Vector3(currentRotation.x, newYRotation, currentRotation.z);
            }
        }

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
            return;
        }
        else
        {
            animator.SetTrigger("hit");
        }
    }

    #endregion

    #region 이벤트핸들러

    // 다이얼로그 시작 시 호출
    private void HandleDialogueStarted(EventManager.DialogueType type)
    {
        if (type == EventManager.DialogueType.Tutorial  || type == EventManager.DialogueType.SpawnPointAdded)
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
        if (type == EventManager.DialogueType.Tutorial  || type == EventManager.DialogueType.SpawnPointAdded)
        {
            isDialogueActive = false;
                
            // 네비게이션 재개
            if (agent != null && agent.enabled && currentState == EnemyState.Chasing)
            {
                agent.isStopped = false;
            }
        }
    }
    
    // 웨이브 시작 시 체력 갱신 메서드
    private void UpdateEnemyHealthForWave(int waveNumber)
    {
        if (enemyData != null)
        {
            // 현재 웨이브에 맞는 체력으로 갱신
            startingHealth = enemyData.CalculateHealthForWave(waveNumber);
            currentHealth = startingHealth;
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
    
        // NavMeshAgent 활성화
        agent.enabled = true;
    
        // NavMesh 위에 있는지 확인
        if (agent.isOnNavMesh)
        {
            BeginChase();
        }
        else
        {
            // NavMesh 위치 찾기 시도
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
                BeginChase();
            }
        }
    }

    #endregion

    #region FSM (Finite State Machine) 유한상태기계

    /// <summary>
    /// 적의 추적을 시작하는 메서드
    /// NavMeshAgent가 활성화되고 유효한 NavMesh 위에 배치되었음을 전제로 함.
    /// </summary>
    private void BeginChase()
    {
        if (target == null) return;
    
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            currentState = EnemyState.Chasing;
            agent.isStopped = false;
        }
        else
        {
            StartCoroutine(RecoverNavMeshAgent());
        }
    }

    /// <summary>
    /// 추적 상태
    /// </summary>
    void Chase()
    {
        if (target == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        // 아직 포메이션 포지션이 없고, 성문과의 거리가 정지 거리의 4배 이내일 때
        if (!hasFormationPosition && distanceToTarget <= originalStoppingDistance * 4f)
        {
            assignedPosition = formationManager.GetFormationPosition(this, target);
            if (assignedPosition.HasValue)
            {
                hasFormationPosition = true;
                agent.stoppingDistance = 0.5f;
            }
        }

        // 포메이션 포지션이 있는 경우
        if (hasFormationPosition && assignedPosition.HasValue)
        {
            agent.SetDestination(assignedPosition.Value);
            
            // 포지션에 도달했는지 체크
            float distanceToPosition = Vector3.Distance(transform.position, assignedPosition.Value);
            
            // 포지션에 도달하면 공격 시작
            if (distanceToPosition < 1.5f && !isInPosition)
            {
                isInPosition = true;
                currentState = EnemyState.Attacking;
                agent.isStopped = true;
                animator.SetBool("isChasing", false);
                lastAttackTime = Time.time - enemyData.attackCooldown;
                return;
            }
        }
        else
        {
            // 포메이션 포지션이 없는 경우
            agent.stoppingDistance = originalStoppingDistance;
            agent.SetDestination(target.position);
            
            // 공격 범위 도달 체크
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

// NavMeshAgent 복구 코루틴
    private IEnumerator RecoverNavMeshAgent()
    {
        // 현재 위치 저장
        Vector3 currentPosition = transform.position;
    
        // 에이전트 비활성화
        agent.enabled = false;
        yield return new WaitForSeconds(0.1f);
    
        // 에이전트 재활성화
        agent.enabled = true;
        
        // NavMesh 위치 찾기
        NavMeshHit hit;
        if (NavMesh.SamplePosition(currentPosition, out hit, 3.0f, NavMesh.AllAreas))
        {
            // NavMesh 위치로 이동
            transform.position = hit.position;
            agent.Warp(hit.position);
        
            // 상태 업데이트
            if (currentState == EnemyState.Dead)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }
            else
            {
                currentState = EnemyState.Chasing;
                agent.isStopped = false;
            }
        }
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
                IDamageable castle = target.GetComponent<IDamageable>();
                if (castle != null)
                {
                    castle.TakeDamage(enemyData.attackDamage);
                }

                // 현재 시간을 할당
                lastAttackTime = Time.time;
            }
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

        agent.isStopped = true;
        agent.enabled = false;

        // 추가해야 할 코드: 물리 시뮬레이션 비활성화
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // 이 한 줄이 핵심입니다!

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
        
        ParticlePoolManager.Instance.SpawnSoulParticle(particlePosition);

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
