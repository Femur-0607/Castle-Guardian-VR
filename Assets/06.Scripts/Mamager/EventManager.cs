using System;
using UnityEngine;

// 옵저버 패턴을 사용 이벤트 구독 관리
// 어떤 객체나 변수의 상태 변화에 관심이 있는 다른 곳(관찰자)에게 그 상태 변화를 알리기 위해 사용
// 송신자 = 메서드가 이벤트 발송 -> 이벤트 매니저 = 액션 델리게이트 발동, 전달 -> 수신자 = 구독한 이벤트 감지 후 메서드 작동
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    
    public event Action OnFireStart;    // 조준 시작 시 발동할 이벤트
    public event Action OnFireRelease;  // 조준 해제 시 발동할 이벤트
    public event Action<Vector2> OnFireCharging;    // 조준 중 발동할 이벤트
    public event Action<Vector2> OnLookChanged;     // 시야 회전 시 발동할 이벤트
    public event Action OnDeath; // 사망 시 발동할 이벤트
    
    // 싱글톤 패턴 사용
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    // 람다식을 사용해서 이벤트 송신 간소화
    public void FireStartEvent() => OnFireStart?.Invoke();
    public void FireReleaseEvent() => OnFireRelease?.Invoke();
    public void FireChargingEvent(Vector2 pos) => OnFireCharging?.Invoke(pos);
    public void LookChangedEvent(Vector2 lookDelta) => OnLookChanged?.Invoke(lookDelta);
    public void DeathEvent() => OnDeath?.Invoke();
}
