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

    public GameObject GetParticle(string key, Vector3 position, Quaternion rotation, bool trackAsSoul = false)
    {
        if (!particlePools.ContainsKey(key))
        {
            Debug.LogError($"풀에 {key} 파티클이 등록되지 않았습니다.");
            return null;
        }

        Queue<GameObject> pool = particlePools[key];
        GameObject particleObj = null;

        if (pool.Count == 0)
        {
            ParticlePoolItem config = particleTypes.Find(x => x.key == key);
            if (config.autoExpand)
            {
                particleObj = CreateNewPoolItem(key);
            }
            else
            {
                Debug.LogWarning($"{key} 풀이 비었으며 자동 확장이 비활성화되었습니다.");
                return null;
            }
        }
        else
        {
            particleObj = pool.Dequeue();
        }

        particleObj.transform.position = position;
        particleObj.transform.rotation = rotation;
        particleObj.SetActive(true);

        // 지속적으로 필드에 남아있는 파티클인 경우 추적 목록에 추가
        if (trackAsSoul)
        {
            activeFieldSouls.Add(particleObj);
        }
        else
        {
            // 일시적인 파티클의 경우 자동 반환 처리
            ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                StartCoroutine(ReturnToPoolAfterPlay(particleObj, key, ps.main.duration + ps.main.startLifetime.constantMax));
            }
        }

        return particleObj;
    }

    private System.Collections.IEnumerator ReturnToPoolAfterPlay(GameObject obj, string poolKey, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(obj, poolKey);
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
    public void CollectAllSouls(Transform target, float duration = 1.0f, System.Action onComplete = null)
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

        // 순차적으로 영혼을 수집하기 위한 리스트 복사 및 셔플
        var soulsToCollect = new List<GameObject>(activeFieldSouls);
        for (int i = 0; i < soulsToCollect.Count; i++)
        {
            int randomIndex = Random.Range(i, soulsToCollect.Count);
            var temp = soulsToCollect[i];
            soulsToCollect[i] = soulsToCollect[randomIndex];
            soulsToCollect[randomIndex] = temp;
        }

        // 각 영혼마다 약간의 시간 간격을 두고 순차적으로 수집
        for (int i = 0; i < soulsToCollect.Count; i++)
        {
            var soul = soulsToCollect[i];
            
            // 잠시 후에 영혼 이동 시작 (순차적 효과)
            float staggerDelay = i * 0.1f;  // 각 영혼마다 0.1초 간격
            
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
        // 시작 위치 (현재 LootBeam 위치)
        Vector3 startPos = soul.transform.position;
        
        // 1. LootBeam 파티클 비활성화 및 풀로 반환
        ReturnToPool(soul, "LootBeam_Generic_Epic_Variant");
        
        // 2. 트레일 파티클 생성
        GameObject trailParticle = GetParticle("VFX_Trail_Void_Variant", startPos, Quaternion.identity);
        if (trailParticle == null)
        {
            Debug.LogError("VFX_Trail_Void_Variant 파티클을 찾을 수 없습니다.");
            onSoulCollected?.Invoke();
            return;
        }
        
        // 3. 중간 지점 (아치형을 만들기 위한 높이)
        Vector3 controlPoint = (startPos + target.position) / 2 + Vector3.up * Random.Range(2f, 4f);
        
        // 4. 경로 포인트 배열 생성
        Vector3[] path = new Vector3[3] { 
            startPos, 
            controlPoint, 
            target.position 
        };
        
        // 5. 작은 랜덤 지연 추가로 모든 영혼이 동시에 움직이지 않도록 함
        float randomDelay = Random.Range(0f, 0.3f);
        
        // 6. 크기 효과
        trailParticle.transform.DOScale(trailParticle.transform.localScale * 1.2f, 0.3f)
            .SetDelay(randomDelay)
            .SetEase(Ease.OutBack);
        
        // 7. 경로를 따라 이동 애니메이션
        trailParticle.transform.DOPath(path, duration, PathType.CatmullRom)
            .SetDelay(randomDelay)
            .SetEase(Ease.InOutQuad)
            .OnUpdate(() => {
                // 이동 중 트레일이 플레이어를 향하도록 회전
                trailParticle.transform.LookAt(target);
                
                // 이동 중 약간의 크기 변화를 줘서 맥동 효과
                float pulseScale = 1f + 0.1f * Mathf.Sin(Time.time * 8f);
                trailParticle.transform.localScale = Vector3.one * pulseScale;
            })
            .OnComplete(() => {
                // 도착 시 효과 (임팩트 효과)
                trailParticle.transform.DOScale(Vector3.zero, 0.2f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => {
                        // 플레이어 경험치 증가
                        if (PlayerExperienceSystem.Instance != null)
                        {
                            PlayerExperienceSystem.Instance.AddExperience(1);
                        }
                        
                        // 트레일 파티클 비활성화 및 풀로 반환
                        ReturnToPool(trailParticle, "VFX_Trail_Void_Variant");
                        
                        // 개별 영혼 수집 콜백 실행
                        onSoulCollected?.Invoke();
                    });
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
