using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    #region 필드변수
    
    [Header("참조")]
    [SerializeField] private BuildManager buildManager;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private ArrowManager arrowManager;

    [Header("화살 버튼")]
    [SerializeField] private Button arrowUpgradeButton;
    [SerializeField] private Button explosiveArrowButton;
    [SerializeField] private Button poisonArrowButton;

    [Header("타워 버튼")]
    [SerializeField] private Button normalTowerButton;
    [SerializeField] private Button explosiveUpgradeButton;
    [SerializeField] private Button slowUpgradeButton;

    [Header("타워 가격 셋팅")]
    public int towerCost = 50;  // 타워 구매 비용
    public int explosiveUpgradeCost = 100;  // 폭발 타워 업그레이드 비용
    public int slowUpgradeCost = 100;  // 둔화 타워 업그레이드 비용

    [Header("화살 가격 셋팅")]
    public int bowUpgradeCost = 75; // 활 업그레이드 비용 (모든 화살 공통 적용)
    public int explosiveArrowCost = 150; // 폭발 화살 해금 비용
    public int poisonArrowCost = 150; // 독 화살 해금 비용
    
    #endregion

    #region 유니티 이벤트 함수
    
    private void Start()
    {
        // 타워 버튼 리스너
        normalTowerButton.onClick.AddListener(OnTowerSelected);
        explosiveUpgradeButton.onClick.AddListener(() => UpgradeTower(TowerType.Explosive));
        slowUpgradeButton.onClick.AddListener(() => UpgradeTower(TowerType.Slow));

        // 화살 버튼 리스너
        arrowUpgradeButton.onClick.AddListener(UpgradeBow);
        explosiveArrowButton.onClick.AddListener(UnlockExplosiveArrow);
        poisonArrowButton.onClick.AddListener(UnlockPoisonArrow);

        // ArrowManager의 화살 상태에 따라 버튼 활성화/비활성화 초기 설정
        UpdateArrowButtonInteractable();
    }
    
    // 화살 버튼 상태 업데이트
    private void UpdateArrowButtonInteractable()
    {
        // 폭발 화살 버튼
        if (explosiveArrowButton != null)
        {
            bool isUnlocked = arrowManager.IsArrowUnlocked(ProjectileData.ProjectileType.Explosive);
            explosiveArrowButton.interactable = !isUnlocked; // 해금되면 버튼 비활성화
        }

        // 독 화살 버튼
        if (poisonArrowButton != null)
        {
            bool isUnlocked = arrowManager.IsArrowUnlocked(ProjectileData.ProjectileType.Poison);
            poisonArrowButton.interactable = !isUnlocked; // 해금되면 버튼 비활성화
        }
    }

    #endregion

    #region 돈 확인

    private bool TryPurchase(int cost)
    {
        // 돈이 충분한지 확인
        if (GameManager.Instance.HasEnoughMoney(cost))
        {
            // 돈 차감
            GameManager.Instance.DeductMoney(cost);
            return true;
        }
        else
        {
            // 돈 부족 알림 표시
            uiManager.ShowNotEnoughMoneyPopup();
            return false;
        }
    }

    #endregion

    #region 타워 관련 메소드
    public void OnTowerSelected()
    {
        if (TryPurchase(towerCost))
        {
            buildManager.EnterBuildMode();
        }
    }

    private void UpgradeTower(TowerType upgradeType)
    {
        int cost = upgradeType == TowerType.Explosive ? explosiveUpgradeCost : slowUpgradeCost;

        // 업그레이드 가능한 1단계 타워가 없으면 리턴
        if (!buildManager.HasUpgradeableTowers())
        {
            return;
        }

        if (TryPurchase(cost))
        {
            buildManager.EnterBuildMode(true, upgradeType);
        }
    }
    #endregion

    #region 화살 관련 메소드

    // 활 업그레이드 (모든 화살에 적용)
    private void UpgradeBow()
    {
        // 돈 확인
        if (TryPurchase(bowUpgradeCost))
        {
            // 현재 화살 데이터 가져오기
            ProjectileData normalArrow = ArrowManager.Instance.GetNormalArrowData();
            
            // 업그레이드 전 데미지로 UI 표시
            uiManager.ShowArrowUpgradePopup();

            // UI 표시 후 업그레이드 실행
            bool success = arrowManager.UpgradeAllArrows();
        }
    }

    // 폭발 화살 해금
    private void UnlockExplosiveArrow()
    {
        // 해금되지 않은 경우 해금 - 돈 확인
        if (TryPurchase(explosiveArrowCost))
        {
            // 해금 요청
            bool success = arrowManager.UnlockArrow(ProjectileData.ProjectileType.Explosive);

            if (success)
            {
                // 버튼 상태 업데이트
                UpdateArrowButtonInteractable();
                // UI 업데이트
            }
        }
    }

    // 독 화살 해금
    private void UnlockPoisonArrow()
    {
        // 해금되지 않은 경우 해금 - 돈 확인
        if (TryPurchase(poisonArrowCost))
        {
            // 해금 요청
            bool success = arrowManager.UnlockArrow(ProjectileData.ProjectileType.Poison);

            if (success)
            {
                // 버튼 상태 업데이트
                UpdateArrowButtonInteractable();
                // UI 업데이트
            }
        }
    }
    
    #endregion
}
