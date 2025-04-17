using UnityEngine;

/// <summary>
/// BuildableNode는 씬 내 타워 건설 위치를 관리합니다.
/// - 기본 상태: 고스트 머터리얼(예: BlueprintEffectV3)로 회색 표시
/// - 마우스 오버 시: 건설 가능하면 초록, 이미 건설되어 있으면 빨강으로 표시
/// - 빌드모드에서 클릭하면, BuildManager를 통해 타워 건설(머터리얼 교체 및 타워 동작 활성화)
/// </summary>
public class BuildableNode : MonoBehaviour
{
    #region 필드 변수

    [SerializeField] private BuildManager buildManager;

    [Header("노드 시각 셋팅")]
    public Color baseColor = Color.red;       // 기본 색상 (건설 전)
    public Color hoverColor = Color.green;     // 건설 가능한 타워 색상
    private Color towerColor = Color.white;     // 기본 아처 타워 색상
    private Color explosiveTowerColor = Color.red;    // 폭발 타워 색상 
    private Color slowTowerColor = Color.green;  // 슬로우 타워 색상 (초록색)
    
    [Header("머터리얼 설정")]
    [SerializeField] private Material initialMaterial;
    [SerializeField] private Material towerMaterial;

    private Renderer nodeRenderer;
    private BoxCollider nodeCollider;
    
    private int towerLevel; // 0: 건설 전, 1: 1단계, 2: 2단계
    // 호환성을 위한 프로퍼티
    public bool IsOccupied => towerLevel > 0;
    

    [Header("타워 컴포넌트")]
    [SerializeField] private ArcherTower archerTower;
    [SerializeField] private ExplosiveTower explosiveTower;
    [SerializeField] private SlowTower slowTower;
    
    private TowerType currentTowerType = TowerType.Normal;

    #endregion

    #region 유니티 이벤트 함수

    private void Awake()
    {
        nodeRenderer = GetComponent<Renderer>();
        nodeCollider = GetComponent<BoxCollider>();
        archerTower = GetComponent<ArcherTower>();
        explosiveTower = GetComponent<ExplosiveTower>();
        slowTower = GetComponent<SlowTower>();

        SetColor(baseColor);
        nodeRenderer.material = initialMaterial;
        nodeRenderer.material.color = baseColor;

        // 시작 시 모든 타워 비활성화
        archerTower.enabled = false;
        explosiveTower.enabled = false;
        slowTower.enabled = false;
    }
    
    #endregion

    #region 기초 타워 건설
    
    /// <summary>
    /// Renderer의 머터리얼 색상을 지정된 색상으로 변경합니다.
    /// </summary>
    /// <param name="color">적용할 색상</param>
    public void SetColor(Color color)
    {
        if (nodeRenderer != null && nodeRenderer.material != null)
        {
            nodeRenderer.material.color = color;
        }
    }

    /// <summary>
    /// 이 Node에 타워를 건설할 수 있으면 true를 반환합니다.
    /// </summary>
    private bool CanPlaceTower()
    {
        return towerLevel == 0;
    }
    
    /// <summary>
    /// 노드의 가시성을 설정합니다. (콜라이더와 렌더러 활성화/비활성화)
    /// </summary>
    public void SetNodeVisibility(bool visible)
    {
        // 일반 모드에서의 렌더링 설정
        if (!buildManager.isBuildMode)
        {
            // 0단계(건설 전) 노드는 렌더러 꺼짐
            if (towerLevel == 0)
            {
                nodeRenderer.enabled = false;
                nodeCollider.enabled = false;
            }
            // 1단계 이상 타워는 렌더러 켜짐
            else
            {
                nodeRenderer.enabled = true;
                nodeCollider.enabled = false;
            }
        }
        // 빌드 모드에서의 렌더링 설정
        else
        {
            // 표시하려는 노드만 렌더러 켜짐
            nodeRenderer.enabled = visible;
            nodeCollider.enabled = visible;
        }
    }

    /// <summary>
    /// BuildManager가 호출하여 이 Node에 타워를 건설합니다.
    /// 실제로는 고스트 머터리얼을 실제 타워 머터리얼로 전환합니다.
    /// </summary>
    public void BuildTower()
    {
        if (!CanPlaceTower()) return;

        towerLevel = 1; // 1단계 타워로 설정

        nodeRenderer.material = towerMaterial;
        nodeRenderer.material.color = towerColor;

        // 1단계 타워 노드로 등록
        buildManager.RegisterTier1Node(this);
        
        buildManager.ExitBuildMode();
        
        ActivateArcherTower();
    }

