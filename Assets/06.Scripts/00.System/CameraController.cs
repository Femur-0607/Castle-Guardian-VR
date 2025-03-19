using System.Collections;
using UnityEngine;

/// <summary>
/// A/D 키 또는 VR 조이스틱을 사용하여 카메라 시점을 전환하는 스크립트
/// </summary>
public class CameraController : MonoBehaviour
{
    #region 필드 변수

    [Header("카메라 설정")]
    [SerializeField] private Camera centerCamera;       // 중앙 카메라
    [SerializeField] private Camera leftCamera;         // 왼쪽 카메라
    [SerializeField] private Camera rightCamera;        // 오른쪽 카메라

    // 빌드 모드 참조
    [SerializeField] private BuildManager buildManager;
    
    private Camera currentCamera;
    private Camera targetCamera;
    
    private CameraPosition currentPosition = CameraPosition.Center;
    private enum CameraPosition { Left, Center, Right }
    
    private bool isTransitioning = false;

    #endregion

    #region 유니티 이벤트 함수

    private void Awake()
    {
        // 기본으로 중앙 카메라를 활성화하고 나머지는 비활성화
        centerCamera.gameObject.SetActive(true);
        leftCamera.gameObject.SetActive(false);
        rightCamera.gameObject.SetActive(false);
        
        currentCamera = centerCamera;
        
        EventManager.Instance.CameraChangedEvent(currentCamera, "Center");
    }
    
    private void Start()
    {
        // 초기 카메라 설정 (중앙 카메라 활성화)
        SetActiveCamera(centerCamera);
    }
    
    private void OnEnable()
    {
        EventManager.Instance.OnCameraSwitch += HandleCameraSwitch;
        EventManager.Instance.OnWaveEnd += HandleWaveEnd;
    }
    
    private void OnDisable()
    {
        EventManager.Instance.OnCameraSwitch -= HandleCameraSwitch;
        EventManager.Instance.OnWaveEnd -= HandleWaveEnd;
    }
    
    #endregion

    #region 카메라 전환 메서드
    
    /// <summary>
    /// 카메라 전환 입력 처리
    /// </summary>
    /// <param name="direction">방향 값: -1(왼쪽), 0(중앙), 1(오른쪽)</param>
    private void HandleCameraSwitch(float direction)
    {
        // 전환 중이면 무시
        if (isTransitioning)
        {
            return;
        }
        
        // 빌드 모드 체크 - 빌드 모드가 활성화되어 있으면 카메라 전환 무시
        if (buildManager != null && buildManager.isBuildMode)
        {
            return;
        }
        
        // 중립 상태는 무시 (키 릴리즈 등)
        if (direction == 0)
        {
            return;
        }
        
        CameraPosition targetPosition = currentPosition;
        
        // 카메라 위치에 따라 다른 전환 로직 적용
        switch (currentPosition)
        {
            case CameraPosition.Center:
                // 센터 카메라에서는 a(-1)키는 왼쪽, d(1)키는 오른쪽으로 이동
                targetPosition = direction < 0 ? CameraPosition.Left : CameraPosition.Right;
                break;
                
            case CameraPosition.Left:
                // 왼쪽 카메라에서는 a(-1)키를 눌러야 센터로 이동
                if (direction < 0)
                {
                    targetPosition = CameraPosition.Center;
                }
                break;
                
            case CameraPosition.Right:
                // 오른쪽 카메라에서는 d(1)키를 눌러야 센터로 이동
                if (direction > 0)
                {
                    targetPosition = CameraPosition.Center;
                }
                break;
        }
        
        // 현재 위치와 타겟 위치가 다를 때만 전환
        if (targetPosition != currentPosition)
        {
            SwitchCamera(targetPosition);
        }
    }
    
    /// <summary>
    /// 지정된 위치로 카메라 전환
    /// </summary>
    private void SwitchCamera(CameraPosition position)
    {
        // 위치가 같으면 무시
        if (position == currentPosition) return;
        
        // 전환할 카메라 선택
        switch (position)
        {
            case CameraPosition.Left:
                targetCamera = leftCamera;
                break;
            case CameraPosition.Center:
                targetCamera = centerCamera;
                break;
            case CameraPosition.Right:
                targetCamera = rightCamera;
                break;
        }
        
        // 대상 카메라가 없으면 무시
        if (targetCamera == null) return;
        
        // 부드러운 전환 시작
        StartCoroutine(TransitionCamera());
        
        // 현재 위치 업데이트
        currentPosition = position;
        
        /*
        // 사운드 재생
        if (SoundManager.Instance)
        {
            SoundManager.Instance.PlaySound(cameraSwitchSoundName);
        }
        */
    }
    
