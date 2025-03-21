using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public class PlayerExperienceSystem : MonoBehaviour
{
    private static PlayerExperienceSystem _instance;
    public static PlayerExperienceSystem Instance => _instance;

    [Header("레벨 시스템")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int[] expRequiredPerLevel;

    [Header("UI 요소")]
    [SerializeField] private Slider expBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statsText;

    [Header("플레이어 스텟")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float damageIncreasePerLevel = 2f;
    [SerializeField] private float attackSpeedIncreasePerLevel = 0.1f;

    // 현재 스텟 계산용 프로퍼티
    public float CurrentDamage => baseDamage + (currentLevel - 1) * damageIncreasePerLevel;
    public float CurrentAttackSpeed => baseAttackSpeed + (currentLevel - 1) * attackSpeedIncreasePerLevel;

    // 레벨업 이벤트
    public event Action<int> OnLevelUp;

    private void Awake()
    {
        _instance = this;
        
        // 기본 경험치 테이블 설정 (따로 정의되지 않은 경우)
        if (expRequiredPerLevel == null || expRequiredPerLevel.Length == 0)
        {
            expRequiredPerLevel = new int[10];
            for (int i = 0; i < 10; i++)
            {
                expRequiredPerLevel[i] = 100 * (i + 1);
            }
        }
        
        UpdateUI();
    }

    public void AddExperience(int amount)
    {
        currentExp += amount;
        
        // 경험치 바 애니메이션 업데이트
        UpdateExpBarWithAnimation();
        
        // 레벨업 체크는 애니메이션 후 실행
    }

    private void UpdateExpBarWithAnimation()
    {
        if (expBar != null)
        {
            // 현재 값에서 목표값까지 부드럽게 증가
            float targetValue = currentExp;
            float startValue = expBar.value;
            
            DOTween.To(() => expBar.value, x => expBar.value = x, targetValue, 0.5f)
                .SetEase(Ease.OutQuad) // 부드러운 감속 효과
                .OnUpdate(() => {
                    // 업데이트 중에 추가 효과 (예: 색상 변화, 파티클 등)
                    if (expBar.value >= expBar.maxValue)
                    {
                        // 레벨업 처리
                        currentLevel++;
                        currentExp = 0;
                        
                        // 레벨업 시각 효과
                        PlayLevelUpEffect();
                        
                        // 다음 레벨 경험치 요구량으로 바 최대값 업데이트
                        if (currentLevel <= expRequiredPerLevel.Length)
                        {
                            expBar.maxValue = expRequiredPerLevel[currentLevel - 1];
                        }
                        
                        // 바 초기화 후 애니메이션 재시작
                        expBar.value = 0;
                        UpdateUI();
                    }
                })
                .OnComplete(() => {
                    // 애니메이션 완료 후 UI 업데이트
                    UpdateUI();
                });
        }
    }

    private void PlayLevelUpEffect()
    {
        // 레벨업 효과음
        // AudioManager.Instance?.PlaySFX("LevelUp");
        
        // 레벨업 UI 효과 (텍스트 효과)
        if (levelText != null)
        {
            // 원래 크기 저장
            Vector3 originalScale = levelText.transform.localScale;
            
            // 텍스트 확대 효과
            levelText.transform.DOScale(originalScale * 1.5f, 0.3f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => {
                    // 원래 크기로 복귀
                    levelText.transform.DOScale(originalScale, 0.2f)
                        .SetEase(Ease.InOutQuad);
                });
            
            // 반짝임 효과 (색상 변경)
            Color originalColor = levelText.color;
            DOTween.Sequence()
                .Append(levelText.DOColor(Color.yellow, 0.2f))
                .Append(levelText.DOColor(originalColor, 0.3f));
        }
    }

    private void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"레벨: {currentLevel}";
        
        if (expBar != null)
        {
            // 현재 레벨에 필요한 경험치를 기준으로 진행률 계산
            if (currentLevel <= expRequiredPerLevel.Length)
            {
                expBar.maxValue = expRequiredPerLevel[currentLevel - 1];
                expBar.value = currentExp;
            }
            else
            {
                expBar.maxValue = 1;
                expBar.value = 1;
            }
        }
        
        if (statsText != null)
        {
            statsText.text = $"공격력: {CurrentDamage:F1}\n공격속도: {CurrentAttackSpeed:F1}";
        }
    }
}
