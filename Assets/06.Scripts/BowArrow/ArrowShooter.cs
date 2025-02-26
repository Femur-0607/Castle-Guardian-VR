using UnityEngine;
using UnityEngine.InputSystem;
using ProjectileCurveVisualizerSystem;

// 실제 화살을 쏘는 스크립트
public class ArrowShooter : MonoBehaviour
{
    [Header("참조")]
    public Transform arrowSpawnPoint;  // 화살이 생성될 위치
    public GameObject arrowPrefab;     // 화살 프리팹
    public ProjectileCurveVisualizer curveVisualizer;
    
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

    private void OnFireStart()
    {
        isAiming = true;
        
        initialMouseY = currentMouseY;  // 조준 시작 시의 마우스 Y값을 초기값으로 저장
        currentArrowSpeed = minSpeed;   // 초기값은 최소 속도
    }
    
    // 마우스 위치 변경 이벤트 처리 (주로 Y 좌표를 이용하여 드래그 거리를 계산)
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

    private void OnFireRelease()
    {
        if (!isAiming) return;

        isAiming = false;
        curveVisualizer.HideProjectileCurve(); // 가이드라인 숨김

        // 화살 생성 및 발사
        GameObject arrowInstance = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        Projectile arrow = arrowInstance.GetComponent<Projectile>();

        if (arrow != null)
        {
            arrow.Throw(arrowSpawnPoint.forward * currentArrowSpeed); // Throw() 호출 저장된 화살의 힘으로 발사
        }
    }
}