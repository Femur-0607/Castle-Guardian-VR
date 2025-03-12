using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private BuildManager buildManager;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private UIManager uiManager;
    
    [Header("타워 버튼")]
    [SerializeField] private Button nomalTowerButton;
    [SerializeField] private Button explosiveUpgradeButton;
    [SerializeField] private Button slowUpgradeButton;
    
    [Header("타워 가격 셋팅")]
    public int towerCost = 50;  // 타워 구매 비용
    public int explosiveUpgradeCost = 100;  // 폭발 타워 업그레이드 비용
    public int slowUpgradeCost = 100;  // 둔화 타워 업그레이드 비용

    private void Start() 
    {
        // 타워 버튼 리스너
        nomalTowerButton.onClick.AddListener(OnTowerSelected);
        explosiveUpgradeButton.onClick.AddListener(() => UpgradeTower(TowerType.Explosive));
        slowUpgradeButton.onClick.AddListener(() => UpgradeTower(TowerType.Slow));
    }

    public void OnTowerSelected()
    {
        // 구매 비용 확인
        if (GameManager.Instance.HasEnoughMoney(towerCost))
        {
            // 돈 차감 후 빌드 모드로 전환
            GameManager.Instance.DeductMoney(towerCost);
            buildManager.EnterBuildMode();
        }
    }
    
    public void UpgradeTower(TowerType upgradeType)
    {
        int cost = upgradeType == TowerType.Explosive ? explosiveUpgradeCost : slowUpgradeCost;
        
        // 업그레이드 가능한 1단계 타워가 없으면 리턴
        if (!buildManager.HasUpgradeableTowers())
        {
            Debug.LogWarning("No upgradeable towers available");
            return;
        }
        
        // 돈 확인
        if (GameManager.Instance.HasEnoughMoney(cost))
        {
            // 돈 차감
            GameManager.Instance.DeductMoney(cost);
            
            // 업그레이드 모드로 빌드 모드 시작
            buildManager.EnterBuildMode(true, upgradeType);
        }
    }
}
