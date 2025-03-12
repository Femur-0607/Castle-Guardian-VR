using UnityEngine;
using UnityEngine.InputSystem;
using ProjectileCurveVisualizerSystem;

// 화살 오브젝트가 인풋을 받았을때 어떻게 처리해야하는지에 대한 스크립트
public class ArrowShooter : MonoBehaviour
{
    #region 필드 변수

    [Header("참조")]
    public Transform arrowSpawnPoint;       // 화살이 생성될 트랜스폼
    public ProjectilePool projectilePool;   // 화살 오브젝트 풀 참조
    public ProjectileCurveVisualizer curveVisualizer;   // 라인레더러 경로 생성 스크립트
    // playerCamera 참조는 필요 없음 (ArrowShooter가 카메라 자식이므로)

    [Header("화살 설정")]
    public float minSpeed = 1f;    // 활을 가장 조금 당겼을 때의 속도(곡률 최소)
    public float maxSpeed = 40f;    // 활을 최대로 당겼을 때의 속도(곡률 최대)
    public float dragToFullPower = 200f;
    // 마우스를 얼마나 아래로 드래그하면 maxSpeed에 도달할지(픽셀/단위)

    private float currentArrowSpeed;
    private bool isAiming = false;     // 조준 중 여부

    // 마우스 드래그 계산용
    private float initialMouseY;
    private float currentMouseY;

    #endregion

    #region 유니티 이벤트 함수

    private void OnEnable()
    {
        EventManager.Instance.OnFireStart += OnFireStart;
        EventManager.Instance.OnFireRelease += OnFireRelease;
        EventManager.Instance.OnFireCharging += OnFireCharging;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnFireStart -= OnFireStart;
        EventManager.Instance.OnFireRelease -= OnFireRelease;
        EventManager.Instance.OnFireCharging -= OnFireCharging;
    }

    #endregion

    #region 인풋에 따른 화살 로직

    /// <summary>
    /// 마우스 클릭시 발동하는 메서드 ( 화살 준비 ) 
    /// </summary>
    private void OnFireStart()
    {
        isAiming = true;
        initialMouseY = currentMouseY;  // 조준 시작 시의 마우스 Y값을 초기값으로 저장
        currentArrowSpeed = minSpeed;   // 초기값은 최소 속도
    }

    /// <summary>
    /// 마우스 위치 변경 이벤트 처리
    /// <para>주로 Y 좌표를 이용하여 드래그 거리를 계산한다.<br/>마우스를 아래로 드래그 ( 시위 당김 )</para>
    /// </summary>
    /// <param name="pos">현재 마우스 위치 (예: Vector3 타입의 좌표)로, 주로 Y값을 사용하여 드래그 거리를 계산함</param>
    private void OnFireCharging(Vector2 pos)
    {
        currentMouseY = pos.y;

        if (isAiming)
        {
            // 드래그 거리 계산: 처음 클릭한 Y와 현재 Y의 차이 (아래로 드래그 시 양수가 됨)
            float dragDistance = initialMouseY - currentMouseY;
            if (dragDistance < 0)
                dragDistance = 0;

            // 0~1 범위의 t 값으로 보정 후, 선형 보간(Lerp)을 이용해 힘 계산
            float t = Mathf.Clamp01(dragDistance / dragToFullPower);
            currentArrowSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);

            // 궤적 시각화 갱신
            curveVisualizer.VisualizeProjectileCurve(
                arrowSpawnPoint.position,
                0f,
                arrowSpawnPoint.forward * currentArrowSpeed,
                0.1f,
                0.1f,
                false,
                out Vector3 updatedPosition,
                out RaycastHit hit
            );
        }
    }

    /// <summary>
    /// 마우스 땔때 작동되는 메서드
    /// </summary> 화살 쏘기
    private void OnFireRelease()
    {
        if (!isAiming) return;

        SoundManager.Instance.PlaySound("ArrowShoot");

        isAiming = false;
        curveVisualizer.HideProjectileCurve(); // 화살 궤적 숨김

        // 풀에서 화살 가져오기
        Projectile arrow = projectilePool.GetProjectileAt(arrowSpawnPoint.position);

        // 발사 방향에 속도 적용
        Vector3 launchDirection = arrowSpawnPoint.forward * currentArrowSpeed;

        // 발사 방향 디버그 로그
        Debug.Log($"<color=orange>[발사 정보]</color> " +
                 $"방향: {arrowSpawnPoint.forward}, " +
                 $"속도: {currentArrowSpeed:F1}m/s");

        // 화살 발사
        arrow.Launch(launchDirection);
    }
    
    #endregion
}