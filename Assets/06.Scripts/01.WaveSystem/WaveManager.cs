using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// 웨이브를 관리하는 스크립트
public class WaveManager : MonoBehaviour
{
    #region 필드 변수

    [Header("웨이브 설정")]
    [SerializeField] private WaveData waveData;
    [SerializeField] private SpawnManager[] spawnManagers;  // 활성 SpawnManager들의 리스트 (인스펙터나 초기화 시 등록)

    private int currentWaveIndex = -1; // 현재 웨이브 인덱스
    public int CurrentWave => currentWaveIndex + 1; // 현재 웨이브 번호
    public int enemyCountInWave { get; private set; } // 현재 웨이브 적 수
    [SerializeField] private bool isWaveActive;  // 웨이브 진행 여부

    #endregion

    #region 유니티 이벤트 함수

    private void Start()
    {
        // 시작 시 추가 스폰 포인트 비활성화
        for (int i = 1; i < spawnManagers.Length; i++)
        {
            spawnManagers[i].gameObject.SetActive(false);
        }
    }

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
        if (isWaveActive)
        {
            int enemyCount = GetTotalLiveEnemyCount();

            if (enemyCount <= 0)
            {
                WaveEndEvent();
            }
        }
    }

    #endregion

    #region 웨이브 관리

    // 게임 시작 시 호출되는 메서드
    public void OnWaveStart()
    {
        currentWaveIndex = 0;
        StartWave(currentWaveIndex);
    }

    // 웨이브 종료 후 다음 웨이브 시작하는 메서드
    public void StartNextWaveEvent()
    {
        currentWaveIndex++;
        StartWave(currentWaveIndex);
    }

    // 웨이브 시작 시 호출되는 메서드
    private void StartWave(int waveIndex)
    {
        if (waveIndex >= waveData.waves.Count)
        {
            GameManager.Instance.EndGame(true);
            return;
        }

        WaveData.Wave currentWave = waveData.waves[waveIndex];

        // 총 적 수 계산 (GetTotalEnemyCount 메서드 대체)
        int totalEnemies = 0;
        foreach (var pattern in currentWave.spawnPatterns)
        {
            totalEnemies += pattern.count;
        }
        enemyCountInWave = totalEnemies;

        isWaveActive = true;

        // 스폰 포인트 활성화
        for (int i = 0; i < spawnManagers.Length; i++)
        {
            spawnManagers[i].gameObject.SetActive(i < currentWave.activeSpawnPoints);
        }

        SoundManager.Instance.PlaySound("BattleBGM");

        // 웨이브 시작 코루틴 실행
        StartCoroutine(ProcessWave(currentWave));
    }

    private IEnumerator ProcessWave(WaveData.Wave wave)
    {
        // 웨이브 시작
        EventManager.Instance.WaveStartEvent(CurrentWave, enemyCountInWave);

        // 각 스폰 패턴에 대한 코루틴 시작
        List<Coroutine> spawnCoroutines = new List<Coroutine>();

        foreach (var pattern in wave.spawnPatterns)
        {
            if (pattern.spawnPointIndex < 0 || pattern.spawnPointIndex >= spawnManagers.Length)
                continue;

            // 여기가 중요! SpawnEnemiesForPattern 코루틴 시작하고 리스트에 추가
            var coroutine = StartCoroutine(SpawnEnemiesForPattern(pattern));
            spawnCoroutines.Add(coroutine);
        }

        // 모든 스폰 코루틴 완료 대기
        foreach (var coroutine in spawnCoroutines)
        {
            yield return coroutine;
        }
    }

    // 웨이브 종료 시 발동하는 메서드
    // (스폰 매니저에서 적이 모두 소멸했을 때 호출)
    public void WaveEndEvent()
    {
        isWaveActive = false;

        SoundManager.Instance.PlaySound("MainBGM");

        // 웨이브 클리어 보상
        if (currentWaveIndex < waveData.waves.Count)
        {
            GameManager.Instance.AddMoney(waveData.waves[currentWaveIndex].clearReward);
        }

        EventManager.Instance.WaveEndEvent(CurrentWave); // 이벤트 매니저에 웨이브 종료 송신

        if (CurrentWave >= 10) // 10웨이브가 마지막
        {
            GameManager.Instance.EndGame(true);
        }
    }

    // 스폰매니저 전체의 에너미 카운트를 계속 체크하고있는 메서드
    private int GetTotalLiveEnemyCount()
    {
        int total = 0;
        foreach (var sm in spawnManagers)
        {
            if (sm.gameObject.activeSelf)
                total += sm.LiveEnemyCount;
        }
        return total;
    }
    
    private IEnumerator SpawnEnemiesForPattern(WaveData.SpawnPointPattern pattern)
    {
        SpawnManager targetSpawnManager = spawnManagers[pattern.spawnPointIndex];

        // 여기가 중요! pattern.count만큼 반복하여 적 생성
        for (int i = 0; i < pattern.count; i++)
        {
            targetSpawnManager.SpawnEnemy(pattern.enemyType);
            yield return new WaitForSeconds(pattern.spawnInterval);
        }
    }
    
    #endregion
}
