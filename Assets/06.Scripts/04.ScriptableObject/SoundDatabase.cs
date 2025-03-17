using UnityEngine;
using System.Collections.Generic;

public enum SoundCategory
{
    BGM,
    SFX ,
    UI
}

[System.Serializable]
public class SoundData
{
    [Tooltip("사운드를 식별할 ID (예: \"MainTitle\", \"Walk\", \"ButtonClick\" 등)")]
    public string soundID;

    [Tooltip("사운드 카테고리 (BGM / SFX / UI 등)")]
    public SoundCategory category;

    [Tooltip("재생할 오디오 클립")]
    public AudioClip clip;

    [Tooltip("이 사운드만의 볼륨 오프셋(0~1). 1이면 100%")]
    [Range(0f, 1f)]
    public float volume = 1f;

    [Tooltip("BGM처럼 Loop가 필요한 경우 체크")]
    public bool loop = false;
}

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Audio/Sound Database", order = 0)]
public class SoundDatabase : ScriptableObject
{
    [Header("사운드 리스트")]
    public List<SoundData> soundList;
}