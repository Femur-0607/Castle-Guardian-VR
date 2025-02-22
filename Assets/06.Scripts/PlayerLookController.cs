using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLookController : MonoBehaviour
{
    [Header("입력 관련")]
    private Vector2 _lookDelta;             // 인풋 시스템으로 받는 인풋값
    private float _lookSensitivity = 20f;   // 민감도 조절 변수
    
    [Header("회전 관련")]
    public float pitchRange  = 10f;         // 회전제한 상,하 기준 ±70도
    public float yawRange  = 70f;           // 회전제한 좌,우 기준 ±70도
    
    // 현재 회전 값
    private float _currentPitch;
    private float _currentYaw;
    
    // 초기 회전 값을 저장
    private float _initialPitch;
    private float _initialYaw;
    
    [Header("참조")]
    private Transform _lookTransform;
    
    private void OnLook(InputValue value)
    {
        _lookDelta = value.Get<Vector2>();
    }

    private void Start()
    {
        _lookTransform = GetComponent<Transform>();
        
        // 민감도 제한
        _lookSensitivity = Mathf.Clamp(_lookSensitivity, 10f, 30f);
        
        // 초기 로테이션 값 저장 (초기 피치와 요 값)
        Vector3 initialEuler = _lookTransform.localRotation.eulerAngles;
        _initialPitch = initialEuler.x;
        _initialYaw = initialEuler.y;
        _currentPitch = _initialPitch;
        _currentYaw = _initialYaw;
    }

    private void Update()
    {
        UpdateLook();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            _lookSensitivity += 5f;
        }
        else if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            _lookSensitivity -= 5f;
        }
#endif
    }

    // 입력값에 따라 회전을 계산하고, 초기값 기준으로 제한하여 적용하는 메서드
    private void UpdateLook()
    {
        float deltaYaw = _lookDelta.x * _lookSensitivity * Time.deltaTime;
        float deltaPitch = _lookDelta.y * _lookSensitivity * Time.deltaTime;
        
        _currentYaw += deltaYaw;
        _currentPitch -= deltaPitch;        // 상하 회전은 반전시키는게 일반적 (마우스 위로 올리면 위를 보게)

        // 초기값 기준으로 clamping
        _currentYaw = Mathf.Clamp(_currentYaw, _initialYaw - yawRange, _initialYaw + yawRange);
        _currentPitch = Mathf.Clamp(_currentPitch, _initialPitch - pitchRange, _initialPitch + pitchRange);

        // 새로운 로테이션 적용
        _lookTransform.localRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0f);
    }
}
