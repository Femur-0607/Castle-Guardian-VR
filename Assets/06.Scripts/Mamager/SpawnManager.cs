using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private WaveManager waveManager;
    
    // 현재 웨이브에서 활성화된 적 수를 관리하는 변수
    private int liveEnemyCount = 0;

    // --------------- 적 스폰 --------------ㅎ
    public void SpawnEnemy(Vector3 position)
    {
        // enemyPool에서 적 하나 Get
        enemyPool.GetEnemy(position, Quaternion.identity);
        liveEnemyCount++; // 스폰 시 카운트 증가
    }

    /// <summary>
    /// 지정된 적 수만큼 랜덤한 위치에서 적을 스폰하는 메서드.
    /// </summary>
    public void SpawnWave(int enemyCount, Vector3 spawnCenter)
    {
        liveEnemyCount = 0; // 새로운 웨이브 시작 시 초기화
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 randPos = spawnCenter + Random.insideUnitSphere * 2f;
            randPos.y = spawnCenter.y;
            SpawnEnemy(randPos);
        }
    }
    
    /// <summary>
    /// 적이 죽었을 때 호출하여 활성 적 수를 감소시키고, 모두 소멸 시 웨이브 종료를 알림.
    /// (Enemy 스크립트의 Die()나 죽음 이벤트에서 직접 호출)
    /// </summary>
    public void EnemyDied()
    {
        liveEnemyCount--;
        if (liveEnemyCount <= 0)
        {
            // 모든 적 소멸: 웨이브 매니저에 직접 알림
            waveManager?.WaveEndEvent();
        }
    }
}