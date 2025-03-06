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
    [Tooltip("건설 미리보기(고스트) 머터리얼 (예: BlueprintEffectV3)")]
    [SerializeField] private Material ghostMaterial;
    [Tooltip("타워 건설 완료 후 적용할 실제 타워 머터리얼")]
    [SerializeField] private Material towerMaterial;

    private Renderer renderer;
    private BoxCollider boxCollider;
    // 타워 건설 여부를 판단하는 변수
    private bool isOccupied = false;

    #endregion

    #region 유니티 이벤트 함수
    
    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        boxCollider = GetComponent<BoxCollider>();
        
        renderer.material = ghostMaterial;  // 초기 상태: 고스트 머터리얼로 설정
        SetColor(baseColor);
        renderer.material.color = baseColor;
    }

    private void OnMouseEnter()
    {
        // UI 위에 있을 경우엔 클릭/오버 효과를 무시
        if (EventSystem.current.IsPointerOverGameObject()) return;
        // 빌드모드가 아닐 경우엔 효과 적용하지 않음
        if (!buildManager.isBuildMode) return;

        if (!isOccupied) SetColor(hoverColor);  //  건설 가능하면 hoverColor색으로 머터리얼 변환
    }

    private void OnMouseExit()
    {
        if (isOccupied) return;
        SetColor(baseColor);
    }

    private void OnMouseDown()
    {
        // 빌드모드가 아닐 경우 처리하지 않음
        if (!buildManager.isBuildMode) return;

        BuildTower();
    }
    
    #endregion

    #region Public 메서드

    /// <summary>
    /// 이 Node에 타워를 건설할 수 있으면 true를 반환합니다.
    /// </summary>
    public bool CanPlaceTower()
    {
        return !isOccupied;
    }
    
    /// <summary>
    /// 노드의 가시성을 설정합니다. (콜라이더와 렌더러 활성화/비활성화)
    /// </summary>
    public void SetNodeVisibility(bool visible)
    {
        // 이미 건설된 노드는 렌더러는 항상 활성화 상태 유지
        if (isOccupied)
        {
            renderer.enabled = true;
            boxCollider.enabled = false; // 건설된 노드는 콜라이더 비활성화
        }
        else
        {
            renderer.enabled = visible;
            boxCollider.enabled = visible;
        }
    }

    /// <summary>
    /// BuildManager가 호출하여 이 Node에 타워를 건설합니다.
    /// 실제로는 고스트 머터리얼을 실제 타워 머터리얼로 전환합니다.
    /// </summary>
    public void BuildTower()
    {
        if (!CanPlaceTower()) return;

        isOccupied = true;

        // 실제 타워 머터리얼로 교체하여 건설 완료 상태를 표시
        renderer.material = towerMaterial;

        // 하얀색으로 색상 초기화
        renderer.material.color = towerColor;

        // 건설된 노드로 등록
        buildManager.RegisterConstructedNode(this);
        
        // 콜라이더 비활성화 (더 이상 클릭할 필요 없음)
        boxCollider.enabled = false;
        
        // 빌드 모드 종료
        buildManager.ExitBuildMode();
        // 추가: 타워 동작 스크립트 활성화 등 추가 로직을 넣을 수 있음
    }

    #endregion

    #region Helper 메서드

    /// <summary>
    /// Renderer의 머터리얼 색상을 지정된 색상으로 변경합니다.
    /// </summary>
    /// <param name="color">적용할 색상</param>
    private void SetColor(Color color)
    {
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = color;
        }
    }

    #endregion
}