    /// <summary>
    /// 카메라 전환 코루틴
    /// </summary>
    private IEnumerator TransitionCamera()
    {
        isTransitioning = true;
        
        // 대상 카메라 활성화
        targetCamera.gameObject.SetActive(true);
        SetCameraChildrenActive(targetCamera, true);
        
        // 페이드 인/아웃 효과 (필요시 구현)
        // ...
        
        // 짧은 대기
        yield return new WaitForSeconds(0.1f);
        
        // 이전 카메라 비활성화
        if (currentCamera != targetCamera)
        {
            currentCamera.gameObject.SetActive(false);
            SetCameraChildrenActive(currentCamera, false);
        }
        
        // 현재 카메라 업데이트
        currentCamera = targetCamera;
        
        // 카메라 변경 이벤트 발생
        if (EventManager.Instance != null)
        {
            string positionString = currentPosition.ToString();
            EventManager.Instance.CameraChangedEvent(currentCamera, positionString);
        }
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// 카메라 자식 컴포넌트 활성화/비활성화
    /// </summary>
    /// <param name="camera">대상 카메라</param>
    /// <param name="active">활성화 여부</param>
    private void SetCameraChildrenActive(Camera camera, bool active)
    {
        if (GameManager.Instance.gameStarted == false) return;
        
        // PlayerLookController와 ArrowShooter 컴포넌트 찾기
        PlayerLookController lookController = camera.GetComponentInChildren<PlayerLookController>();
        ArrowShooter arrowShooter = camera.GetComponentInChildren<ArrowShooter>();
        
        // 컴포넌트 활성화/비활성화
        if (lookController != null) lookController.enabled = active;
        if (arrowShooter != null) arrowShooter.enabled = active;
    }
    
    /// <summary>
    /// 현재 활성화된 카메라 반환 (BuildManager에서 호출)
    /// </summary>
    /// <returns>현재 활성화된 카메라</returns>
    public Camera GetCurrentCamera()
    {
        return currentCamera;
    }
    
    /// <summary>
    /// 모든 멀티뷰 카메라 비활성화 (BuildManager에서 호출)
    /// </summary>
    public void DisableAllCameras()
    {
        // 모든 카메라 비활성화
        if (leftCamera)
        {
            leftCamera.gameObject.SetActive(false);
            SetCameraChildrenActive(leftCamera, false);
        }
        
        if (centerCamera)
        {
            centerCamera.gameObject.SetActive(false);
            SetCameraChildrenActive(centerCamera, false);
        }
        
        if (rightCamera)
        {
            rightCamera.gameObject.SetActive(false);
            SetCameraChildrenActive(rightCamera, false);
        }
    }
    
    /// <summary>
    /// 빌드 모드 종료 후 이전 카메라 상태 복원 (BuildManager에서 호출)
    /// </summary>
    /// <param name="cameraToRestore">복원할 카메라</param>
    public void RestoreCameraState(Camera cameraToRestore)
    {
        if (cameraToRestore == null) return;
        
        // 복원할 카메라 활성화
        cameraToRestore.gameObject.SetActive(true);
        SetCameraChildrenActive(cameraToRestore, true);
        
        // 현재 카메라 업데이트
        currentCamera = cameraToRestore;
        
        // 현재 위치 업데이트
        if (cameraToRestore == leftCamera)
            currentPosition = CameraPosition.Left;
        else if (cameraToRestore == rightCamera)
            currentPosition = CameraPosition.Right;
        else
            currentPosition = CameraPosition.Center;
            
        // 카메라 변경 이벤트 발생
        if (EventManager.Instance != null)
        {
            string positionString = currentPosition.ToString();
            EventManager.Instance.CameraChangedEvent(currentCamera, positionString);
        }
    }
    
    /// <summary>
    /// 왼쪽 카메라로 전환
    /// </summary>
    public void SwitchToLeftCamera()
    {
        SetActiveCamera(leftCamera);
        currentPosition = CameraPosition.Left;
        
        // 카메라 변경 이벤트 발생
        if (EventManager.Instance != null)
        {
            EventManager.Instance.CameraChangedEvent(currentCamera, "Left");
        }
    }
    
    /// <summary>
    /// 중앙 카메라로 전환
    /// </summary>
    public void SwitchToCenterCamera()
    {
        SetActiveCamera(centerCamera);
        currentPosition = CameraPosition.Center;
        
        // 카메라 변경 이벤트 발생
        if (EventManager.Instance != null)
        {
            EventManager.Instance.CameraChangedEvent(currentCamera, "Center");
        }
    }
    
    /// <summary>
    /// 오른쪽 카메라로 전환
    /// </summary>
    public void SwitchToRightCamera()
    {
        SetActiveCamera(rightCamera);
        currentPosition = CameraPosition.Right;
        
        // 카메라 변경 이벤트 발생
        if (EventManager.Instance != null)
        {
            EventManager.Instance.CameraChangedEvent(currentCamera, "Right");
        }
    }
    
    /// <summary>
    /// 카메라 활성화
    /// </summary>
    /// <param name="camera">활성화할 카메라</param>
    private void SetActiveCamera(Camera camera)
    {
        if (camera == null) return;
        
        // 모든 카메라 비활성화
        DisableAllCameras();
        
        // 활성화할 카메라 활성화
        camera.gameObject.SetActive(true);
        SetCameraChildrenActive(camera, true);
        
        // 현재 카메라 업데이트
        currentCamera = camera;
    }
    
    /// <summary>
    /// 웨이브 종료 시 호출되는 메서드
    /// </summary>
    private void HandleWaveEnd(int waveNumber)
    {
        // 웨이브 종료시 카메라가 꺼지는 문제 해결
        // 현재 카메라 위치가 중앙이 아니면 중앙으로 변경
        if (currentPosition != CameraPosition.Center)
        {
            SwitchCamera(CameraPosition.Center);
        }
        else
        {
            // 모든 카메라가 꺼지는 경우 대비, 중앙 카메라가 활성화되어 있는지 확인
            if (!centerCamera.gameObject.activeSelf)
            {
                SetActiveCamera(centerCamera);
            }
        }
    }
    
    #endregion
} 