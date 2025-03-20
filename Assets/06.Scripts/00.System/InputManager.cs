using UnityEngine;
using UnityEngine.InputSystem;

// 인풋값을 관리하는 스크립트
// 추후 VR관련도 추가하기
public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction fireAction;
    private InputAction chargingAction;
    private InputAction lookAction;
    private InputAction cameraSwitchAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        fireAction = playerInput.actions["Fire"];
        chargingAction = playerInput.actions["Charging"];
        lookAction = playerInput.actions["Look"];
        cameraSwitchAction = playerInput.actions["CameraSwitch"];

        fireAction.performed += ctx => EventManager.Instance.FireStartEvent();
        fireAction.canceled += ctx => EventManager.Instance.FireReleaseEvent();
        chargingAction.performed += ctx => EventManager.Instance.FireChargingEvent(ctx.ReadValue<Vector2>());
        lookAction.performed += ctx => EventManager.Instance.LookChangedEvent(ctx.ReadValue<Vector2>());
        lookAction.canceled += ctx => EventManager.Instance.LookChangedEvent(Vector2.zero);
        cameraSwitchAction.performed += ctx => EventManager.Instance.CameraSwitchEvent(ctx.ReadValue<float>());
        cameraSwitchAction.canceled += ctx => EventManager.Instance.CameraSwitchEvent(0f);
    }
    
    private void OnDisable()
    {
        fireAction.performed -= ctx => EventManager.Instance.FireStartEvent();
        fireAction.canceled -= ctx => EventManager.Instance.FireReleaseEvent();
        chargingAction.performed -= ctx => EventManager.Instance.FireChargingEvent(ctx.ReadValue<Vector2>());
        lookAction.performed -= ctx => EventManager.Instance.LookChangedEvent(ctx.ReadValue<Vector2>());
        lookAction.canceled -= ctx => EventManager.Instance.LookChangedEvent(Vector2.zero);
        cameraSwitchAction.performed -= ctx => EventManager.Instance.CameraSwitchEvent(ctx.ReadValue<float>());
        cameraSwitchAction.canceled -= ctx => EventManager.Instance.CameraSwitchEvent(0f);
    }
    
    // Update 메서드 추가 - 기존 Input 시스템 사용
    private void Update()
    {
#if UNITY_EDITOR
        // F2 키를 눌렀을 때 테스트 모드 시작
        if (Input.GetKeyDown(KeyCode.F2))
        {
            StartTestMode();
        }
#endif
    }
    
    // 테스트 모드 시작 메서드 (F2 키로 실행)
    private void StartTestMode()
    {
#if UNITY_EDITOR
        // 웨이브 매니저 찾기
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
        {
            // 게임 시작 처리
            if (!GameManager.Instance.gameStarted)
            {
                GameManager.Instance.StartGame();
            }
            
            // 플레이어 컨트롤 활성화
            GameManager.Instance.EnablePlayerControlsForTest();
            
            // 웨이브 시작 (기존의 OnWaveStart 메서드 호출)
            waveManager.OnWaveStart();
            
            Debug.Log("[테스트 모드] 다이얼로그 건너뛰고 1웨이브 시작");
        }
#endif
    }
}