    private void ActivateArcherTower()
    {
        // 이전 타워 비활성화
        DeactivateAllTowers();
        
        // 아처 타워 활성화
        archerTower.enabled = true;
        if (archerTower.gameObject != gameObject) archerTower.gameObject.SetActive(true);
        
        // 노드 상태 업데이트
        towerLevel = 1;
    }

    #endregion

    #region 타워 업그레이드

    public void UpgradeToExplosiveTower()
    {
        // 이전 타워 비활성화
        DeactivateAllTowers();

        // 폭발 타워 활성화
        explosiveTower.enabled = true;
        if (explosiveTower.gameObject != gameObject) explosiveTower.gameObject.SetActive(true);

        // 타워 레벨과 타입 업데이트
        towerLevel = 2;

        nodeRenderer.material = towerMaterial;
        nodeRenderer.material.color = explosiveTowerColor;  // 폭발 타워용 색상

        // 2단계 타워로 등록
        buildManager.UpgradeToTier2(this);

        currentTowerType = TowerType.Explosive;
    }

    public void UpgradeToSlowTower()
    {
        // 이전 타워 비활성화
        DeactivateAllTowers();

        // 둔화 타워 활성화
        slowTower.enabled = true;
        if (slowTower.gameObject != gameObject) slowTower.gameObject.SetActive(true);

        // 타워 레벨과 타입 업데이트
        towerLevel = 2;

        nodeRenderer.material = towerMaterial;
        nodeRenderer.material.color = slowTowerColor;  // 슬로우 타워용 색상

        // 2단계 타워로 등록
        buildManager.UpgradeToTier2(this);

        currentTowerType = TowerType.Slow;
    }

    /// <summary>
    /// 타워 업그레이드 시 초기화
    /// </summary>
    private void DeactivateAllTowers()
    {
        // 모든 타워 컴포넌트 비활성화
        archerTower.enabled = false;
        explosiveTower.enabled = false;
        slowTower.enabled = false;

        // 타워의 게임오브젝트도 비활성화 (시각적 요소)
        if (archerTower.gameObject != gameObject) archerTower.gameObject.SetActive(false);
        if (explosiveTower.gameObject != gameObject) explosiveTower.gameObject.SetActive(false);
        if (slowTower.gameObject != gameObject) slowTower.gameObject.SetActive(false);
    }

    #endregion
    
    #region 빌드 모드 관련 메서드
    
    /// <summary>
    /// 빌드 모드 진입 시 호출되는 메서드
    /// </summary>
    public void OnBuildModeEnter()
    {
        // 0단계나 1단계 타워만 머티리얼 변경
        if (towerLevel == 0 || towerLevel == 1)
        {
            // 초기 머티리얼로 변경하고 baseColor 적용
            nodeRenderer.material = initialMaterial;
            nodeRenderer.material.color = baseColor;
        }
        else if (towerLevel == 2)
        {
            // 2단계 타워도 초기 머터리얼로 변경
            nodeRenderer.material = initialMaterial;
            nodeRenderer.material.color = baseColor;
        }
    }

    /// <summary>
    /// 빌드 모드 종료 시 호출되는 메서드
    /// </summary>
    public void OnBuildModeExit()
    {
        // 0단계 노드는 비활성화
        if (towerLevel == 0)
        {
            nodeRenderer.enabled = false;
        }
        // 1단계 타워는 타워 머티리얼로 변경
        else if (towerLevel == 1)
        {
            nodeRenderer.material = towerMaterial;
            nodeRenderer.material.color = towerColor;
        }
         else if (towerLevel == 2)
        {
            nodeRenderer.material = towerMaterial;
            
            // 타워 타입으로 분기
            if (currentTowerType == TowerType.Explosive)
            {
                nodeRenderer.material.color = explosiveTowerColor;
            }
            else if (currentTowerType == TowerType.Slow)
            {
                nodeRenderer.material.color = slowTowerColor;
            }
        }
    }
    
    #endregion
}