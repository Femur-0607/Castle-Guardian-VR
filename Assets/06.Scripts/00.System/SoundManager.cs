using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    #region 필드 변수
    
    // 안정성이 중요한 경우 사용
    // - Instance를 get 전용으로 만들어 외부에서 변경 불가능
    // - Null을 반환할 가능성이 있어 예외 발생 가능성을 줄일 수 있음
    private static SoundManager _instance;
    public static SoundManager Instance => _instance;

    [Header("오디오 믹서(볼륨 제어)")]
    [Tooltip("유니티 AudioMixer 에셋 참조")]
    public AudioMixer audioMixer;

    [Header("믹서에서 Exposed Parameter 이름")]
    [Tooltip("BGM 볼륨 파라미터 이름 (예: \"BGMVolume\"). AudioMixer에서 직접 노출해둬야 함")]
    public string bgmVolumeParam = "BGMVolume";
    [Tooltip("SFX 볼륨 파라미터 이름 (예: \"SFXVolume\"). AudioMixer에서 직접 노출해둬야 함")]
    public string sfxVolumeParam = "SFXVolume";
    [Tooltip("UI 볼륨 파라미터 이름 (예: \"UIVolume\"). AudioMixer에서 직접 노출해둬야 함")]
    public string uiVolumeParam = "UIVolume";

    [Header("BGM / SFX / UI AudioSource")]
    [Tooltip("BGM 전용 AudioSource")]
    public AudioSource bgmSource;
    [Tooltip("SFX 전용 AudioSource (OneShot용)")]
    public AudioSource sfxSource;
    [Tooltip("UI 전용 AudioSource (OneShot용)")]
    public AudioSource uiSource;

    [Header("ScriptableObject")]
    [Tooltip("SoundDatabase.asset을 참조")]
    public SoundDatabase soundDatabase;

    [Header("AudioSource Pool")]
    [Tooltip("AudioSource 풀 참조")]
    public AudioSourcePool audioSourcePool;

    // 내부용 Dictionary: soundID → SoundData
    private Dictionary<string, SoundData> soundDict;

    public GameObject oVRCamera;

    #endregion

    private void Awake()
    {
        // 싱글톤 체크(옵션)
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // ScriptableObject에서 사운드 정보 빌드
        BuildSoundDictionary();
    }

    private void BuildSoundDictionary()
    {
        soundDict = new Dictionary<string, SoundData>();

        if (soundDatabase == null || soundDatabase.soundList == null)
        {
            Debug.LogWarning("SoundDatabase가 비어있습니다.");
            return;
        }

        foreach (var data in soundDatabase.soundList)
        {
            if (!soundDict.ContainsKey(data.soundID))
            {
                soundDict.Add(data.soundID, data);
            }
            else
            {
                Debug.LogWarning($"SoundID 중복 발견: {data.soundID}. 먼저 등록된 데이터로 유지됩니다.");
            }
        }
    }

    #region 볼륨 세팅 (AudioMixer)
    /// <summary>
    /// 0 ~ 1 사이의 값(volume)을 AudioMixer의 dB로 변환하여 적용
    /// </summary>
    private float LinearToDecibel(float linear)
    {
        // 음향공학적으로 20 * log10(…) 형태를 많이 사용
        // 0일 때 -80dB (사일런트), 1일 때 0dB
        // 값이 0이 될 경우 log10(0)이 에러이므로 최소값 보정
        float dB;
        if (linear <= 0f)
            dB = -80f;
        else
            dB = Mathf.Log10(linear) * 20f;

        return dB;
    }

    public void SetBGMVolume(float volume)
    {
        float dB = LinearToDecibel(volume);
        audioMixer.SetFloat(bgmVolumeParam, dB);
    }

    public void SetSFXVolume(float volume)
    {
        float dB = LinearToDecibel(volume);
        audioMixer.SetFloat(sfxVolumeParam, dB);
    }

    public void SetUIVolume(float volume)
    {
        float dB = LinearToDecibel(volume);
        audioMixer.SetFloat(uiVolumeParam, dB);
    }
    #endregion

    #region 사운드 재생 (Dictionary)
    /// <summary>
    /// SoundID로 사운드를 재생. BGM이면 loop 소스, SFX/UI면 OneShot
    /// </summary>
    public void PlaySound(string soundID)
    {
        if (soundDict == null)
        {
            Debug.LogWarning("SoundDictionary가 생성되지 않았습니다.");
            return;
        }

        if (!soundDict.TryGetValue(soundID, out SoundData data))
        {
            Debug.LogWarning($"해당 SoundID({soundID})가 SoundDatabase에 없습니다.");
            return;
        }

        switch (data.category)
        {
            case SoundCategory.BGM:
                // BGM 전용 소스에 clip 할당 후 Play
                if (bgmSource.clip != data.clip)
                {
                    bgmSource.clip = data.clip;
                    bgmSource.loop = data.loop;
                    bgmSource.volume = data.volume; // Mixer 전 단계의 볼륨 오프셋
                    bgmSource.Play();
                }
                break;

            case SoundCategory.SFX:
                if (audioSourcePool != null)
                {
                    // 카메라 위치에서 사운드 재생 (3D 효과)
                    audioSourcePool.PlaySound(data.clip, oVRCamera.transform.position, data.volume);
                }
                else
                {
                    // AudioSourcePool이 없으면 기존 방식으로 재생
                    sfxSource.volume = data.volume;
                    sfxSource.PlayOneShot(data.clip);
                }
                break;

            case SoundCategory.UI:
                // UI도 OneShot
                uiSource.volume = data.volume; 
                uiSource.PlayOneShot(data.clip);
                break;
        }
    }

    // 새로운 메서드 추가 - 특정 위치에서 3D 사운드 재생
    public void PlaySound3D(string soundID, Vector3 position, Transform parent = null)
    {
        Debug.Log($"PlaySound3D({soundID}, {position})");
        
        float distanceToCamera = Vector3.Distance(oVRCamera.transform.position, position);
        Debug.Log($"카메라와의 거리: {distanceToCamera}유닛 (최대 들림 거리: 20유닛)");
        
        if (soundDict == null || audioSourcePool == null)
        {
            Debug.LogWarning("SoundDictionary 또는 AudioSourcePool이 초기화되지 않았습니다.");
            return;
        }

        if (!soundDict.TryGetValue(soundID, out SoundData data))
        {
            Debug.LogWarning($"해당 SoundID({soundID})가 SoundDatabase에 없습니다.");
            return;
        }

        if (data.category != SoundCategory.SFX)
        {
            Debug.LogWarning($"SoundID({soundID})는 SFX 카테고리가 아닙니다. 3D 사운드는 SFX 카테고리만 지원합니다.");
            return;
        }

        // AudioSourcePool을 통해 3D 사운드 재생
        audioSourcePool.PlaySound(data.clip, position, data.volume, parent);
    }

    /// <summary>
    /// 특정 SoundID가 BGM이면 멈춤
    /// (필요시 만들어둔 메서드. BGM만 멈추고 싶을 때 활용)
    /// </summary>
    public void StopBGM(string soundID)
    {
        if (soundDict.TryGetValue(soundID, out SoundData data))
        {
            if (data.category == SoundCategory.BGM && bgmSource.clip == data.clip)
            {
                bgmSource.Stop();
            }
        }
    }

    /// <summary>
    /// 모든 BGM 중지 (원한다면)
    /// </summary>
    public void StopAllBGM()
    {
        bgmSource.Stop();
    }
    #endregion
}
