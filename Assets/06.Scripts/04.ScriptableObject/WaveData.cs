using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "WaveData", menuName = "Wave/WaveData")]
public class WaveData : ScriptableObject
{

    [System.Serializable]
    public class Wave
    {
        [Header("웨이브 기본 정보")]
        public string waveName;  // 예: "약한 적 무리", "궁수 혼합", "탱커 웨이브" 등
        
        [Header("웨이브 구성")]
        [Tooltip("이 웨이브에서 사용할 스폰 포인트 개수 (1-3)")]
        [Range(1, 3)] public int activeSpawnPoints = 1;

        [Header("스폰 패턴")]
        [Tooltip("각 스폰 포인트별 스폰 패턴")]
        public List<SpawnPointPattern> spawnPatterns;

        [Header("웨이브 설정")]
        public int clearReward = 100;
    }

    public List<Wave> waves;

    [System.Serializable]
    public class SpawnPointPattern
    {
        [Tooltip("어느 스폰 포인트에서 스폰할지 (0, 1, 2)")]
        public int spawnPointIndex;
        [Tooltip("스폰할 적 타입")]
        public EnemyType enemyType;
        [Tooltip("해당 적 타입의 수")]
        public int count;
        [Tooltip("스폰 간격 (초)")]
        public float spawnInterval = 1f;
    }
}
