using UnityEngine;
using UnityEngine.InputSystem;
using ProjectileCurveVisualizerSystem;

public class ArrowShooter : MonoBehaviour
{
    [Header("참조")]
    public Transform arrowSpawnPoint;  // 화살이 생성될 위치
    public GameObject arrowPrefab;     // 화살 프리팹
    public ProjectileCurveVisualizer curveVisualizer;
    
    [Header("화살 설정")]
    public float arrowSpeed = 20f;
    private bool isAiming = false;     // 조준 중 여부
    private Vector3 predictedHitPoint; // 예측 충돌 위치
    private Vector3 launchVelocity;    // 예상 발사 속도

    private void Update()
    {
        if (isAiming)
        {
            // 궤적 시각화 갱신
            curveVisualizer.VisualizeProjectileCurve(
                arrowSpawnPoint.position,
                0f,
                arrowSpawnPoint.forward * arrowSpeed,
                0.1f,
                0.1f,
                false,
                out Vector3 updatedPosition,
                out RaycastHit hit
            );

            predictedHitPoint = hit.point; // 충돌 예상 지점 저장
        }
    }

    private void OnFireStart()
    {
        isAiming = true;
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
            arrow.Throw(arrowSpawnPoint.forward * arrowSpeed); // Throw() 호출하여 발사
        }
    }
    
    private void OnFire(InputValue value)
    {
        float pressedValue = value.Get<float>(); 
        // 1이면 눌림, 0이면 안 눌림
        bool isPressed = pressedValue > 0.5f; // 임계값(0.5) 이상이면 true 처리

        if (isPressed)
        {
            OnFireStart();
        }
        else
        {
            OnFireRelease();
        }
    }
}