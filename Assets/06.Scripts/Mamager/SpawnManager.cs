using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    # region 필드 변수
    
    [Header("참조")]
    [SerializeField] private EnemyPool enemyPool;
    
    // 웨이브 매니저로부터 전달받을 웨이브 관련 정보로 스폰할 때 사용할 변수들
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnCenter;   // 적 스폰 기준 위치
    [SerializeField] private Transform[] targets;    // 스폰 시 타겟 할당
    // [SerializeField] private float spawnRadius = 2f; // 적 스폰 반경
    
    // 현재 웨이브에서 활성화된 적 수를 관리하는 변수
    public int LiveEnemyCount { get; private set; }
    
    // ★ 활성화된 적을 추적하기 위한 리스트
    private List<Enemy> activeEnemies = new List<Enemy>();
    
    #endregion

    #region 유니티 이벤트 함수

    private void OnEnable()
    {
        //  웨이브 시작 이벤트 구독
        EventManager.Instance.OnWaveStart += HandleWaveStart;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnWaveStart -= HandleWaveStart;
    }

    private void Update()
    {
        // 테스트용: F1 키 입력 시 모든 적 강제 Kill
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ForceKillAllEnemies();
        }
    }

    #endregion

    #region 스폰 관련 로직

    /// <summary>
    /// 웨이브 시작 이벤트 수신 핸들러
    /// </summary>
    /// <param name="wave">웨이브 번호</param>
    /// <param name="enemyCount">이번 웨이브에 생성할 적 수</param>
    private void HandleWaveStart(int wave, int enemyCount) => SpawnWave(enemyCount, spawnCenter.position); // SpawnWave 호출: spawnCenter와 spawnRadius를 사용해 랜덤 위치에서 스폰
    
    /// <summary>
    /// 지정된 적 수만큼 랜덤한 위치에서 적을 스폰하는 메서드.
    /// </summary>
    public void SpawnWave(int enemyCount, Vector3 center)
    {
        LiveEnemyCount = 0; // 새로운 웨이브 시작 시 초기화
        activeEnemies.Clear(); // 새 웨이브 시작 시 초기화
        
        StartCoroutine(SpawnWaveCoroutine(enemyCount, center));
    }
    
    private IEnumerator SpawnWaveCoroutine(int enemyCount, Vector3 center)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy(center);
            
            yield return new WaitForSeconds(1f); // 1초 간격으로 스폰, 필요에 따라 조정
        }
    }
    
    public void SpawnEnemy(Vector3 position)
    {
        NavMeshHit hit;
        float radius = 2f;
        // spawnPos 근처에서 NavMesh 위의 점 찾기
        if (NavMesh.SamplePosition(position, out hit, radius, NavMesh.AllAreas))
        {
            // 찾은 NavMesh 지점의 y값으로 원래 위치의 y값을 보정합니다.
            Vector3 adjustedPos = position;
            adjustedPos.y = hit.position.y;
            position = adjustedPos;
        }

        // 이제 spawnPos는 NavMesh 위의 좌표이므로 안전하게 에이전트를 사용할 수 있음
        
        Enemy e = enemyPool.GetEnemy(position, Quaternion.identity);    // enemyPool에서 적 하나를 꺼내 스폰
        e.target = targets[Random.Range(0, targets.Length)];    // 랜덤한 타겟 할당
        e.spawnManager = this;  // Enemy의 변수 spawnManager에 생성한 스폰매니저를 할당 (다이 메서드 실행시 보고용)
        activeEnemies.Add(e);

        LiveEnemyCount++;
    }
    
    /// <summary>
    /// 적이 죽었을 때 호출하여 활성 적 수를 감소시키고, 모두 소멸 시 웨이브 종료를 알림.
    /// (Enemy 스크립트의 Die()나 죽음 이벤트에서 직접 호출)
    /// </summary>
    public void EnemyDied(Enemy enemy)
    {
        LiveEnemyCount--;
        activeEnemies.Remove(enemy);
    }
    
    #endregion
    
    // ★ 테스트용: 모든 적에게 큰 데미지를 줘서 강제 Kill
    private void ForceKillAllEnemies()
    {
        // activeEnemies 리스트 복사본을 만들어 순회(동적 제거 방지)
        foreach (Enemy enemy in activeEnemies.ToArray())
        {
            enemy.TakeDamage(999999f);
        }
    } 
}