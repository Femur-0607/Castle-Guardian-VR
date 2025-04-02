using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
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
    [SerializeField] private Button nomalTowerButton;
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

    private void Start() 
    {
        // 타워 버튼 리스너
        nomalTowerButton.onClick.AddListener(OnTowerSelected);
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
            Text buttonText = explosiveArrowButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                bool isUnlocked = arrowManager.IsArrowUnlocked(ProjectileData.ProjectileType.Explosive);
                buttonText.text = isUnlocked ? "업그레이드됨" : "해금";
                explosiveArrowButton.interactable = !isUnlocked; // 해금되면 버튼 비활성화
            }
        }
        
        // 독 화살 버튼
        if (poisonArrowButton != null)
        {
            Text buttonText = poisonArrowButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                bool isUnlocked = arrowManager.IsArrowUnlocked(ProjectileData.ProjectileType.Poison);
                buttonText.text = isUnlocked ? "업그레이드됨" : "해금";
                poisonArrowButton.interactable = !isUnlocked; // 해금되면 버튼 비활성화
            }
        }
        
        // 활 업그레이드 버튼은 항상 활성화 상태 유지
        if (arrowUpgradeButton != null)
        {
            Text buttonText = arrowUpgradeButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = "활 업그레이드";
            }
        }
    }

    #region 타워 관련 메소드
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
    #endregion

    #region 화살 관련 메소드
    // 활 업그레이드 (모든 화살에 적용)
    public void UpgradeBow()
    {
        // 돈 확인
        if (GameManager.Instance.HasEnoughMoney(bowUpgradeCost))
        {
            // 돈 차감
            GameManager.Instance.DeductMoney(bowUpgradeCost);
            
            // 모든 화살 공통 업그레이드
            bool success = arrowManager.UpgradeAllArrows();
            
            // 성공 시 효과음 재생 또는 UI 업데이트 등 추가 가능
            if (success)
            {
                Debug.Log("활 업그레이드 성공!");
                // 효과음 재생
                SoundManager.Instance.PlaySound("Upgrade");
                // UI 업데이트 메소드 호출 (필요시)
                // uiManager.UpdateArrowUI();
            }
        }
        else
        {
            Debug.Log("돈이 부족합니다!");
            SoundManager.Instance.PlaySound("Error");
        }
    }

    // 폭발 화살 해금
    public void UnlockExplosiveArrow()
    {
        // 이미 해금되었는지 확인
        if (arrowManager.IsArrowUnlocked(ProjectileData.ProjectileType.Explosive))
        {
            Debug.Log("이미 해금된 화살입니다!");
            SoundManager.Instance.PlaySound("Error");
            return;
        }
        
        // 해금되지 않은 경우 해금 - 돈 확인
        if (GameManager.Instance.HasEnoughMoney(explosiveArrowCost))
        {
            // 돈 차감
            GameManager.Instance.DeductMoney(explosiveArrowCost);
            
            // 해금 요청
            bool success = arrowManager.UnlockArrow(ProjectileData.ProjectileType.Explosive);
            
            if (success)
            {
                Debug.Log("폭발 화살 해금 성공!");
                SoundManager.Instance.PlaySound("Unlock");
                // 버튼 상태 업데이트
                UpdateArrowButtonInteractable();
                // UI 업데이트
            }
        }
        else
        {
            Debug.Log("돈이 부족합니다!");
            SoundManager.Instance.PlaySound("Error");
        }
    }

    // 독 화살 해금
    public void UnlockPoisonArrow()
    {
        // 이미 해금되었는지 확인
        if (arrowManager.IsArrowUnlocked(ProjectileData.ProjectileType.Poison))
        {
            Debug.Log("이미 해금된 화살입니다!");
            SoundManager.Instance.PlaySound("Error");
            return;
        }
        
        // 해금되지 않은 경우 해금 - 돈 확인
        if (GameManager.Instance.HasEnoughMoney(poisonArrowCost))
        {
            // 돈 차감
            GameManager.Instance.DeductMoney(poisonArrowCost);
            
            // 해금 요청
            bool success = arrowManager.UnlockArrow(ProjectileData.ProjectileType.Poison);
            
            if (success)
            {
                Debug.Log("독 화살 해금 성공!");
                SoundManager.Instance.PlaySound("Unlock");
                // 버튼 상태 업데이트
                UpdateArrowButtonInteractable();
                // UI 업데이트
            }
        }
        else
        {
            Debug.Log("돈이 부족합니다!");
            SoundManager.Instance.PlaySound("Error");
        }
    }
    #endregion
}
