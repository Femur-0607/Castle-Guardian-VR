using UnityEngine;

// 인풋값을 관리하는 스크립트
// 추후 VR관련도 추가하기
public class InputManager : MonoBehaviour
{
    private float lastStickValue = 0f;  // 이전 조이스틱 입력값 저장

    private void Update()
    {
        // 왼쪽 조이스틱 입력 처리
        Vector2 leftStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
        
        // 조이스틱의 x축 값이 임계값을 넘으면 카메라 전환 이벤트 발생
        float threshold = 0.5f; // 조이스틱 감도 임계값
        float currentStickValue = 0f;

        if (Mathf.Abs(leftStick.x) > threshold)
        {
            // x값이 양수면 오른쪽(1), 음수면 왼쪽(-1)으로 변환
            currentStickValue = Mathf.Sign(leftStick.x);
        }

        // 이전 입력값과 현재 입력값이 다를 때만 이벤트 발생
        if (currentStickValue != lastStickValue)
        {
            EventManager.Instance.CameraSwitchEvent(currentStickValue);
            lastStickValue = currentStickValue;
        }
    }
}
