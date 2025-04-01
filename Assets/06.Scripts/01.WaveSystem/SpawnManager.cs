using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    #region 필드 변수

    [Header("참조")]
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private FormationManager formationManager;

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

    private void Update()
    {
        // VR 왼쪽 컨트롤러의 Y 버튼 입력 시 모든 적 강제 Kill
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            ForceKillAllEnemies();
        }
    }

    #endregion

    #region 스폰 관련 로직

    /// <summary>
    /// 적 스폰 메서드
    /// </summary>
    public void SpawnEnemy(EnemyType type)
    {
        Vector3 position = spawnCenter.position;

        // type에 따라 적절한 EnemyData 가져오기
        EnemyData data = GetEnemyDataByType(type);

        // 적 스폰
        Enemy e = enemyPool.GetEnemy(position, Quaternion.identity);
        e.target = targets[UnityEngine.Random.Range(0, targets.Length)];
        e.spawnManager = this;
        e.enemyData = data; // 적 데이터 설정
        e.formationManager = formationManager;

        // 적 타입에 따라 모델 활성화
        e.ActivateModelByType(type);

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

    /// <summary>
    /// 적이 죽었을 때 호출되는 메서드
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