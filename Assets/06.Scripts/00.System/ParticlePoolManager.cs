using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;
public class ParticlePoolManager : MonoBehaviour
{
    private static ParticlePoolManager _instance;
    public static ParticlePoolManager Instance => _instance;

    [System.Serializable]
    public class ParticlePoolItem
    {
        public GameObject prefab;
        public int initialPoolSize = 10;
        public string key;
        public bool autoExpand = true;
    }

    [SerializeField] private List<ParticlePoolItem> particleTypes = new List<ParticlePoolItem>();
    private Dictionary<string, Queue<GameObject>> particlePools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> particleContainer = new Dictionary<string, GameObject>();

    // 필드에 남아있는 영혼(LootBeam) 파티클들 추적
    private List<GameObject> activeFieldSouls = new List<GameObject>();

    private void Awake()
    {
        _instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var item in particleTypes)
        {
            GameObject container = new GameObject($"Pool_{item.key}");
            container.transform.SetParent(transform);
            particleContainer[item.key] = container;

            Queue<GameObject> objectPool = new Queue<GameObject>();
            particlePools[item.key] = objectPool;

            for (int i = 0; i < item.initialPoolSize; i++)
            {
                CreateNewPoolItem(item.key);
            }
        }
    }

    private GameObject CreateNewPoolItem(string key)
    {
        ParticlePoolItem config = particleTypes.Find(x => x.key == key);
        if (config == null) return null;

        GameObject newObject = Instantiate(config.prefab);
        newObject.SetActive(false);
        newObject.transform.SetParent(particleContainer[key].transform);
        particlePools[key].Enqueue(newObject);
        return newObject;
    }

    public void ReturnToPool(GameObject obj, string poolKey)
    {
        if (!particlePools.ContainsKey(poolKey)) return;

        obj.SetActive(false);
        obj.transform.SetParent(particleContainer[poolKey].transform);
        particlePools[poolKey].Enqueue(obj);

        // 추적 목록에서도 제거
        activeFieldSouls.Remove(obj);
    }

    // 모든 필드 영혼을 플레이어에게 이동시키는 메서드
    public void CollectAllSouls(Transform target, float duration, System.Action onComplete = null)
    {
        // 수집할 영혼이 없으면 즉시 콜백 실행
        if (activeFieldSouls.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        // 영혼 수집 카운터
        int soulCount = activeFieldSouls.Count;
        int collectedCount = 0;

        // 리스트 복사
        var soulsToCollect = new List<GameObject>(activeFieldSouls);

        // 각 영혼마다 약간의 시간 간격을 두고 순차적으로 수집
        for (int i = 0; i < soulsToCollect.Count; i++)
        {
            var soul = soulsToCollect[i];
            
            // 잠시 후에 영혼 이동 시작 (순차적 효과)
            float staggerDelay = i * 0.2f;  // 각 영혼마다 0.2초 간격
            
            StartCoroutine(DelayedSoulCollection(soul, target, duration, staggerDelay, () => {
                collectedCount++;
                
                // 모든 영혼이 수집되었는지 확인
                if (collectedCount >= soulCount)
                {
                    // 마지막 영혼이 수집되면 콜백 실행
                    onComplete?.Invoke();
                }
            }));
        }
    }

    private IEnumerator DelayedSoulCollection(GameObject soul, Transform target, float duration, float delay, System.Action onSoulCollected)
    {
        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(delay);
        
        // 지연 후 영혼 이동 시작
        MoveSoulToPlayer(soul, target, duration, onSoulCollected);
    }

    private void MoveSoulToPlayer(GameObject soul, Transform target, float duration, System.Action onSoulCollected = null)
    {
        Debug.Log("Target Position for Soul: " + target.position);
        
        // 영혼 파티클을 직접 플레이어 방향으로 이동
        soul.transform.DOMove(target.position, duration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => {
                // 플레이어 경험치 증가
                if (PlayerExperienceSystem.Instance != null)
                {
                    PlayerExperienceSystem.Instance.AddExperience(1);
                }
            
                // 영혼 파티클 비활성화 및 풀로 반환
                ReturnToPool(soul, "LootBeam_Generic_Epic_Variant");
            
                // 콜백 실행
                onSoulCollected?.Invoke();
            });
    }

    // 영혼 파티클 생성 (기존 ParticleEffectPool 활용)
    public void SpawnSoulParticle(Vector3 position)
    {
        // 1. 사망 효과 재생 (일회성)
        ParticleEffectPool.Instance.PlayEffect(
            GetPrefabByName("Loot_Poof_Variant"), position, Quaternion.identity);
            
        // 2. 영혼 효과 생성 (지속성)
        GameObject soulEffect = ParticleEffectPool.Instance.GetEffectInstance(
            GetPrefabByName("LootBeam_Generic_Epic_Variant"), position, Quaternion.identity);
            
        // 영혼 추적에 추가
        if (soulEffect != null)
            activeFieldSouls.Add(soulEffect);
    }
    
    // 프리팹 가져오기 헬퍼 메서드
    private GameObject GetPrefabByName(string name)
    {
        // particleTypes 목록에서 해당 키를 가진 아이템 찾기
        ParticlePoolItem item = particleTypes.Find(x => x.key == name);
        if (item != null)
            return item.prefab;
        
        // 찾지 못한 경우 Resources에서 시도
        return Resources.Load<GameObject>($"Particles/{name}");
    }
}
