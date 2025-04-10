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
            Debug.Log($"Enemy {gameObject.name}: Successfully activated on NavMesh");
            BeginChase();
        }
        else
        {
            Debug.LogWarning($"Enemy {gameObject.name}: Failed to activate on NavMesh, trying to find valid position");
        
            // NavMesh 위치 찾기 시도
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
                BeginChase();
                Debug.Log($"Enemy {gameObject.name}: Repositioned to NavMesh at {hit.position}");
            }
            else
            {
                Debug.LogError($"Enemy {gameObject.name}: Could not find nearby NavMesh position");
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
            Debug.LogWarning($"Enemy {gameObject.name}: Cannot begin chase, NavMeshAgent not ready");
            StartCoroutine(RecoverNavMeshAgent());
        }
    }

    /// <summary>
    /// 추적 상태
    /// </summary>
    void Chase()
{
    if (target == null || animator == null) return;
    
    // NavMeshAgent 유효성 검사를 먼저 수행
    if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
    {
        Debug.LogWarning($"Enemy {gameObject.name}: NavMeshAgent is not active or not on NavMesh. Trying to recover...");
        StartCoroutine(RecoverNavMeshAgent());
        return; // 복구 시도 중에는 다른 로직 실행 중지
    }

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

    // 목적지 설정 (안전하게)
    try
    {
        if (hasFormationPosition && assignedPosition.HasValue)
        {
            agent.SetDestination(assignedPosition.Value);
            
            // 포지션에 도달했는지 체크
            float distanceToPosition = Vector3.Distance(transform.position, assignedPosition.Value);
            
            // 주기적으로만 로그 출력
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"Enemy {gameObject.name}: distToPos={distanceToPosition:F2}, velocity={agent.velocity.magnitude:F2}");
            }
            
            // 포지션에 도달하면 공격 시작
            if (distanceToPosition < 1.5f && !isInPosition)
            {
                isInPosition = true;
                currentState = EnemyState.Attacking;
                SafeStopAgent();
                transform.LookAt(target);
                animator.SetBool("isChasing", false);
                
                // 첫 공격을 바로 실행하기 위해 lastAttackTime 초기화
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
                SafeStopAgent();
                animator.SetBool("isChasing", false);
                return;
            }
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Enemy {gameObject.name}: Error in Chase: {e.Message}");
        // 에러 발생 시 NavMeshAgent 복구 시도
        StartCoroutine(RecoverNavMeshAgent());
        return;
    }

    animator.SetBool("isChasing", true);
}

// NavMeshAgent 안전하게 정지
    private void SafeStopAgent()
    {
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }
    }

// NavMeshAgent 복구 코루틴
    private IEnumerator RecoverNavMeshAgent()
    {
        // 현재 위치 저장
        Vector3 currentPosition = transform.position;
    
        // 에이전트 비활성화
        agent.enabled = false;
        yield return new WaitForSeconds(0.2f);
    
        // 에이전트 재활성화
        agent.enabled = true;
        yield return new WaitForSeconds(0.1f);
    
        // NavMesh 위치 찾기
        NavMeshHit hit;
        if (NavMesh.SamplePosition(currentPosition, out hit, 3.0f, NavMesh.AllAreas))
        {
            // NavMesh 위치로 이동
            transform.position = hit.position;
            agent.Warp(hit.position);
            Debug.Log($"Enemy {gameObject.name}: Recovered to NavMesh at {hit.position}");
        
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
        else
        {
            Debug.LogError($"Enemy {gameObject.name}: Failed to find NavMesh position nearby");
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
    
    private void OnDrawGizmos()
{
    // 플레이 모드가 아닐 때는 최소한의 정보만 표시
    if (!Application.isPlaying)
    {
        if (target != null)
        {
            // 타겟(성문)과의 연결선
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(transform.position, target.position);
        }
        return;
    }
    
    // 타겟이 없으면 표시하지 않음
    if (target == null) return;
    
    // 에너미와 타겟(성문) 사이의 거리 표시
    Gizmos.color = Color.red;
    Gizmos.DrawLine(transform.position, target.position);
    
    // 텍스트 정보 표시 위치 계산
    Vector3 textPos = transform.position + Vector3.up * 2f;
    
    // 거리 및 상태 정보 표시
    float distToTarget = Vector3.Distance(transform.position, target.position);
    #if UNITY_EDITOR
    UnityEditor.Handles.BeginGUI();
    UnityEditor.Handles.Label(textPos, 
        $"State: {currentState}\n" +
        $"ToTarget: {distToTarget:F1}");
    #endif
    
    // 포메이션 포지션이 할당된 경우 추가 정보 표시
    if (hasFormationPosition && assignedPosition.HasValue)
    {
        // 할당된 포지션 표시
        Gizmos.color = isInPosition ? Color.green : Color.yellow;
        Gizmos.DrawSphere(assignedPosition.Value, 0.5f);
        
        // 에너미와 할당된 포지션 사이의 연결선
        Gizmos.DrawLine(transform.position, assignedPosition.Value);
        
        // 포지션까지의 거리 표시
        float distToPos = Vector3.Distance(transform.position, assignedPosition.Value);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(textPos + Vector3.up * 0.5f, 
            $"HasPosition: {hasFormationPosition}\n" +
            $"InPosition: {isInPosition}\n" +
            $"ToPos: {distToPos:F1}");
        #endif
        
        // 도달 범위 표시 (1.5f 원)
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f); // 반투명 노란색
        Gizmos.DrawWireSphere(assignedPosition.Value, 1.5f); // 도달 판정 거리
    }
    else
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(textPos + Vector3.up * 0.5f, 
            $"HasPosition: {hasFormationPosition}\n" +
            $"StopDist: {agent.stoppingDistance:F1}");
        #endif
    }
    
    // NavMeshAgent 경로 표시
    if (agent != null && agent.hasPath)
    {
        Gizmos.color = Color.blue;
        Vector3[] corners = agent.path.corners;
        
        for (int i = 0; i < corners.Length - 1; i++)
        {
            Gizmos.DrawLine(corners[i], corners[i + 1]);
            Gizmos.DrawSphere(corners[i], 0.2f);
        }
        
        if (corners.Length > 0)
        {
            Gizmos.DrawSphere(corners[corners.Length - 1], 0.2f);
        }
    }
    
    #if UNITY_EDITOR
    UnityEditor.Handles.EndGUI();
    #endif
}
}
