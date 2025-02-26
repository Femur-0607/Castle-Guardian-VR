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

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        fireAction = playerInput.actions["Fire"];
        chargingAction = playerInput.actions["Charging"];
        lookAction = playerInput.actions["Look"];

        fireAction.performed += ctx => EventManager.Instance.FireStartEvent();
        fireAction.canceled += ctx => EventManager.Instance.FireReleaseEvent();
        chargingAction.performed += ctx => EventManager.Instance.FireChargingEvent(ctx.ReadValue<Vector2>());
        lookAction.performed += ctx => EventManager.Instance.LookChangedEvent(ctx.ReadValue<Vector2>());
        lookAction.canceled += ctx => EventManager.Instance.LookChangedEvent(Vector2.zero);
    }

    private void OnDisable()
    {
        fireAction.performed -= ctx => EventManager.Instance.FireStartEvent();
        fireAction.canceled -= ctx => EventManager.Instance.FireReleaseEvent();
        chargingAction.performed -= ctx => EventManager.Instance.FireChargingEvent(ctx.ReadValue<Vector2>());
        lookAction.performed -= ctx => EventManager.Instance.LookChangedEvent(ctx.ReadValue<Vector2>());
    }
}
