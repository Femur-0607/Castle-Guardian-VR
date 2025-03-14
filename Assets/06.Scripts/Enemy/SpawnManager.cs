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
    
    [Header("스폰 셋팅")]
    [SerializeField] private Transform spawnCenter;   // 적 스폰 기준 위치
    [SerializeField] private Transform[] targets;    // 스폰 시 타겟 할당
    
    // 현재 웨이브에서 활성화된 적 수를 관리하는 변수
    public int LiveEnemyCount { get; private set; }
    
    // ★ 활성화된 적을 추적하기 위한 리스트
    private List<Enemy> activeEnemies = new List<Enemy>();
    
    [Header("적 타입 설정")]
    [SerializeField] private EnemyData normalEnemyData;
    [SerializeField] private EnemyData archerEnemyData;
    [SerializeField] private EnemyData scoutEnemyData;
    [SerializeField] private EnemyData tankerEnemyData;

    [SerializeField] private WaveManager waveManager;
    
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
#if UNITY_EDITOR
            // 테스트용: F1 키 입력 시 모든 적 강제 Kill
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ForceKillAllEnemies();
            }
#endif
    }

    #endregion

    #region 스폰 관련 로직

    /// <summary>
    /// 웨이브 시작 이벤트 수신 핸들러
    /// </summary>
    /// <param name="wave">웨이브 번호</param>
    /// <param name="enemyCount">이번 웨이브에 생성할 적 수</param>
    private void HandleWaveStart(int wave, int enemyCount) => SpawnWave(enemyCount, spawnCenter.position);
    
    /// <summary>
    /// 지정된 적 수만큼 적을 스폰하는 메서드.
    /// </summary>
    public void SpawnWave(int enemyCount, Vector3 center)
    {
        LiveEnemyCount = 0; // 새로운 웨이브 시작 시 초기화
        activeEnemies.Clear(); // 새 웨이브 시작 시 초기화
        
        StartCoroutine(SpawnWaveCoroutine(enemyCount, center));
    }
    
    private IEnumerator SpawnWaveCoroutine(int totalEnemyCount, Vector3 center)
    {
        // 현재 웨이브에 맞는 구성을 WaveManager에서 가져오기
        WaveManager.WaveEnemyComposition currentWaveComp = waveManager.GetWaveComposition(waveManager.currentWave);
        
        if (currentWaveComp == null)
        {
            // 기본 구성: 모두 일반 적
            for (int i = 0; i < totalEnemyCount; i++)
            {
                SpawnEnemyOfType(EnemyType.Normal, center);
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            // 지정된 구성에 따라 스폰
            foreach (var enemyData in currentWaveComp.enemies)
            {
                for (int i = 0; i < enemyData.count; i++)
                {
                    SpawnEnemyOfType(enemyData.enemyType, center);
                    yield return new WaitForSeconds(1f);
                }
            }
        }
    }
    
    private void SpawnEnemyOfType(EnemyType type, Vector3 position)
    {
        // type에 따라 적절한 EnemyData 가져오기
        EnemyData data = GetEnemyDataByType(type);
        
        // 적 스폰
        Enemy e = enemyPool.GetEnemy(position, Quaternion.identity);
        e.target = targets[Random.Range(0, targets.Length)];
        e.spawnManager = this;
        e.enemyData = data; // 적 데이터 설정
        
        // 모델 교체 (적 타입에 따라)
        SwitchEnemyModel(e, data.enemyType);
        
        activeEnemies.Add(e);
        LiveEnemyCount++;
    }

    private EnemyData GetEnemyDataByType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Archer: return archerEnemyData;
            case EnemyType.Scout: return scoutEnemyData;
            case EnemyType.Tanker: return tankerEnemyData;
            default: return normalEnemyData;
        }
    }
    
    // 적 모델 교체
    private void SwitchEnemyModel(Enemy enemy, EnemyType type)
    {
        enemy.ActivateModelByType(type);
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