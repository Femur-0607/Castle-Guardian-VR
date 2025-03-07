using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 감지용

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
    private Color baseColor = Color.red;       // 기본 색상 (건설 전)
    private Color towerColor = Color.white;     // 기본 색상 (건설 후)
    private Color hoverColor = Color.green;     // 마우스 올렸을 때 건설 가능한 색상
    
    [Header("머터리얼 설정")]
    [Tooltip("타워 건설 완료 후 적용할 실제 타워 머터리얼")]
    [SerializeField] private Material towerMaterial;

    private Renderer nodeRenderer;
    private BoxCollider nodeCollider;
    
    // 타워 건설 여부를 판단하는 변수
    private bool _isOccupied = false;
    public bool IsOccupied => _isOccupied;

    [Header("타워 컴포넌트")]
    [SerializeField] private ArcherTower archerTower;
    [SerializeField] private ExplosiveTower explosiveTower;
    [SerializeField] private SlowTower slowTower;

    private Tower activeTower;

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
        nodeRenderer.material.color = baseColor;

        // 시작 시 모든 타워 비활성화
        archerTower.enabled = false;
        explosiveTower.enabled = false;
        slowTower.enabled = false;
    }

    private void OnMouseEnter()
    {
        // UI 위에 있을 경우엔 클릭/오버 효과를 무시
        if (EventSystem.current.IsPointerOverGameObject()) return;
        // 빌드모드가 아닐 경우엔 효과 적용하지 않음
        if (!buildManager.isBuildMode) return;

        if (!_isOccupied) SetColor(hoverColor);  //  건설 가능하면 hoverColor색으로 머터리얼 변환
    }

    private void OnMouseExit()
    {
        if (_isOccupied) return;
        SetColor(baseColor);
    }

    private void OnMouseDown()
    {
        // 빌드모드가 아닐 경우 처리하지 않음
        if (!buildManager.isBuildMode) return;

        BuildTower();
    }

    #endregion

    #region 기초 타워 건설
    
    /// <summary>
    /// Renderer의 머터리얼 색상을 지정된 색상으로 변경합니다.
    /// </summary>
    /// <param name="color">적용할 색상</param>
    private void SetColor(Color color)
    {
        if (nodeRenderer != null && nodeRenderer.material != null)
        {
            nodeRenderer.material.color = color;
        }
    }

    /// <summary>
    /// 이 Node에 타워를 건설할 수 있으면 true를 반환합니다.
    /// </summary>
    public bool CanPlaceTower()
    {
        return !_isOccupied;
    }
    
    /// <summary>
    /// 노드의 가시성을 설정합니다. (콜라이더와 렌더러 활성화/비활성화)
    /// </summary>
    public void SetNodeVisibility(bool visible)
    {
        // 이미 건설된 노드는 렌더러는 항상 활성화 상태 유지
        if (_isOccupied)
        {
            nodeRenderer.enabled = true;
            nodeCollider.enabled = false; // 건설된 노드는 콜라이더 비활성화
        }
        else
        {
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

        _isOccupied = true;

        nodeRenderer.material = towerMaterial; // 실제 타워 머터리얼로 교체하여 건설 완료 상태를 표시

        nodeRenderer.material.color = towerColor; // 하얀색으로 색상 초기화

        buildManager.RegisterConstructedNode(this); // 건설된 노드로 등록

        buildManager.ExitBuildMode(); // 빌드 모드 종료
        
        ActivateArcherTower(); // 기본 타워 활성화
    }

    public void ActivateArcherTower()
    {
        // 이전 타워 비활성화
        DeactivateAllTowers();
        
        // 아처 타워 활성화
        archerTower.enabled = true;
        if (archerTower.gameObject != gameObject) archerTower.gameObject.SetActive(true);
        
        // 현재 활성 타워 참조 업데이트
        activeTower = archerTower;
        
        // 노드 상태 업데이트
        _isOccupied = true;
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

        // 현재 활성 타워 참조 업데이트
        activeTower = explosiveTower;
    }

    public void UpgradeToSlowTower()
    {
        // 이전 타워 비활성화
        DeactivateAllTowers();
        
        // 둔화 타워 활성화
        slowTower.enabled = true;
        if (slowTower.gameObject != gameObject) slowTower.gameObject.SetActive(true);
        
        // 현재 활성 타워 참조 업데이트
        activeTower = slowTower;
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
}