using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 3D 공간 오디오를 위한 AudioSource 전용 풀 매니저
/// </summary>
public class AudioSourcePool : MonoBehaviour
{
    [Header("AudioSource Prefab & Pool Settings")]
    [SerializeField] private GameObject audioSourcePrefab;  // AudioSource가 있는 프리팹
    [SerializeField] private int defaultCapacity = 20;      // 풀 초기 생성 갯수
    [SerializeField] private int maxSize = 50;              // 풀 최대 갯수
    
    // Unity의 공식 오브젝트 풀 인터페이스
    private IObjectPool<AudioSource> audioSourcePool;
    
    private void Awake()
    {
        // 풀 생성
        audioSourcePool = new ObjectPool<AudioSource>(
            createFunc: CreatePooledItem,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }
    
    // -------------------- ObjectPool 콜백 구현부 --------------------
    
    private AudioSource CreatePooledItem()
    {
        // 오디오소스 프리팹 생성
        GameObject obj = Instantiate(audioSourcePrefab, transform);
        AudioSource source = obj.GetComponent<AudioSource>();
        
        // 기본 3D 사운드 설정
        ConfigureAudioSource(source);
        
        return source;
    }
    
    private void OnTakeFromPool(AudioSource source)
    {
        source.gameObject.SetActive(true);
    }
    
    private void OnReturnedToPool(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
    }
    
    private void OnDestroyPoolObject(AudioSource source)
    {
        Destroy(source.gameObject);
    }
    
    // -------------------- 사용 편의 함수 --------------------
    
    /// <summary>
    /// 3D 사운드를 재생하기 위한 AudioSource를 풀에서 가져옵니다.
    /// </summary>
    /// <param name="clip">재생할 오디오 클립</param>
    /// <param name="position">사운드 발생 위치</param>
    /// <param name="volume">볼륨(0.0f ~ 1.0f)</param>
    /// <param name="parent">AudioSource를 붙일 부모 트랜스폼(없으면 풀의 자식으로 유지)</param>
    /// <returns>설정된 AudioSource</returns>
    public AudioSource PlaySound(AudioClip clip, Vector3 position, float volume = 1.0f, Transform parent = null)
    {
        if (clip == null) return null;
        
        AudioSource source = audioSourcePool.Get();
        source.transform.position = position;
        
        // 부모 설정 (필요시)
        if (parent != null)
            source.transform.SetParent(parent);
        else
            source.transform.SetParent(transform);
        
        // 오디오 소스 설정
        source.clip = clip;
        source.volume = volume;
        source.Play();
        
        // 클립 재생이 끝나면 풀로 자동 반환
        StartCoroutine(ReturnToPoolAfterPlay(source, clip.length));
        
        return source;
    }
    
    /// <summary>
    /// 3D 사운드를 재생하기 위한 AudioSource를 설정만 하고 가져옵니다. (수동 재생 필요)
    /// </summary>
    public AudioSource GetAudioSource(Vector3 position, Transform parent = null)
    {
        AudioSource source = audioSourcePool.Get();
        source.transform.position = position;
        
        if (parent != null)
            source.transform.SetParent(parent);
        else
            source.transform.SetParent(transform);
            
        return source;
    }
    
    /// <summary>
    /// AudioSource를 풀로 반환합니다.
    /// </summary>
    public void ReleaseAudioSource(AudioSource source)
    {
        if (source != null)
            audioSourcePool.Release(source);
    }
    
    // -------------------- 유틸리티 함수 --------------------
    
    private System.Collections.IEnumerator ReturnToPoolAfterPlay(AudioSource source, float clipLength)
    {
        yield return new WaitForSeconds(clipLength + 0.1f); // 재생 완료 후 약간의 여유 시간
        
        if (source != null && source.gameObject.activeInHierarchy)
            audioSourcePool.Release(source);
    }
    
    /// <summary>
    /// AudioSource의 3D 설정을 구성합니다.
    /// </summary>
    private void ConfigureAudioSource(AudioSource source)
    {
        // 3D 사운드 설정
        source.spatialBlend = 1.0f;  // 완전한 3D 사운드
        source.rolloffMode = AudioRolloffMode.Linear;  // 거리에 따른 감쇠 모드
        source.minDistance = 1f;  // 최소 거리 (이 거리 내에서는 볼륨 최대)
        source.maxDistance = 150f;  // 최대 거리 (이 거리 이상에서는 들리지 않음)
        
        // VR 최적화 설정 (선택 사항)
        source.spatialize = true;  // 공간화 활성화 (VR/AR 오디오용)
        source.spread = 0f;  // 방향성 있는 소리
        source.dopplerLevel = 1f;  // 도플러 효과 활성화
    }
}