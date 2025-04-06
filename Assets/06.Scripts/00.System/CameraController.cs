using System.Collections;
using UnityEngine;

/// <summary>
/// VR 환경에서 카메라 시점을 전환하는 스크립트
/// </summary>
public class CameraController : MonoBehaviour
{
    #region 필드 변수

    [Header("VR 카메라 설정")]
    public GameObject ovrCameraRig;    // OVR 카메라 리그
    [SerializeField] private OVRScreenFade screenFade;   // OVR 스크린 페이드

    [Header("카메라 위치 설정")]
    [SerializeField] private Transform centerPosition;   // 중앙 위치
    [SerializeField] private Transform leftPosition;     // 왼쪽 위치
    [SerializeField] private Transform rightPosition;    // 오른쪽 위치
    [SerializeField] private Transform buildPosition;    // 빌드 모드 위치
    [SerializeField] private Transform uiPosition;       // UI 카메라 위치

    [Header("페이드 설정")]
    [SerializeField] private float fadeTime = 0.3f;      // 페이드 시간

    // 빌드 모드 참조
    [SerializeField] private BuildManager buildManager;
    
    public enum CameraPosition { Left, Center, Right, UI, Build }
    private CameraPosition currentPosition = CameraPosition.Center;
    
    private bool isTransitioning = false;

    #endregion

    #region 유니티 이벤트 함수

    private void Awake()
    {
        // 초기 위치 설정 (UI 모드)
        if (ovrCameraRig != null && uiPosition != null)
        {
            ovrCameraRig.transform.position = uiPosition.position;
            ovrCameraRig.transform.rotation = uiPosition.rotation;
        }
    }
    
    private void OnEnable()
    {
        EventManager.Instance.OnCameraSwitch += HandleCameraSwitch;
        EventManager.Instance.OnWaveEnd += HandleWaveEnd;
        EventManager.Instance.OnDialogueStarted += HandleDialogueStarted;
        EventManager.Instance.OnDialogueEnded += HandleDialogueEnded;
        EventManager.Instance.OnGameStart += HandleGameStart;
    }
    
    private void OnDisable()
    {
        EventManager.Instance.OnCameraSwitch -= HandleCameraSwitch;
        EventManager.Instance.OnWaveEnd -= HandleWaveEnd;
        EventManager.Instance.OnDialogueStarted -= HandleDialogueStarted;
        EventManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
        EventManager.Instance.OnGameStart -= HandleGameStart;
    }

    #endregion

    #region 이벤트 핸들러

    private void HandleDialogueStarted(EventManager.DialogueType type) => SwitchCamera(CameraPosition.UI);
    
    private void HandleDialogueEnded(EventManager.DialogueType type) => SwitchCamera(CameraPosition.Center);
    
    private void HandleGameStart() => SwitchCamera(CameraPosition.Center);
    
    private void HandleWaveEnd(int waveNumber) => SwitchCamera(CameraPosition.UI);

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
        
        // UI 모드나 빌드 모드에서는 카메라 전환 무시
        if (currentPosition == CameraPosition.UI || currentPosition == CameraPosition.Build)
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
    public void SwitchCamera(CameraPosition position)
    {
        // 위치가 같으면 무시
        if (position == currentPosition) return;
        
        Transform targetTransform = null;
        switch (position)
        {
            case CameraPosition.Left:
                targetTransform = leftPosition;
                break;
            case CameraPosition.Center:
                targetTransform = centerPosition;
                break;
            case CameraPosition.Right:
                targetTransform = rightPosition;
                break;
            case CameraPosition.UI:
                targetTransform = uiPosition;
                break;
            case CameraPosition.Build:
                targetTransform = buildPosition;
                break;
        }
        
        // 대상 카메라 위치가 없으면 무시
        if (targetTransform == null) return;
        
        // 부드러운 전환 시작
        StartCoroutine(TransitionCamera(targetTransform));
        
        // 현재 위치 업데이트
        currentPosition = position;
    }
    
    /// <summary>
    /// 카메라 전환 코루틴
    /// </summary>
    private IEnumerator TransitionCamera(Transform targetTransform)
    {
        isTransitioning = true;
        
        // 페이드 아웃
        if (screenFade != null)
        {
            screenFade.FadeOut();
            yield return new WaitForSeconds(fadeTime);
        }
        
        // 즉시 위치 이동
        ovrCameraRig.transform.position = targetTransform.position;
        ovrCameraRig.transform.rotation = targetTransform.rotation;
        
        // 페이드 인
        if (screenFade != null)
        {
            screenFade.FadeIn();
            yield return new WaitForSeconds(fadeTime);
        }
        
        isTransitioning = false;
        
        // 카메라 변경 이벤트 발생
        if (EventManager.Instance != null)
        {
            EventManager.Instance.CameraChangedEvent(currentPosition);
        }
    }

    #endregion
} 