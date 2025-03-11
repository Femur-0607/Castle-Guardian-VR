using System;
using UnityEngine;

// 옵저버 패턴을 사용 이벤트 구독 관리
// 어떤 객체나 변수의 상태 변화에 관심이 있는 다른 곳(관찰자)에게 그 상태 변화를 알리기 위해 사용
// 송신자 = 메서드가 이벤트 발송 -> 이벤트 매니저 = 액션 델리게이트 발동, 전달 -> 수신자 = 구독한 이벤트 감지 후 메서드 작동
public class EventManager : MonoBehaviour
{
    #region 필드 변수

    // 초기화 순서가 중요한 경우 사용
    // - Instance가 먼저 설정되지 않으면 다른 스크립트에서 접근할 때 NullReferenceException 발생 가능
    // - 하지만 필요할 경우 다른 인스턴스로 교체할 수도 있음
    public static EventManager Instance { get; private set; }
    
    public event Action OnFireStart;                    // 조준 시작 시 발동할 이벤트
    public event Action OnFireRelease;                  // 조준 해제 시 발동할 이벤트
    public event Action<Vector2> OnFireCharging;        // 조준 중 발동할 이벤트
    public event Action<Vector2> OnLookChanged;         // 시야 회전 시 발동할 이벤트
    public event Action<int, int> OnWaveStart;          // 웨이브 시작 시 발동할 이벤트
    public event Action<int> OnWaveEnd;                 // 웨이브 종료 시 발동할 이벤트
    public event Action OnGameStart;                    // 게임 시작 시 발동할 이벤트
    public event Action<bool> OnGameEnd;                // 게임 종료 시 발동할 이벤트 (승리 여부 포함)
    public event Action<int> OnMoneyChanged;            // 골드 변경 시 발동할 이벤트
    
    #endregion

    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    // 람다식을 사용해서 이벤트 송신 간소화
    public void FireStartEvent() => OnFireStart?.Invoke();
    public void FireReleaseEvent() => OnFireRelease?.Invoke();
    public void FireChargingEvent(Vector2 pos) => OnFireCharging?.Invoke(pos);
    public void LookChangedEvent(Vector2 lookDelta) => OnLookChanged?.Invoke(lookDelta);
    public void WaveStartEvent(int wave, int enemyCount) => OnWaveStart?.Invoke(wave, enemyCount);
    public void WaveEndEvent(int wave) => OnWaveEnd?.Invoke(wave);
    public void GameStartEvent() => OnGameStart?.Invoke();
    public void GameEndEvent(bool isGameOver) => OnGameEnd?.Invoke(isGameOver);
    public void MoneyChangedEvent(int amount) => OnMoneyChanged?.Invoke(amount);
    
    
}
