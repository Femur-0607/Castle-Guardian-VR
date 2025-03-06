using UnityEngine;

// 웨이브를 관리하는 스크립트
public class WaveManager : MonoBehaviour
{
    #region 필드 변수
    
    // 활성 SpawnManager들의 리스트 (인스펙터나 초기화 시 등록)
    [SerializeField] private SpawnManager[] spawnManagers;

    public int currentWave { get; private set; }   // 현재 웨이브
    public int enemyCountInWave { get; private set; } // 웨이브 마다 스폰할 적 수
    private bool isWaveActive;  // 웨이브 진행 여부

    #endregion

    #region 유니티 이벤트 함수
    
    private void OnEnable()
    {
        EventManager.Instance.OnGameStart += OnWaveStart;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnGameStart -= OnWaveStart;
    }

    void Update()
    {
        // 전체 SpawnManager의 LiveEnemyCount 합이 0이면 웨이브 종료
        if (isWaveActive && GetTotalLiveEnemyCount() <= 0)
        {
            WaveEndEvent();
        }
    }

    #endregion

    #region 웨이브 관리

    // 게임 시작 시 호출되는 메서드
    public void OnWaveStart()
    {
        currentWave = 1;
        enemyCountInWave = 5;
        isWaveActive = true;
        EventManager.Instance.WaveStartEvent(currentWave, enemyCountInWave);
    }
    
    /// <summary>
    /// 웨이브 시작 시 발동하는 메서드
    /// </summary>
    public void StartNextWaveEvent()
    {
        currentWave++;
        // 웨이브가 높아질수록 적 수 증가 (예시: 매 웨이브마다 2씩 증가)
        enemyCountInWave = 5 + (currentWave - 1) * 2;
        
        isWaveActive = true;
        
        // 이벤트 매니저에게 웨이브 시작 송신(웨이브 번호, 적의 수)
        EventManager.Instance.WaveStartEvent(currentWave, enemyCountInWave);
    }

    /// <summary>
    /// 웨이브 종료 시 발동하는 메서드
    /// (스폰 매니저에서 적이 모두 소멸했을 때 호출)
    /// </summary>
    public void WaveEndEvent()
    {
        isWaveActive = false;
        
        EventManager.Instance.WaveEndEvent(currentWave); // 이벤트 매니저에 웨이브 종료 송신
        
        if (currentWave >= 10) // 예시: 10웨이브가 최종 웨이브라면
        {
            GameManager.Instance.EndGame(true); // 승리로 게임 종료
        }
    }

    /// <summary>
    /// 스폰매니저 전체의 에너미 카운트를 계속 체크하고있는 메서드
    /// </summary>
    /// <returns></returns>
    private int GetTotalLiveEnemyCount()
    {
        int total = 0;
        foreach(var sm in spawnManagers)
        {
            total += sm.LiveEnemyCount;
        }
        return total;
    }
    
    #endregion
}
