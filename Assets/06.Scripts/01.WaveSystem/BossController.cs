using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class BossController : LivingEntity
{
    #region 필드 변수

    [Header("보스 설정")]
    [SerializeField] private int maxHealth = 1000;
    [SerializeField] private int attackDamage = 100;  // 공격 데미지

    [Header("공격 패턴")]
    // [SerializeField] private GameObject projectilePrefab;
    // [SerializeField] private float attackRate = 3f;
    // [SerializeField] private int projectileCount = 5;
    // [SerializeField] private float spreadAngle = 30f;

    [Header("참조")]
    // [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform targetBattlePosition;
    [SerializeField] private Castle targetCastle;  // 공격 대상 성

    [Header("보스 애니메이션 관련")]
    private const int ANIM_IDLE = 1;
    private const int ANIM_MOVE = 2;
    private const int ANIM_ATTACK = 3;
    private const int ANIM_DAMAGE = 4;
    private const int ANIM_DIE = 5;

    [Header("출현 효과")]
    [SerializeField] private Material bossMaterial; // Inspector에서 할당할 보스 머티리얼
    [SerializeField] private float initialAlpha = 0.0f;  // 초기 알파값
    [SerializeField] private float alphaChangePerWave = 0.1f;  // 웨이브당 알파값 증가량

    private Rigidbody rb;
    private NavMeshAgent navAgent;  // 네비게이션 에이전트
    private bool isInBattleMode = false;
    private bool isMovingToBattlePosition = false;
    
    private float currentAlpha;  // 현재 알파값
    private Material currentMaterialInstance; // 현재 사용 중인 머티리얼 인스턴스를 추적

    #endregion

    #region 유니티 이벤트

    private void Awake()
    {
        // NavMeshAgent 컴포넌트 가져오기 (없으면 추가)
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
            navAgent.speed = 7.0f;
            navAgent.angularSpeed = 120.0f;
            navAgent.acceleration = 8.0f;
            navAgent.stoppingDistance = 5.0f;  // 도달 범위를 5.0f로 증가
        }

        // 렌더러에서 머티리얼 직접 참조
        SkinnedMeshRenderer renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer != null && bossMaterial == null)
        {
            bossMaterial = renderer.sharedMaterial;
        }

        // 초기에는 비활성화
        navAgent.enabled = false;

        // 초기 알파값 설정
        currentAlpha = initialAlpha;
        SetAlpha(currentAlpha);

        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    private void OnEnable()
    {
        EventManager.Instance.OnWaveStart += HandleWaveStart;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnWaveStart -= HandleWaveStart;
    }

    protected override void Start()
    {
        startingHealth = maxHealth; // 부모 클래스의 startingHealth 설정
        base.Start(); // LivingEntity의 Start 호출하여 초기화 진행

        Initialize(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (targetBattlePosition != null && navAgent != null)
        {
            // 목표 위치 표시
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);  // 반투명 빨간색
            Gizmos.DrawWireSphere(targetBattlePosition.position, navAgent.stoppingDistance);
            
            // 현재 위치에서 목표 위치까지의 선 표시
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);  // 반투명 노란색
            Gizmos.DrawLine(transform.position, targetBattlePosition.position);
            
            // 레이 뷰 표시
            Gizmos.color = Color.cyan;  // 청록색
            Vector3 direction = (targetBattlePosition.position - transform.position).normalized;
            Gizmos.DrawRay(transform.position, direction * Vector3.Distance(transform.position, targetBattlePosition.position));
            
            // 현재 위치와 목표 위치에 큐브 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(targetBattlePosition.position, Vector3.one);
        }
    }

    #endregion

    #region 이벤트 핸들러

    // 웨이브 시작 핸들러
    private void HandleWaveStart(int waveNumber)
    {
        // 10웨이브 시작 시 전투 모드 시작
        if (waveNumber == 10)
        {
            StartBattle();
        }
        // 1-9웨이브는 알파값만 조정
        else if (waveNumber >= 1 && waveNumber <= 9)
        {
            // 웨이브에 따라 알파값 증가
            currentAlpha = initialAlpha + (waveNumber * alphaChangePerWave);
            SetAlpha(currentAlpha);
        }
    }

    #endregion

    #region 보스 관련

    // 초기화 메서드
    public void Initialize(bool battleMode)
    {
        isInBattleMode = battleMode;

        // 비전투 모드인 경우 애니메이션 설정
        if (!battleMode && animator != null)
        {
            animator.SetInteger("animation", ANIM_IDLE);  // 대기 상태
        }
    }

    // 전투 시작
    public void StartBattle()
    {
        // 완전히 불투명하게 설정
        SetAlpha(1.0f);
        
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // NavMesh 위치 찾기 및 이동
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            navAgent.Warp(hit.position);
        }
        
        // 네비게이션 활성화 및 목표 위치로 이동 시작
        if (targetBattlePosition != null)
        {
            navAgent.enabled = true;
            navAgent.SetDestination(targetBattlePosition.position);
            isMovingToBattlePosition = true;
            
            // 이동 애니메이션 설정
            if (animator != null)
            {
                animator.SetInteger("animation", ANIM_MOVE);  // 이동 상태
            }
            
            // 목적지 도착 체크 코루틴 시작
            StartCoroutine(CheckDestinationReached());
        }
        else
        {
            // 목표 위치가 없으면 바로 전투 시작
            EnterBattleMode();
        }
    }

    // 목적지 도착 체크 코루틴
    private IEnumerator CheckDestinationReached()
    {
        while (isMovingToBattlePosition)
        {
            // 목표 위치와의 실제 거리 계산
            float distanceToTarget = Vector3.Distance(transform.position, targetBattlePosition.position);
            
            // 목적지에 도착했는지 확인
            if (!navAgent.pathPending && distanceToTarget <= navAgent.stoppingDistance)
            {
                isMovingToBattlePosition = false;
                
                // 이동 애니메이션 종료
                if (animator != null)
                {
                    animator.SetInteger("animation", ANIM_IDLE);
                }
                
                // 전투 모드 진입
                EnterBattleMode();
            }
            
            yield return null;
        }
    }

    // 전투 모드 진입
    private void EnterBattleMode()
    {
        isInBattleMode = true;
        
        // 네비게이션 비활성화
        navAgent.enabled = false;
        
        if (animator != null)
        {
            animator.SetInteger("animation", ANIM_IDLE);
        }
        
        // 공격 코루틴 시작
        StartCoroutine(AttackPattern());
    }

    // LivingEntity의 TakeDamage 메서드 오버라이드
    public override void TakeDamage(float damage)
    {
        if (!isAlive) return;

        // 기본 데미지 처리는 부모 클래스에 위임
        base.TakeDamage(damage);

        // 체력 변경 이벤트 발생 (UI 업데이트용)
        float healthPercent = currentHealth / startingHealth;
        EventManager.Instance.BossHealthChangedEvent(healthPercent);
        
        // 남은 체력 디버그 로그 출력
        Debug.Log($"보스가 {damage}의 데미지를 받았습니다. 남은 체력: {currentHealth}/{startingHealth} ({healthPercent * 100}%)");
    }

    // LivingEntity의 Die 메서드 오버라이드
    public override void Die()
    {
        if (!isAlive) return;

        // 부모 클래스의 Die 호출
        base.Die();

        // 사망 애니메이션
        if (animator != null)
        {
            animator.SetInteger("animation", ANIM_DIE);
        }

        // 보스전 종료 이벤트 발송 (승리)
        EventManager.Instance.WaveEndEvent(10);  // 10웨이브 종료 이벤트

        // 각종 컴포넌트 비활성화
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // 3초 후 오브젝트 제거
        Destroy(gameObject, 3f);
    }

    // 공격 애니메이션 이벤트에서 호출되는 메서드
    public void OnAttackAnimationEvent()
    {
        if (targetCastle != null)
        {
            targetCastle.TakeDamage(attackDamage);
            // 공격 사운드 재생
            // SoundManager.Instance.PlaySound("BossAttack");
        }
    }

    // 공격 패턴 코루틴 수정
    private IEnumerator AttackPattern()
    {
        while (isInBattleMode && isAlive)
        {
            // 기본 공격 주기
            yield return new WaitForSeconds(3f);

            // 공격 애니메이션 실행
            if (animator != null)
            {
                animator.SetInteger("animation", ANIM_ATTACK);
                OnAttackAnimationEvent();
            }

            // 애니메이션 종료 대기
            yield return new WaitForSeconds(1f);

            // 다시 대기 상태로
            if (animator != null)
            {
                animator.SetInteger("animation", ANIM_IDLE);
            }
        }
    }
    
    #endregion

    #region 보스 시각 효과
    
    // 알파값 설정 메서드
    public void SetAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);  // 0-1 사이 값으로 제한
        
        if (bossMaterial != null)
        {
            // 렌더러 찾기
            SkinnedMeshRenderer renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                // 이전 머티리얼 인스턴스 삭제
                if (currentMaterialInstance != null)
                {
                    Destroy(currentMaterialInstance);
                }

                // 새 머티리얼 인스턴스 생성
                currentMaterialInstance = new Material(bossMaterial);
                
                // Unity Toon Shader 투명도 설정
                currentMaterialInstance.SetFloat("_TransparentEnabled", 1);  // 투명 모드 활성화
                currentMaterialInstance.SetFloat("_ClippingMode", 2);        // TRANSMODE로 설정
                
                // alpha가 0일 때 -1(완전 투명), alpha가 1일 때 0(완전 불투명)이 되도록 변환
                float tweakTransparency = -1 + alpha;
                currentMaterialInstance.SetFloat("_Tweak_transparency", tweakTransparency);
                
                // 새 머티리얼 적용
                renderer.material = currentMaterialInstance;
            }
            
            // 현재 알파값 저장
            currentAlpha = alpha;
        }
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 머티리얼 인스턴스 정리
        if (currentMaterialInstance != null)
        {
            Destroy(currentMaterialInstance);
        }
    }
    
    #endregion
}