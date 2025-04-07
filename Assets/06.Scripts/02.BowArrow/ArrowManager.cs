using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 화살 관리 시스템: 다양한 화살 타입을 관리하고 전환하는 역할
/// 화살 잠금 해제, 업그레이드, 현재 장착 화살 관리 담당
/// </summary>
public class ArrowManager : MonoBehaviour
{
    #region 싱글톤

    // 싱글톤 인스턴스 - 전역적으로 접근 가능
    public static ArrowManager Instance { get; private set; }
    
    /// <summary>
    /// 싱글톤 초기화
    /// </summary>
    private void Awake()
    {
        // 싱글톤 패턴 구현 (중복 인스턴스 방지)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #endregion

    #region 필드 변수

    [Header("화살 타입 및 프리팹")]
    [SerializeField] private GameObject normalArrowPrefab;    // 기본 화살 프리팹
    [SerializeField] private GameObject explosiveArrowPrefab; // 폭발 화살 프리팹
    [SerializeField] private GameObject poisonArrowPrefab;    // 독 화살 프리팹
    [SerializeField] private GameObject normalMuzzleEffectPrefab;   // 기본 화살 머즐 이펙트 프리팹 
    [SerializeField] private GameObject explosiveMuzzleEffectPrefab; // 폭발 화살 머즐 이펙트 프리팹
    [SerializeField] private GameObject poisonMuzzleEffectPrefab;    // 독 화살 머즐 이펙트 프리팹
    
    [Header("화살 데이터")]
    [SerializeField] private ProjectileData normalArrowData;    // 기본 화살 데이터
    [SerializeField] private ProjectileData explosiveArrowData; // 폭발 화살 데이터
    [SerializeField] private ProjectileData poisonArrowData;    // 독 화살 데이터
    
    [Header("참조")]
    [SerializeField] private ArrowShooter arrowShooter;  // 화살 발사기 참조
    
    // 현재 장착된 화살 타입 (읽기 전용 속성)
    public ProjectileData.ProjectileType CurrentArrowType { get; private set; } = ProjectileData.ProjectileType.Normal;
    
    // 잠금 해제된 화살 목록
    private List<ProjectileData.ProjectileType> unlockedArrows = new List<ProjectileData.ProjectileType>();
    
    // 각 화살별 레벨과 잠금 상태 관리를 위한 딕셔너리
    private Dictionary<ProjectileData.ProjectileType, int> arrowLevels = new Dictionary<ProjectileData.ProjectileType, int>();
    private Dictionary<ProjectileData.ProjectileType, bool> arrowUnlocked = new Dictionary<ProjectileData.ProjectileType, bool>();

    #endregion

    #region 유니티 이벤트 함수
    
    private void OnEnable()
    {
        EventManager.Instance.OnLevelUp += HandleLevelUp;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnLevelUp -= HandleLevelUp;
    }
    
    /// <summary>
    /// 초기화 - 시작 시 기본 화살 잠금 해제 및 다른 화살 초기화
    /// </summary>
    void Start()
    {
        // 기본 화살 초기화 (기본적으로 잠금 해제된 상태)
        unlockedArrows.Add(ProjectileData.ProjectileType.Normal);
        arrowLevels[ProjectileData.ProjectileType.Normal] = 1;
        arrowUnlocked[ProjectileData.ProjectileType.Normal] = true;
        
        // 폭발 화살 초기화 (초기에는 잠금 상태)
        arrowLevels[ProjectileData.ProjectileType.Explosive] = 0;
        arrowUnlocked[ProjectileData.ProjectileType.Explosive] = false;
        
        // 독 화살 초기화 (초기에는 잠금 상태)
        arrowLevels[ProjectileData.ProjectileType.Poison] = 0;
        arrowUnlocked[ProjectileData.ProjectileType.Poison] = false;
        
        // 시작 시 기본 화살 장착
        SwitchArrowType(ProjectileData.ProjectileType.Normal);
    }

    #endregion

    #region 능력치 상승
    
    /// <summary>
    /// 레벨업 시 화살 데미지 증가
    /// </summary>
    private void HandleLevelUp(int newLevel)
    {
        // 모든 화살의 데미지 증가
        normalArrowData.baseDamage += normalArrowData.damageIncreasePerLevel;
        explosiveArrowData.baseDamage += explosiveArrowData.damageIncreasePerLevel;
        poisonArrowData.baseDamage += poisonArrowData.damageIncreasePerLevel;
    }
    
    #endregion
    
    /// <summary>
    /// 화살 타입 전환 - 다른 종류의 화살로 바꾸는 기능
    /// </summary>
    /// <param name="arrowType">전환할 화살 타입</param>
    public void SwitchArrowType(ProjectileData.ProjectileType arrowType)
    {
        // 잠금 해제된 화살인지 확인
        if (!arrowUnlocked[arrowType])
        {
            return;
        }

        // 현재 화살 타입을 새로운 타입으로 변경
        CurrentArrowType = arrowType;

        // 화살 발사기에 새 프리팹과 데이터 설정 (각 화살 타입에 맞게)
        switch (arrowType)
        {
            case ProjectileData.ProjectileType.Normal:
                arrowShooter.SetProjectilePrefab(normalArrowPrefab, normalArrowData, normalMuzzleEffectPrefab);
                break;

            case ProjectileData.ProjectileType.Explosive:
                arrowShooter.SetProjectilePrefab(explosiveArrowPrefab, explosiveArrowData, explosiveMuzzleEffectPrefab);
                break;

            case ProjectileData.ProjectileType.Poison:
                arrowShooter.SetProjectilePrefab(poisonArrowPrefab, poisonArrowData, poisonMuzzleEffectPrefab);
                break;
        }

        // UI 업데이트 이벤트 발생 (화살 선택 UI 등에서 사용)
        OnArrowTypeChanged?.Invoke(CurrentArrowType);
    }
    
    /// <summary>
    /// 화살 잠금 해제 여부 확인
    /// </summary>
    /// <param name="arrowType">확인할 화살 타입</param>
    /// <returns>해금되었는지 여부</returns>
    public bool IsArrowUnlocked(ProjectileData.ProjectileType arrowType)
    {
        return arrowUnlocked.ContainsKey(arrowType) && arrowUnlocked[arrowType];
    }
    
    /// <summary>
    /// 화살 잠금 해제 - 화살 타입을 사용 가능하게 함
    /// </summary>
    /// <param name="arrowType">해금할 화살 타입</param>
    /// <returns>해금 성공 여부</returns>
    public bool UnlockArrow(ProjectileData.ProjectileType arrowType)
    {   
        // 이미 해금된 화살인지 확인
        if (arrowUnlocked[arrowType])
        {
            return false;
        }
        
        // 화살 잠금 해제 처리
        arrowUnlocked[arrowType] = true;
        arrowLevels[arrowType] = 1;        // 초기 레벨 1
        unlockedArrows.Add(arrowType);     // 사용 가능한 화살 목록에 추가
        
        return true;
    }
    
    /// <summary>
    /// 다음 화살로 순환 - 마우스 휠 등으로 사용 가능한 화살을 순차적으로 전환
    /// </summary>
    public void CycleNextArrow()
    {
        // 사용 가능한 화살이 1개 이하면 무시
        if (unlockedArrows.Count <= 1) 
        {
            return;
        }
        
        // 현재 화살의 인덱스 찾기
        int currentIndex = unlockedArrows.IndexOf(CurrentArrowType);
        
        // 다음 화살로 인덱스 이동 (마지막 화살이면 처음으로 순환)
        int nextIndex = (currentIndex + 1) % unlockedArrows.Count;
        
        // 다음 화살로 전환
        SwitchArrowType(unlockedArrows[nextIndex]);
    }
    
    /// <summary>
    /// 이전 화살로 순환 - 마우스 휠 등으로 사용 가능한 화살을 순차적으로 전환
    /// </summary>
    public void CyclePreviousArrow()
    {
        // 사용 가능한 화살이 1개 이하면 무시
        if (unlockedArrows.Count <= 1) 
        {
            Debug.Log("사용 가능한 화살이 1개 이하입니다.");
            return;
        }
        
        // 현재 화살의 인덱스 찾기
        int currentIndex = unlockedArrows.IndexOf(CurrentArrowType);
        Debug.Log($"현재 화살 인덱스: {currentIndex}, 잠금 해제된 화살 수: {unlockedArrows.Count}");
        
        // 이전 화살로 인덱스 이동 (첫 번째 화살이면 마지막으로 순환)
        int previousIndex = (currentIndex - 1 + unlockedArrows.Count) % unlockedArrows.Count;
        
        // 이전 화살로 전환
        SwitchArrowType(unlockedArrows[previousIndex]);
        Debug.Log($"이전 화살로 변경: {unlockedArrows[previousIndex]}");
    }
    
    /// <summary>
    /// 화살 타입 변경 이벤트 - UI 등에서 구독하여 화살 변경 시 알림 받음
    /// </summary>
    public delegate void ArrowTypeChangedHandler(ProjectileData.ProjectileType newType);
    public event ArrowTypeChangedHandler OnArrowTypeChanged;

    /// <summary>
    /// 활 업그레이드 - 모든 화살의 공통 성능 향상 
    /// </summary>
    /// <returns>업그레이드 성공 여부</returns>
    public bool UpgradeAllArrows()
    {
        // 모든 화살 타입에 공통 배수 적용
        normalArrowData.baseMultiplier += normalArrowData.baseMultiplierIncreasePerLevel;
        
        // 해금된 특수 화살에도 동일한 강화 적용
        if (IsArrowUnlocked(ProjectileData.ProjectileType.Explosive))
        {
            explosiveArrowData.baseMultiplier = normalArrowData.baseMultiplier;
        }
        
        if (IsArrowUnlocked(ProjectileData.ProjectileType.Poison))
        {
            poisonArrowData.baseMultiplier = normalArrowData.baseMultiplier;
        }
        
        // 현재 장착된 화살 UI 갱신
        SwitchArrowType(CurrentArrowType);
        
        return true;
    }
}
