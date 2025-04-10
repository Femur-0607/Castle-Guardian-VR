using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    #region 필드 변수

    [Header("웨이브 설정")]
    [SerializeField] private WaveData waveData;
    [SerializeField] private SpawnManager[] spawnManagers;  // 활성 SpawnManager들의 리스트 (인스펙터나 초기화 시 등록)

    private int currentWaveIndex = -1; // 현재 웨이브 인덱스
    private int CurrentWave => currentWaveIndex + 1; // 현재 웨이브 번호
    public int enemyCountInWave { get; private set; } // 현재 웨이브 적 수
    [SerializeField] private bool isWaveActive;  // 웨이브 진행 여부
    private bool isDialogueActive = false;       // 다이얼로그 표시 중인지 여부

    [SerializeField] private float soulMoveDuration = 3.0f; // 영혼 수집 시간

   public Transform cameraRig;

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
        EventManager.Instance.OnDialogueStarted += HandleDialogueStarted;
        EventManager.Instance.OnDialogueEnded += HandleDialogueEnded;
        EventManager.Instance.OnEnemyForceKill += ForceKillAllEnemies;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnGameStart -= OnWaveStart;
        EventManager.Instance.OnDialogueStarted -= HandleDialogueStarted;
        EventManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
        EventManager.Instance.OnEnemyForceKill -= ForceKillAllEnemies;
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
    private void OnWaveStart()
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
        EventManager.Instance.WaveStartEvent(CurrentWave);

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
    private void WaveEndEvent()
    {
        isWaveActive = false;

        // 웨이브 클리어 보상
        if (currentWaveIndex < waveData.waves.Count)
        {
            GameManager.Instance.AddMoney(waveData.waves[currentWaveIndex].clearReward);
        }

        // 영혼 수집 후 1초 대기 후 웨이브 종료 이벤트 호출
        CollectSoulsToPlayer();
    }

    private void CollectSoulsToPlayer()
    {
        if (cameraRig == null)
        {
            return;
        }
        
        // 모든 남아있는 영혼을 플레이어에게 이동
        ParticlePoolManager.Instance.CollectAllSouls(cameraRig, soulMoveDuration, () => {
            // 경험치 증가 애니메이션이 끝나고 1초 후에 웨이브 종료 이벤트 발생
            StartCoroutine(DelayedWaveEndEvent());
        });
    }

    private IEnumerator DelayedWaveEndEvent()
    {
        // 경험치 애니메이션과 효과음을 위한 1초 대기
        yield return new WaitForSeconds(1f);

        SoundManager.Instance.PlaySound("MainBGM");
        
        // 웨이브 종료 이벤트 발생
        EventManager.Instance.WaveEndEvent(CurrentWave);
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
    
    // 다이얼로그 시작 시 호출
    private void HandleDialogueStarted(EventManager.DialogueType type)
    {
        if (type == EventManager.DialogueType.Tutorial || type == EventManager.DialogueType.SpawnPointAdded)
        {
            isDialogueActive = true;
        }
    }

    // 다이얼로그 종료 시 호출
    private void HandleDialogueEnded(EventManager.DialogueType type)
    {
        if (type == EventManager.DialogueType.Tutorial || type == EventManager.DialogueType.SpawnPointAdded)
        {
            isDialogueActive = false;
        }
    }

    private IEnumerator SpawnEnemiesForPattern(WaveData.SpawnPointPattern pattern)
    {
        SpawnManager targetSpawnManager = spawnManagers[pattern.spawnPointIndex];

        // 여기가 중요! pattern.count만큼 반복하여 적 생성
        for (int i = 0; i < pattern.count; i++)
        {
            // 다이얼로그 중에는 스폰 일시 중지
            while (isDialogueActive && GameManager.Instance.gameStarted)
            {
                yield return null; // 다이얼로그가 끝날 때까지 대기
            }
            
            // 스폰 진행
            targetSpawnManager.SpawnEnemy(pattern.enemyType);
            yield return new WaitForSeconds(pattern.spawnInterval);
        }
    }

    private void ForceKillAllEnemies()
    {
        foreach (var spawnManager in spawnManagers)
        {
            if (spawnManager != null)
            {
                spawnManager.ForceKillAllEnemies();
            }
        }
    }
    
    #endregion
}
