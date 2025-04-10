using UnityEngine;

public class PlayerExperienceSystem : MonoBehaviour
{
    private static PlayerExperienceSystem _instance;
    public static PlayerExperienceSystem Instance => _instance;

    [Header("레벨 시스템")]
    [SerializeField] private int currentLevel = 1;      // 현재 레벨
    [SerializeField] private int currentExp = 0;        // 현재 경험치
    [SerializeField] private int[] expRequiredPerLevel; // 레벨별 필요 경험치

    [Header("플레이어 스텟")]
    [SerializeField] private float baseDamage = 10f;                    // 기본 공격력
    [SerializeField] private float baseAttackSpeed = 1f;                // 기본 공격속도
    [SerializeField] private float damageIncreasePerLevel = 2f;        // 레벨당 공격력 증가량
    [SerializeField] private float attackSpeedIncreasePerLevel = 0.1f; // 레벨당 공격속도 증가량

    // 현재 스텟 계산용 프로퍼티
    public float CurrentDamage => baseDamage + (currentLevel - 1) * damageIncreasePerLevel;
    public float CurrentAttackSpeed => baseAttackSpeed + (currentLevel - 1) * attackSpeedIncreasePerLevel;

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
    }

    /// <summary>
    /// 경험치를 추가합니다.
    /// </summary>
    /// <param name="amount">추가할 경험치 양</param>
    public void AddExperience(int amount)
    {
        currentExp += amount;
        CheckLevelUp();
    }

    /// <summary>
    /// 레벨업 조건을 체크하고 처리합니다.
    /// </summary>
    private void CheckLevelUp()
    {
        // 현재 레벨에 필요한 경험치를 초과했는지 확인
        if (currentLevel <= expRequiredPerLevel.Length && 
            currentExp >= expRequiredPerLevel[currentLevel - 1])
        {
            // 레벨업 처리
            currentLevel++;
            currentExp = 0;
            
            // 레벨업 이벤트 발생
            EventManager.Instance.LevelUpEvent(currentLevel);
        }
    }
}
