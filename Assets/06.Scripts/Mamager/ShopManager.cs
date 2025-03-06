using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private BuildManager buildManager;
    
    [Header("Tower Purchase Settings")]
    public int towerCost = 50;  // 타워 구매 비용

    // 돈이 충분한지 여부 확인
    public bool HasEnoughMoney(int cost)
    {
        return GameManager.Instance.gameMoney >= cost;
    }

    // 돈 차감
    public void DeductMoney(int cost)
    {
        GameManager.Instance.gameMoney -= cost;
    }

    // 상점 버튼에서 타워 선택 시 호출 (예: 버튼 OnClick 이벤트에 연결)
    // 매개변수로 선택된 타워 프리팹을 전달
    public void OnTowerSelected()
    {
        // 구매 비용 확인
        if (HasEnoughMoney(towerCost))
        {
            // 돈 차감 후 빌드 모드로 전환
            DeductMoney(towerCost);
            buildManager.EnterBuildMode();
        }
    }
}
