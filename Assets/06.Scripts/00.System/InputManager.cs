using UnityEngine;

// 인풋값을 관리하는 스크립트
// 추후 VR관련도 추가하기
public class InputManager : MonoBehaviour
{
    #region 필드변수

    private bool wasTriggerPressed = false;    // 이전 프레임의 트리거 상태
    private const float STICK_THRESHOLD = 0.5f; // 조이스틱 감도 임계값
    private bool stickInputProcessed = false;  // 조이스틱 입력이 처리되었는지 여부
    private bool arrowStickInputProcessed = false;  // 화살 전환용 조이스틱 입력이 처리되었는지 여부
    private bool leftStickWasNeutral = true;  // 이전 프레임에서 왼쪽 스틱이 중립이었는지 여부

    #endregion

    #region 유니티 이벤트 함수

    private void Update()
    {
        // 오른쪽 스틱 값 직접 확인
        Vector2 rightStickDebug = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        Debug.Log($"Update에서 직접 확인한 오른쪽 스틱 값: X={rightStickDebug.x}, Y={rightStickDebug.y}");
        
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
        float stickX = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch).x;

        bool isNeutral = Mathf.Abs(stickX) < STICK_THRESHOLD;

        // 빌드 모드일 때는 노드 선택 처리
        if (BuildManager.instance.isBuildMode)
        {
            // 중립에서 벗어난 순간에만 신호 전송
            if (!isNeutral && leftStickWasNeutral)
            {
                int direction = stickX > 0 ? 1 : -1;
                BuildManager.instance.MoveNodeSelection(direction);
            }
        }
        // 일반 모드일 때는 카메라 전환 처리
        else
        {
            // 중립에서 벗어난 순간에만 신호 전송
            if (!isNeutral && leftStickWasNeutral)
            {
                float direction = Mathf.Sign(stickX);
                EventManager.Instance.CameraSwitchEvent(direction);
            }
        }

        leftStickWasNeutral = isNeutral;
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
            EventManager.Instance.GhostDestroyEvent();
            EventManager.Instance.EnemyForceKillEvent();
        }
    }

    /// <summary>
    /// 왼쪽 범퍼 입력 처리 (노드 건설/업그레이드)
    /// </summary>
    private void HandleLeftBumper()
    {
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
        float stickX = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick, OVRInput.Controller.RTouch).x;
        Debug.Log($"오른쪽 스틱 X값: {stickX}");

        bool isNeutral = Mathf.Abs(stickX) < STICK_THRESHOLD;

        // 중립에서 벗어난 순간에만 신호 전송
        if (!isNeutral && leftStickWasNeutral)
        {
            Debug.Log($"스틱 입력 감지: {(stickX > 0 ? "오른쪽" : "왼쪽")}");
            if (stickX > 0)
            {
                ArrowManager.Instance.CycleNextArrow();
            }
            else
            {
                ArrowManager.Instance.CyclePreviousArrow();
            }
        }

        leftStickWasNeutral = isNeutral;
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
