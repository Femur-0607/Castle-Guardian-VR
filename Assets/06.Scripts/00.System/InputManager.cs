using UnityEngine;

// 인풋값을 관리하는 스크립트
// 추후 VR관련도 추가하기
public class InputManager : MonoBehaviour
{
    #region 필드변수

    private float lastStickValue = 0f;         // 이전 조이스틱 입력값 저장 (카메라용)
    private float lastArrowStickValue = 0f;    // 이전 조이스틱 입력값 저장 (화살용)
    private bool wasTriggerPressed = false;    // 이전 프레임의 트리거 상태
    private const float STICK_THRESHOLD = 0.5f; // 조이스틱 감도 임계값

    #endregion

    #region 유니티 이벤트 함수

    private void Update()
    {
        HandleLeftControllerInput();   // 왼쪽 컨트롤러 입력 처리
        HandleRightControllerInput();  // 오른쪽 컨트롤러 입력 처리
    }

    #endregion

    #region 왼쪽 컨트롤러 입력 처리 로직

    /// <summary>
    /// 왼쪽 컨트롤러 입력 처리
    /// </summary>
    private void HandleLeftControllerInput()
    {
        HandleLeftStick();        // 조이스틱 (카메라 전환)
        HandleLeftButtons();      // X,Y 버튼 (고스트/적 관련)
        HandleLeftBumper();       // 범퍼 (노드 건설/업그레이드)
    }

    /// <summary>
    /// 왼쪽 스틱 입력 처리 (카메라 전환)
    /// </summary>
    private void HandleLeftStick()
    {
        Vector2 leftStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
        float currentStickValue = 0f;

        if (Mathf.Abs(leftStick.x) > STICK_THRESHOLD)
        {
            currentStickValue = Mathf.Sign(leftStick.x);
        }

        if (currentStickValue != lastStickValue)
        {
            EventManager.Instance.CameraSwitchEvent(currentStickValue);
            lastStickValue = currentStickValue;
        }
    }

    /// <summary>
    /// 왼쪽 버튼 입력 처리 (고스트/적 관련)
    /// </summary>
    private void HandleLeftButtons()
    {
        // X 버튼 - 고스트 스폰
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            EventManager.Instance.GhostSpawnEvent();
        }

        // Y 버튼 - 고스트/적 제거
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            Debug.Log("Y 버튼 눌림");
            EventManager.Instance.GhostDestroyEvent();
            EventManager.Instance.EnemyForceKillEvent();
        }
    }

    /// <summary>
    /// 왼쪽 범퍼 입력 처리 (노드 건설/업그레이드)
    /// </summary>
    private void HandleLeftBumper()
    {
        Debug.Log("LeftBumper");
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
        {
            EventManager.Instance.BuildNodeHitEvent();
        }
    }

    #endregion

    #region 오른쪽 컨트롤러 입력 처리 로직

    /// <summary>
    /// 오른쪽 컨트롤러 입력 처리
    /// </summary>
    private void HandleRightControllerInput()
    {
        HandleArrowSwitch();     // 화살 전환
        HandleTriggerInput();    // 화살 발사
    }

    /// <summary>
    /// 오른쪽 조이스틱으로 화살 전환
    /// </summary>
    private void HandleArrowSwitch()
    {
        Vector2 rightStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.RTouch);
        float currentArrowStickValue = 0f;

        if (Mathf.Abs(rightStick.x) > STICK_THRESHOLD)
        {
            currentArrowStickValue = Mathf.Sign(rightStick.x);
        }

        if (currentArrowStickValue != lastArrowStickValue)
        {
            if (currentArrowStickValue > 0)
            {
                ArrowManager.Instance.CycleNextArrow();
            }
            else if (currentArrowStickValue < 0)
            {
                ArrowManager.Instance.CyclePreviousArrow();
            }
            lastArrowStickValue = currentArrowStickValue;
        }
    }

    /// <summary>
    /// 오른쪽 트리거 입력 처리 (화살 발사)
    /// </summary>
    private void HandleTriggerInput()
    {
        bool isTriggerPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        
        if (isTriggerPressed && !wasTriggerPressed)
        {
            EventManager.Instance.FireStartEvent();
        }
        else if (isTriggerPressed)
        {
            EventManager.Instance.FireChargingEvent();
        }
        else if (!isTriggerPressed && wasTriggerPressed)
        {
            EventManager.Instance.FireReleaseEvent();
        }

        wasTriggerPressed = isTriggerPressed;
    }

    #endregion
}
