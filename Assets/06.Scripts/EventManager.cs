using System;
using UnityEngine;

// 옵저버 패턴을 사용 이벤트 구독 관리
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    
    public event Action OnFireStart;
    public event Action OnFireRelease;
    public event Action<Vector2> OnFireCharging;
    public event Action<Vector2> OnLookChanged;
    
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
    // 람다식을 사용해서 이벤트 발동 간소화
    public void FireStartEvent() => OnFireStart?.Invoke();
    public void FireReleaseEvent() => OnFireRelease?.Invoke();
    public void FireChargingEvent(Vector2 pos) => OnFireCharging?.Invoke(pos);
    public void LookChangedEvent(Vector2 lookDelta) => OnLookChanged?.Invoke(lookDelta);
}
