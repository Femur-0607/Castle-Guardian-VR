using System;
using UnityEngine;

// 옵저버 패턴을 사용 이벤트 구독 관리
// 어떤 객체나 변수의 상태 변화에 관심이 있는 다른 곳(관찰자)에게 그 상태 변화를 알리기 위해 사용
// 송신자 = 메서드가 이벤트 발송 -> 이벤트 매니저 = 액션 델리게이트 발동, 전달 -> 수신자 = 구독한 이벤트 감지 후 메서드 작동
public class EventManager : MonoBehaviour
{
    #region 필드 변수

    // 다이얼로그 타입 열거형
    public enum DialogueType
    {
        Intro,
        Tutorial
    }

    // 초기화 순서가 중요한 경우 사용
    // - Instance가 먼저 설정되지 않으면 다른 스크립트에서 접근할 때 NullReferenceException 발생 가능
    // - 하지만 필요할 경우 다른 인스턴스로 교체할 수도 있음
    public static EventManager Instance { get; private set; }

    public event Action OnFireStart;                    // 조준 시작 시 발동할 이벤트
    public event Action OnFireCharging;                 // 조준 중 발동할 이벤트
    public event Action OnFireRelease;                  // 조준 해제 시 발동할 이벤트
    public event Action<Vector2> OnLookChanged;         // 시야 회전 시 발동할 이벤트
    public event Action<int> OnWaveStart;               // 웨이브 시작 시 발동할 이벤트
    public event Action<int> OnWaveEnd;                 // 웨이브 종료 시 발동할 이벤트
    public event Action OnGameStart;                    // 게임 시작 시 발동할 이벤트
    public event Action<bool> OnGameEnd;                // 게임 종료 시 발동할 이벤트 (승리 여부 포함)
    public event Action<int> OnMoneyChanged;            // 골드 변경 시 발동할 이벤트
    public event Action<Castle> OnCastleInitialized;    // 성문 초기화 시 발동할 이벤트
    public event Action<float> OnCastleHealthChanged;   // 성문 체력 변경 시 발동할 이벤트
    public event Action<float> OnCameraSwitch;          // 카메라 전환 시 발동할 이벤트 (방향값)
    public event Action<CameraController.CameraPosition> OnCameraChanged;   // 카메라가 실제로 변경되었을 때 발동할 이벤트 (위치)
    public event Action<DialogueType> OnDialogueStarted;   // 다이얼로그 시작 시 발동할 이벤트 (다이얼로그 타입 포함)
    public event Action<DialogueType> OnDialogueEnded;     // 다이얼로그 종료 시 발동할 이벤트 (다이얼로그 타입 포함)
    public event Action OnArrowCooldownStart;             // 화살 쿨타임 시작 시 발동할 이벤트
    public event Action OnArrowCooldownEnd;               // 화살 쿨타임 종료 시 발동할 이벤트
    public event Action OnGhostSpawn;              // 유령 스폰 이벤트
    public event Action OnGhostDestroy;            // 유령 제거 이벤트
    public event Action OnEnemyForceKill;          // 적 강제 제거 이벤트
    public event Action OnBuildNodeHit;  // 노드 건설/업그레이드 이벤트
    public event Action<int> OnLevelUp;            // 레벨업 이벤트

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

    #region 이벤트 발송 메서드

    // 람다식을 사용해서 이벤트 송신 간소화
    public void FireStartEvent() => OnFireStart?.Invoke();
    public void FireReleaseEvent() => OnFireRelease?.Invoke();
    public void FireChargingEvent() => OnFireCharging?.Invoke();
    public void LookChangedEvent(Vector2 lookDelta) => OnLookChanged?.Invoke(lookDelta);
    public void WaveStartEvent(int waveNumber) => OnWaveStart?.Invoke(waveNumber);
    public void WaveEndEvent(int waveNumber) => OnWaveEnd?.Invoke(waveNumber);
    public void GameStartEvent() => OnGameStart?.Invoke();
    public void GameEndEvent(bool isVictory) => OnGameEnd?.Invoke(isVictory);
    public void MoneyChangedEvent(int currentMoney) => OnMoneyChanged?.Invoke(currentMoney);
    public void TriggerOnCastleInitialized(Castle castle) => OnCastleInitialized?.Invoke(castle);
    public void TriggerOnCastleHealthChanged(float currentHealth) => OnCastleHealthChanged?.Invoke(currentHealth);
    public void CameraSwitchEvent(float direction) => OnCameraSwitch?.Invoke(direction);
    public void CameraChangedEvent(CameraController.CameraPosition position) => OnCameraChanged?.Invoke(position);
    public void DialogueStartedEvent(DialogueType type) => OnDialogueStarted?.Invoke(type);
    public void DialogueEndedEvent(DialogueType type) => OnDialogueEnded?.Invoke(type);
    public void ArrowCooldownStartEvent() => OnArrowCooldownStart?.Invoke();
    public void ArrowCooldownEndEvent() => OnArrowCooldownEnd?.Invoke();
    public void GhostSpawnEvent() => OnGhostSpawn?.Invoke();
    public void GhostDestroyEvent() => OnGhostDestroy?.Invoke();
    public void EnemyForceKillEvent() => OnEnemyForceKill?.Invoke();
    public void BuildNodeHitEvent() => OnBuildNodeHit?.Invoke();
    public void LevelUpEvent(int newLevel) => OnLevelUp?.Invoke(newLevel);
    
    #endregion
}
