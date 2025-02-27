using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Pool References")]
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private EnemyPool enemyPool;

    // --------------- 화살 스폰/발사 ---------------
    public void SpawnProjectile(Vector3 position, Quaternion rotation, Vector3 velocity)
    {
        // projectilePool에서 화살 하나 Get
        projectilePool.GetProjectile(position, rotation, velocity);
    }

    // --------------- 적 스폰 ---------------
    public void SpawnEnemy(Vector3 position)
    {
        // enemyPool에서 적 하나 Get
        enemyPool.GetEnemy(position, Quaternion.identity);
    }

    // (옵션) 웨이브 시스템 예시
    public void SpawnWave(int enemyCount, Vector3 spawnCenter)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 randPos = spawnCenter + Random.insideUnitSphere * 2f;
            randPos.y = spawnCenter.y;
            SpawnEnemy(randPos);
        }
    }
}