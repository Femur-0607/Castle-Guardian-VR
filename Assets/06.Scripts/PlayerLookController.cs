using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 시야 관련 인풋을 받았을때 시야가 어떻게 처리하는지에 대한 스크립트
public class PlayerLookController : MonoBehaviour
{
    #region 필드 변수

    [Header("입력 관련")]
    private Vector2 _lookDelta;             // 인풋 시스템으로 받는 인풋값
    private float _lookSensitivity = 1f;   // 민감도 조절 변수
    
    [Header("회전 관련")]
    public float pitchRange  = 20f;         // 회전제한 상,하 기준 ±20도
    public float yawRange  = 70f;           // 회전제한 좌,우 기준 ±70도
    // 현재 회전 값
    private float _currentPitch;
    private float _currentYaw;
    // 초기 회전 값을 저장
    private float _initialPitch;
    private float _initialYaw;
    
    [Header("참조")]
    private Transform _lookTransform;
    
    // Look 업데이트 실행 여부 (조준 중이면 false)
    private bool isLookEnabled = true;

    #endregion

    #region 유니티 이벤트 함수
    
    private void OnEnable()
    {
        EventManager.Instance.OnLookChanged += OnLookChanged;
        EventManager.Instance.OnFireStart += DisableLook;
        EventManager.Instance.OnFireRelease += EnableLook;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnLookChanged -= OnLookChanged;
        EventManager.Instance.OnFireStart -= DisableLook;
        EventManager.Instance.OnFireRelease -= EnableLook;
    }
    
    private void OnLookChanged(Vector2 lookDelta) =>  _lookDelta = lookDelta;
    private void DisableLook() => isLookEnabled = false;
    private void EnableLook() => isLookEnabled = true;
    
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

        // 유니티 에디터상에서 '[',']' 키 입력으로 민감도 조절
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
    
    #endregion
    
    // 입력값에 따라 회전을 계산하고, 초기값 기준으로 제한하여 적용하는 메서드
    private void UpdateLook()
    {
        // 조준 상태(화살 충전 중)라면 회전 로직 실행하지 않음
        if (!isLookEnabled)
            return;
        
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
