using UnityEngine;
using UnityEngine.Pool;

public class EnemyPool : MonoBehaviour
{
    [Header("Enemy Prefab & Pool Settings")]
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private int defaultCapacity = 5;
    [SerializeField] private int maxSize = 30;

    private IObjectPool<Enemy> enemyPool;

    private void Awake()
    {
        enemyPool = new ObjectPool<Enemy>(
            createFunc: CreateEnemy,
            actionOnGet: OnTakeEnemy,
            actionOnRelease: OnReleaseEnemy,
            actionOnDestroy: OnDestroyEnemy,
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    // -------------------- ObjectPool 콜백 --------------------
    private Enemy CreateEnemy()
    {
        Enemy newEnemy = Instantiate(enemyPrefab);
        newEnemy.SetPoolReference(enemyPool);
        return newEnemy;
    }

    private void OnTakeEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(true);
    }

    private void OnReleaseEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void OnDestroyEnemy(Enemy enemy)
    {
        Destroy(enemy.gameObject);
    }

    // -------------------- 사용 편의 함수 --------------------
    /// <summary>
    /// 적을 풀에서 하나 꺼내 스폰 위치에 배치
    /// </summary>
    public Enemy GetEnemy(Vector3 spawnPos, Quaternion rotation)
    {
        Enemy e = enemyPool.Get();
        e.transform.SetPositionAndRotation(spawnPos, rotation);
        return e;
    }

    /// <summary>
    /// 외부(적 자체 등)에서 풀로 반환
    /// </summary>
    public void ReleaseEnemy(Enemy e)
    {
        enemyPool.Release(e);
    }
}