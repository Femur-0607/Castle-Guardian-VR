using UnityEngine;
using ProjectileCurveVisualizerSystem;

/// <summary>
/// 화살 발사 시스템: 화살을 발사하는 역할을 담당
/// 사용자 입력을 처리하고 화살 발사의 힘과 방향을 제어
/// </summary>
public class ArrowShooter : MonoBehaviour
{
    #region 필드 변수

    [Header("화살 설정")]
    public float minSpeed = 1f;    // 활을 가장 조금 당겼을 때의 속도(곡률 최소)
    public float maxSpeed = 40f;    // 활을 최대로 당겼을 때의 속도(곡률 최대)
    public float chargeTime = 1.5f;  // 최대 힘까지 걸리는 시간

    private float currentArrowSpeed;  // 현재 화살 속도
    private bool isAiming = false;     // 조준 중 여부
    private float chargeTimer = 0f;  // 차징 타이머

    [Header("화살 프리팹-ArrowManager에서 설정")]
    [SerializeField] private GameObject muzzleEffectPrefab; // 발사 전 효과 프리팹 (ArrowManager에서 설정)
    [SerializeField] private ProjectileData projectileData; // 현재 화살 데이터 (ArrowManager에서 설정)

    [Header("궤적 관련")]
    [SerializeField] private Transform arrowSpawnPoint; // 화살 생성 위치
    [SerializeField] private ProjectileCurveVisualizer curveVisualizer; // 궤적 시각화 도구
    #endregion

    #region 유니티 이벤트 함수
    
    /// <summary>
    /// 컴포넌트 활성화 시 이벤트 구독
    /// </summary>
    private void OnEnable()
    {
        // 이벤트 매니저 이벤트 구독
        EventManager.Instance.OnFireStart += OnFireStart;
        EventManager.Instance.OnFireRelease += OnFireRelease;
        EventManager.Instance.OnFireCharging += OnFireCharging;
    }

    /// <summary>
    /// 컴포넌트 비활성화 시 이벤트 구독 해제
    /// </summary>
    private void OnDisable()
    {
        // 이벤트 매니저 이벤트 구독 해제
        EventManager.Instance.OnFireStart -= OnFireStart;
        EventManager.Instance.OnFireRelease -= OnFireRelease;
        EventManager.Instance.OnFireCharging -= OnFireCharging;
    }

    #endregion

    #region 이벤트 핸들러

    /// <summary>
    /// 발사 버튼 누를 때 호출 - 조준 시작
    /// </summary>
    private void OnFireStart()
    {
        // 쿨타임 중이면 조준 무시
        if (GameManager.Instance.IsArrowCooldown) return;
        
        isAiming = true;
        currentArrowSpeed = minSpeed;
        chargeTimer = 0f;  // 타이머 초기화
    }

    /// <summary>
    /// 발사 버튼 누르고 있는 동안 호출 - 화살 힘 계산
    /// </summary>
    private void OnFireCharging()
    {
        if (GameManager.Instance.IsArrowCooldown) return;
        
        if (isAiming)
        {
            // 차징 타이머 증가
            chargeTimer += Time.deltaTime;
            
            // 0~1 범위의 t 값 계산
            float t = Mathf.Clamp01(chargeTimer / chargeTime);
            
            // 선형 보간으로 힘 계산
            currentArrowSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);
            
            // 궤적 시각화 갱신
            curveVisualizer.VisualizeProjectileCurve(
                arrowSpawnPoint.position, 
                0f, 
                arrowSpawnPoint.forward * currentArrowSpeed,
                0.1f, // 화살 반지름
                0.1f, // 타임스텝
                false, // 수직 방향으로 발사하지 않음
                out Vector3 impactPos,
                out RaycastHit hit
            );
        }
    }

    /// <summary>
    /// 발사 버튼 뗄 때 호출 - 화살 발사
    /// </summary>
    private void OnFireRelease()
    {
        // 쿨타임 중이거나 조준 중이 아니면 발사 무시
        if (GameManager.Instance.IsArrowCooldown || !isAiming) return;

        try
        {
            // ParticleEffectPool을 사용하여 발사 이펙트 생성
            ParticleEffectPool.Instance.PlayEffect(muzzleEffectPrefab, arrowSpawnPoint.position,
                arrowSpawnPoint.rotation);
        }
        catch (System.Exception e)
        {
            Debug.LogError("화살 발사 중 오류 발생: " + e.Message);
        }

        // 발사 사운드 재생
        SoundManager.Instance.PlaySound("ArrowShoot");

        // 조준 종료 및 궤적 숨김
        isAiming = false;
        curveVisualizer.HideProjectileCurve(); // 화살 궤적 숨김

        try 
        {
            // ProjectilePool에서 현재 화살 타입에 맞는 화살 가져오기
            Projectile projectile = ProjectilePool.Instance.GetProjectile();
        
            // 화살 위치/회전 설정
            projectile.transform.position = arrowSpawnPoint.position;
            projectile.transform.rotation = arrowSpawnPoint.rotation;
        
            // 데이터 설정 및 발사
            projectile.SetProjectileData(projectileData);
            projectile.Launch(arrowSpawnPoint.forward * currentArrowSpeed);
        
            // 쿨타임 시작 - GameManager에 요청
            GameManager.Instance.StartArrowCooldown();
        }
        catch (System.Exception e)
        {
            Debug.LogError("화살 발사 중 오류 발생: " + e.Message);
        }
        
        // 쿨타임 시작 - GameManager에 요청
        GameManager.Instance.StartArrowCooldown();
    }

    #endregion

    /// <summary>
    /// 화살 프리팹 및 데이터 설정 - 화살 타입 변경 시 ArrowManager에서 호출
    /// </summary>
    /// <param name="newPrefab">새 화살 프리팹</param>
    /// <param name="data">새 화살 데이터</param>
    /// <param name="newMuzzleEffect">새 발사 이펙트 프리팹</param>
    public void SetProjectilePrefab(GameObject newPrefab, ProjectileData data, GameObject newMuzzleEffect)
    {
        // 새 데이터 설정
        projectileData = data;

        // 머즐 이펙트 업데이트
        muzzleEffectPrefab = newMuzzleEffect;
        
        // 화살 풀에 현재 화살 타입 설정 
        ProjectilePool.Instance.SetCurrentArrowType(data.projectileType);
    }
}