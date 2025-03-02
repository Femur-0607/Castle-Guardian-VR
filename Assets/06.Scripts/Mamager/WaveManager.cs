using UnityEngine;

// 웨이브를 관리하는 스크립트
public class WaveManager : MonoBehaviour
{
    #region 필드 변수
    
    // 활성 SpawnManager들의 리스트 (인스펙터나 초기화 시 등록)
    [SerializeField] private SpawnManager[] spawnManagers;

    public int currentWave { get; private set; } = 1;   // 현재 웨이브
    public int enemyCountInWave { get; private set; } = 5;  // 웨이브 마다 스폰할 적 수
    private bool isWaveActive;  // 웨이브 진행 여부
    private float waveEndTime;

    #endregion

    #region 유니티 이벤트 함수

    void Update()
    {
        // Test용: 웨이브 종료 1초 후에 웨이브 시작 (웨이브 진행 중이 아닐 때)
        if (!isWaveActive && Time.time - waveEndTime >= 1f)
        {
            isWaveActive = true;
            WaveStartEvent();
        }
        
        // 전체 SpawnManager의 LiveEnemyCount 합이 0이면 웨이브 종료
        if (isWaveActive && GetTotalLiveEnemyCount() <= 0)
        {
            WaveEndEvent();
        }
    }

    #endregion

    #region 웨이브 관리

    /// <summary>
    /// 웨이브 시작 시 이벤트 매니저에 송신
    /// </summary>
    private void WaveStartEvent() => EventManager.Instance.WaveStartEvent(currentWave, enemyCountInWave);
    
    private int GetTotalLiveEnemyCount()
    {
        int total = 0;
        foreach(var sm in spawnManagers)
        {
            total += sm.LiveEnemyCount;
        }
        return total;
    }

    /// <summary>
    /// 웨이브 종료 시 발동하는 메서드
    /// (스폰 매니저에서 적이 모두 소멸했을 때 호출)
    /// </summary>
    public void WaveEndEvent()
    {
        isWaveActive = false;
        EventManager.Instance.WaveEndEvent(currentWave); // 이벤트 매니저에 웨이브 종료 송신
        currentWave++; // 웨이브 수 증가 
        waveEndTime = Time.time;
    }

    #endregion

}
