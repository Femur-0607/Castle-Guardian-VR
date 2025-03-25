using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class ParticlePool : MonoBehaviour
{
    public static class ParticleKeys
    {
        // 영혼 관련
        public const string DEATH_POOF = "Loot_Poof_Variant";
        public const string SOUL_BEAM = "LootBeam_Generic_Epic_Variant";
        public const string SOUL_TRAIL = "VFX_Trail_Void_Variant";

        // 화살 관련
        public const string NORMAL_ARROW_HIT = "NormalArrowHit";
        public const string EXPLOSIVE_ARROW_HIT = "ExplosiveArrowHit";
        public const string POISON_ARROW_HIT = "PoisonArrowHit";
        public const string NORMAL_MUZZLE = "NormalMuzzle";
        public const string EXPLOSIVE_MUZZLE = "ExplosiveMuzzle";
        public const string POISON_MUZZLE = "PoisonMuzzle";
    }

    // 머즐 이펙트 enum 타입 (인덱스 쉽게 접근용)
    public enum MuzzleType
    {
        Normal = 0,
        Explosive = 1,
        Poison = 2
    }

    // 머즐 이펙트 프리팹 배열
    [Header("머즐 이펙트 프리팹 배열")]
    [SerializeField] private GameObject[] muzzleEffectPrefabs = new GameObject[3]; // 0: 일반, 1: 폭발, 2: 독

    #region 싱글톤
    private static ParticlePool _instance;
    public static ParticlePool Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<ParticlePool>();
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }
    #endregion

    [System.Serializable]
    public class ParticlePoolItem
    {
        public GameObject prefab;           // 파티클 프리팹
        public string key;                 // 파티클 키
        public int defaultCapacity = 10;    // 풀 기본 크기
        public int maxPoolSize = 20;        // 풀 최대 크기
        public bool autoReturn = true;      // 자동 반환 여부
    }

    [SerializeField] private List<ParticlePoolItem> particleTypes = new List<ParticlePoolItem>();
    private Dictionary<string, IObjectPool<GameObject>> particlePools = new Dictionary<string, IObjectPool<GameObject>>();
    private Dictionary<string, ParticlePoolItem> poolConfigs = new Dictionary<string, ParticlePoolItem>();
    private List<GameObject> activeFieldSouls = new List<GameObject>();

    private void Start()
    {
        InitializePools();
        RegisterMuzzlePrefabs();
    }

    // 머즐 이펙트 프리팹 등록 메서드
    private void RegisterMuzzlePrefabs()
    {
        // 머즐 이펙트 프리팹이 제대로 설정되었는지 확인
        for (int i = 0; i < muzzleEffectPrefabs.Length; i++)
        {
            if (muzzleEffectPrefabs[i] == null)
            {
                Debug.LogError($"머즐 이펙트 프리팹 {i}번이 설정되지 않았습니다.");
                continue;
            }

            // 각 머즐 타입에 맞는 키 설정
            string key = "";
            switch ((MuzzleType)i)
            {
                case MuzzleType.Normal:
                    key = ParticleKeys.NORMAL_MUZZLE;
                    break;
                case MuzzleType.Explosive:
                    key = ParticleKeys.EXPLOSIVE_MUZZLE;
                    break;
                case MuzzleType.Poison:
                    key = ParticleKeys.POISON_MUZZLE;
                    break;
            }

            // 파티클 풀에 존재하지 않는 경우 새로 등록
            if (!particlePools.ContainsKey(key))
            {
                ParticlePoolItem newItem = new ParticlePoolItem
                {
                    prefab = muzzleEffectPrefabs[i],
                    key = key,
                    defaultCapacity = 5,
                    maxPoolSize = 10,
                    autoReturn = true
                };

                // 이미 particleTypes에 있는지 확인
                bool found = false;
                foreach (var item in particleTypes)
                {
                    if (item.key == key)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    particleTypes.Add(newItem);
                    CreatePool(newItem);
                    poolConfigs[key] = newItem;
                }
            }
        }
    }

    // 머즐 이펙트 프리팹 가져오기 메서드
    public GameObject GetMuzzleEffectPrefab(MuzzleType type)
    {
        int index = (int)type;
        if (index >= 0 && index < muzzleEffectPrefabs.Length)
        {
            return muzzleEffectPrefabs[index];
        }
        
        Debug.LogError($"유효하지 않은 머즐 타입: {type}");
        return null;
    }

    // 머즐 이펙트 키 가져오기 메서드
    public string GetMuzzleEffectKey(MuzzleType type)
    {
        switch (type)
        {
            case MuzzleType.Normal:
                return ParticleKeys.NORMAL_MUZZLE;
            case MuzzleType.Explosive:
                return ParticleKeys.EXPLOSIVE_MUZZLE;
            case MuzzleType.Poison:
                return ParticleKeys.POISON_MUZZLE;
            default:
                return ParticleKeys.NORMAL_MUZZLE;
        }
    }

    private void InitializePools()
    {
        foreach (var item in particleTypes)
        {
            CreatePool(item);
            poolConfigs[item.key] = item;
        }
    }

    private void CreatePool(ParticlePoolItem config)
    {
        IObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: () => CreateParticleObject(config),
            actionOnGet: OnParticleGet,
            actionOnRelease: OnParticleRelease,
            actionOnDestroy: OnParticleDestroy,
            collectionCheck: true,
            defaultCapacity: config.defaultCapacity,
            maxSize: config.maxPoolSize
        );

        particlePools[config.key] = pool;
    }

    private GameObject CreateParticleObject(ParticlePoolItem config)
    {
        GameObject obj = Instantiate(config.prefab);
        obj.name = config.key;
        
        // 자동 반환 컴포넌트 추가 (필요한 경우)
        if (config.autoReturn)
        {
            var autoReturn = obj.AddComponent<ParticleAutoReturn>();
            autoReturn.Setup(config.key);
        }
        
        return obj;
    }

    private void OnParticleGet(GameObject obj)
    {
        obj.SetActive(true);
    }

    private void OnParticleRelease(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnParticleDestroy(GameObject obj)
    {
        Destroy(obj);
    }

    // 일회성 파티클 재생 (자동 반환)
    public void PlayEffect(string key, Vector3 position, Quaternion rotation)
    {
        if (!particlePools.ContainsKey(key))
        {
            Debug.LogError($"파티클 풀에 {key}가 등록되지 않았습니다.");
            return;
        }

        GameObject effect = particlePools[key].Get();
        effect.transform.position = position;
        effect.transform.rotation = rotation;

        // 파티클 시스템 재생
        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            ps.Clear();
            ps.Play();
        }

        // 자동 반환이 비활성화된 경우 수동으로 반환 처리
        if (!poolConfigs[key].autoReturn)
        {
            float duration = GetLongestDuration(particleSystems);
            StartCoroutine(ReturnToPoolAfterDelay(effect, key, duration));
        }
    }

    // 지속성 파티클 가져오기 (수동 반환)
    public GameObject GetParticle(string key, Vector3 position, Quaternion rotation)
    {
        if (!particlePools.ContainsKey(key))
        {
            Debug.LogError($"파티클 풀에 {key}가 등록되지 않았습니다.");
            return null;
        }

        GameObject effect = particlePools[key].Get();
        effect.transform.position = position;
        effect.transform.rotation = rotation;
        return effect;
    }

    // 파티클 풀 반환
    public void ReturnToPool(GameObject obj, string key)
    {
        if (!particlePools.ContainsKey(key))
        {
            Debug.LogError($"파티클 풀에 {key}가 등록되지 않았습니다.");
            return;
        }

        particlePools[key].Release(obj);
    }

    private float GetLongestDuration(ParticleSystem[] systems)
    {
        float maxDuration = 0f;
        foreach (var ps in systems)
        {
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            maxDuration = Mathf.Max(maxDuration, duration);
        }
        return Mathf.Max(maxDuration, 1f);
    }

    private IEnumerator ReturnToPoolAfterDelay(GameObject obj, string key, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null && particlePools.ContainsKey(key))
        {
            ReturnToPool(obj, key);
        }
    }

    // 모든 영혼을 플레이어로 이동하는 메서드
    public void CollectAllSouls(Transform target, float duration = 1.0f, System.Action onComplete = null)
    {
        if (activeFieldSouls.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        int soulCount = activeFieldSouls.Count;
        int collectedCount = 0;

        var soulsToCollect = new List<GameObject>(activeFieldSouls);
        // 영혼들을 랜덤하게 섞어서 수집
        for (int i = 0; i < soulsToCollect.Count; i++)
        {
            int randomIndex = Random.Range(i, soulsToCollect.Count);
            var temp = soulsToCollect[i];
            soulsToCollect[i] = soulsToCollect[randomIndex];
            soulsToCollect[randomIndex] = temp;
        }

        foreach (var soul in soulsToCollect)
        {
            float staggerDelay = collectedCount * 0.3f;  // 각 영혼마다 0.3초 간격
            StartCoroutine(MoveSoulToPlayer(soul, target, duration, staggerDelay, () =>
            {
                collectedCount++;
                EventManager.Instance.SoulCollectedEvent(20); // 각 영혼 수집마다 이벤트 발생

                if (collectedCount >= soulCount)
                {
                    EventManager.Instance.AllSoulsCollectedEvent();
                    onComplete?.Invoke(); // 모든 영혼 수집 완료 후 콜백 실행
                }
            }));
        }
    }

    // 영혼을 플레이어로 이동하는 코루틴
    private IEnumerator MoveSoulToPlayer(GameObject soul, Transform target, float duration, float delay, System.Action onComplete)
    {
        yield return new WaitForSeconds(delay);

        Vector3 startPos = soul.transform.position;
        Vector3 controlPoint = (startPos + target.position) / 2 + Vector3.up * Random.Range(2f, 4f);
        Vector3[] path = new Vector3[3] { startPos, controlPoint, target.position };

        // 트레일 파티클 생성 및 이동
        GameObject trailParticle = GetParticle("VFX_Trail_Void_Variant", startPos, Quaternion.identity);
        if (trailParticle != null)
        {
            trailParticle.transform.DOPath(path, duration, PathType.CatmullRom)
                .SetEase(Ease.InOutQuad)
                .OnUpdate(() =>
                {
                    trailParticle.transform.LookAt(target);
                })
                .OnComplete(() =>
                {
                    ReturnToPool(trailParticle, "VFX_Trail_Void_Variant");
                    onComplete?.Invoke();
                });
        }

        // 원래 영혼 파티클 반환
        ReturnToPool(soul, "LootBeam_Generic_Epic_Variant");
    }
}

// 자동 반환 컴포넌트
public class ParticleAutoReturn : MonoBehaviour
{
    private ParticleSystem[] particleSystems;
    private string poolKey;
    private bool isReturning;

    public void Setup(string key)
    {
        poolKey = key;
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    private void OnEnable()
    {
        isReturning = false;
        StartCoroutine(CheckCompletion());
    }

    private System.Collections.IEnumerator CheckCompletion()
    {
        while (!isReturning && IsPlaying())
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (!isReturning)
        {
            isReturning = true;
            ParticlePool.Instance.ReturnToPool(gameObject, poolKey);
        }
    }

    private bool IsPlaying()
    {
        foreach (var ps in particleSystems)
        {
            if (ps.isPlaying && ps.particleCount > 0)
                return true;
        }
        return false;
    }

    public void ReturnToPoolManually()
    {
        if (!isReturning)
        {
            isReturning = true;
            StopAllCoroutines();
            ParticlePool.Instance.ReturnToPool(gameObject, poolKey);
        }
    }
}
