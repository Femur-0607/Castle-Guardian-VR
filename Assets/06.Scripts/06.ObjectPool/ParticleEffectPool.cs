using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 파티클 이펙트 재사용을 위한 오브젝트 풀 관리 클래스
/// </summary>
public class ParticleEffectPool : MonoBehaviour
{
    #region 싱글톤
    public static ParticleEffectPool Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #endregion
    
    // 풀을 관리할 딕셔너리
    private Dictionary<string, Queue<GameObject>> particlePools = new Dictionary<string, Queue<GameObject>>();
    [SerializeField] private Transform poolParent;
    [SerializeField] private int defaultPoolSize = 5;
    
    private void Start()
    {
        if (poolParent == null) poolParent = transform;
    }
    
    // 효과 재생
    public void PlayEffect(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            return;
        }

        try
        {
            string key = prefab.name;

            // 풀 확인 및 초기화
            if (!particlePools.ContainsKey(key))
            {
                particlePools[key] = new Queue<GameObject>();
                PreparePool(prefab, key, defaultPoolSize);
            }

            // 풀에서 파티클 가져오기
            GameObject effect = GetFromPool(prefab, key);

            // 위치 설정
            effect.transform.position = position;
            effect.transform.rotation = rotation;
            effect.SetActive(true);

            // 파티클 시스템 지속시간 계산을 위한 참조
            ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>();

            // 자동 반환 처리
            StartCoroutine(ReturnToPoolAfterPlay(effect, key, GetLongestDuration(particleSystems)));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error playing particle effect: {e.Message}");
        }
    }

    // 풀 초기화
    private void PreparePool(GameObject prefab, string key, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, poolParent);
            obj.name = key;
            obj.SetActive(false);
            particlePools[key].Enqueue(obj);
        }
    }
    
    // 풀에서 가져오기
    private GameObject GetFromPool(GameObject prefab, string key)
    {
        // 풀이 비었으면 새로 생성
        if (particlePools[key].Count == 0)
        {
            GameObject obj = Instantiate(prefab, poolParent);
            obj.name = key;
            return obj;
        }
        
        return particlePools[key].Dequeue();
    }
    
    // 가장 긴 파티클 지속시간 계산
    private float GetLongestDuration(ParticleSystem[] systems)
    {
        float maxDuration = 0f;
        foreach (var ps in systems)
        {
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            if (duration > maxDuration)
                maxDuration = duration;
        }
        
        return Mathf.Max(maxDuration, 1f); // 최소 1초
    }
    
    // 재생 후 풀에 반환
    private IEnumerator ReturnToPoolAfterPlay(GameObject effect, string key, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (effect != null)
        {
            effect.SetActive(false);
            particlePools[key].Enqueue(effect);
        }
    }
    
    // ReturnEffect 메서드 추가 (호환성 유지용)
    public void ReturnEffect(GameObject effect)
    {
        string key = effect.name;
        
        if (particlePools.ContainsKey(key))
        {
            effect.SetActive(false);
            particlePools[key].Enqueue(effect);
        }
        else
        {
            Destroy(effect);
        }
    }

    // ParticleEffectPool에 추가할 메서드
    public GameObject GetEffectInstance(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // 파티클 인스턴스만 가져오고 활성화 (자동 반환 없음)
        string key = prefab.name;
        
        // 풀 확인 및 초기화
        if (!particlePools.ContainsKey(key))
        {
            particlePools[key] = new Queue<GameObject>();
            PreparePool(prefab, key, defaultPoolSize);
        }
        
        // 풀에서 파티클 가져오기
        GameObject effect = GetFromPool(prefab, key);
        
        // 위치 설정
        effect.transform.position = position;
        effect.transform.rotation = rotation;
        effect.SetActive(true);
        
        return effect;
    }
}

/// <summary>
/// 파티클 이펙트가 재생 완료되면 자동으로 풀에 반환하는 컴포넌트
/// </summary>
public class ParticleEffectAutoReturn : MonoBehaviour
{
    private ParticleSystem[] particleSystems;
    private string prefabKey;
    private bool isReturning = false;

    public void Setup(string key)
    {
        prefabKey = key;
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    private void OnEnable()
    {
        isReturning = false;
        StartCoroutine(CheckParticleCompletion());
    }

    private IEnumerator CheckParticleCompletion()
    {
        // 모든 파티클 시스템이 재생 완료될 때까지 대기
        while (!isReturning && IsPlaying())
        {
            yield return new WaitForSeconds(0.5f); // 0.5초마다 확인
        }
        
        if (!isReturning)
        {
            isReturning = true;
            ParticleEffectPool.Instance.ReturnEffect(gameObject);
        }
    }

    private bool IsPlaying()
    {
        foreach (var ps in particleSystems)
        {
            if (ps.isPlaying && ps.particleCount > 0)
            {
                return true;
            }
        }
        return false;
    }

    // 수동으로 반환 호출
    public void Return()
    {
        if (!isReturning)
        {
            isReturning = true;
            StopAllCoroutines();
            ParticleEffectPool.Instance.ReturnEffect(gameObject);
        }
    }
}
