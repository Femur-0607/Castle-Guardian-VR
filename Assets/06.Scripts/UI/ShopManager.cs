using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private BuildManager buildManager;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private UIManager uiManager;
    
    [Header("Tower Purchase Settings")]
    public int towerCost = 50;  // 타워 구매 비용
    public int explosiveUpgradeCost = 75;  // 폭발 타워 업그레이드 비용
    public int slowUpgradeCost = 50;  // 둔화 타워 업그레이드 비용

    // 상점 버튼에서 타워 선택 시 호출 (예: 버튼 OnClick 이벤트에 연결)
    // 매개변수로 선택된 타워 프리팹을 전달
    private void OnTowerSelected()
    {
        // 구매 비용 확인
        if (GameManager.Instance.HasEnoughMoney(towerCost))
        {
            // 돈 차감 후 빌드 모드로 전환
            GameManager.Instance.DeductMoney(towerCost);
            buildManager.EnterBuildMode();
        }
    }

    /// <summary>
    /// 상점 나가기 버튼 클릭 시 작동
    /// 다음 웨이브 시작 신호 보내기
    /// </summary>
    public void StartNextWave()
    {
        // 상점 패널 먼저 닫기
        uiManager.HideShopUI();

        // WaveManager에게 다음 웨이브 시작을 요청
        waveManager.StartNextWaveEvent();
    }

    public void PurchaseTowerUpgrade(TowerType upgradeType)
    {
        // 선택된 노드 확인
        BuildableNode selectedNode = buildManager.GetSelectedNode();
        
        if (selectedNode == null || !selectedNode.IsOccupied)
        {
            return;
        }
        
        int cost = upgradeType == TowerType.Explosive ? explosiveUpgradeCost : slowUpgradeCost;
        
        if (GameManager.Instance.HasEnoughMoney(cost))
        {
            GameManager.Instance.DeductMoney(cost);
            
            // 타워 업그레이드
            if (upgradeType == TowerType.Explosive)
            {
                selectedNode.UpgradeToExplosiveTower();
            }
            else if (upgradeType == TowerType.Slow)
            {
                selectedNode.UpgradeToSlowTower();
            }
        }
    }
}
