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

    private bool towerConstructionPossible;  // 타워 건설 여부 확인 불타입 변수

    [Header("노드 시각 셋팅")]
    private Color baseColor = Color.gray;   // 기본 색상 (건설 전)
    private Color hoverColor = Color.green;   // 마우스 올렸을 때 건설 가능한 색상
    private Color occupiedColor = Color.red;      // 이미 타워가 건설되어 있으면 빨간색
    
    [Header("머터리얼 설정")]
    [Tooltip("건설 미리보기(고스트) 머터리얼 (예: BlueprintEffectV3)")]
    [SerializeField] private Material ghostMaterial;
    [Tooltip("타워 건설 완료 후 적용할 실제 타워 머터리얼")]
    [SerializeField] private Material towerMaterial;

    // Renderer 컴포넌트 (Node의 시각적 표현)
    private Renderer rend;
    // 타워 건설 여부를 판단하는 변수
    private bool isOccupied = false;

    #endregion

    #region 유니티 이벤트 함수
    
    private void Awake()
    {
        rend = GetComponent<Renderer>();
        
        rend.material = ghostMaterial;  // 초기 상태: 고스트 머터리얼로 설정
        SetColor(baseColor);
        rend.material.color = baseColor;

        if (isOccupied)
        {
            SetColor(occupiedColor);    // 건설이 되어있는곳은 impossibleColor로 초기화
        }
    }

    private void OnMouseEnter()
    {
        // UI 위에 있을 경우엔 클릭/오버 효과를 무시
        if (EventSystem.current.IsPointerOverGameObject()) return;
        // 빌드모드가 아닐 경우엔 효과 적용하지 않음
        if (!BuildManager.Instance.isBuildMode) return;

        if (!isOccupied) SetColor(hoverColor);  //  건설 가능하면 hoverColor색으로 머터리얼 변환
    }

    private void OnMouseExit()
    {
        SetColor(baseColor);
    }

    private void OnMouseDown()
    {
        // UI 위에서 발생한 클릭은 무시
        if (EventSystem.current.IsPointerOverGameObject()) return;
        // 빌드모드가 아닐 경우 처리하지 않음
        if (!BuildManager.Instance.isBuildMode) return;

        // BuildManager에 건설 요청 (Node의 위치와 상태를 넘김)
        BuildManager.Instance.BuildTowerAt(this);
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
    /// BuildManager가 호출하여 이 Node에 타워를 건설합니다.
    /// 실제로는 고스트 머터리얼을 실제 타워 머터리얼로 전환합니다.
    /// </summary>
    public void BuildTower()
    {
        if (!CanPlaceTower())
        {
            Debug.Log("BuildableNode: Tower already built on " + gameObject.name);
            return;
        }

        isOccupied = true;

        // 실제 타워 머터리얼로 교체하여 건설 완료 상태를 표시
        if (towerMaterial != null)
        {
            rend.material = towerMaterial;
        }
        else
        {
            Debug.LogWarning("BuildableNode: Tower material is not assigned on " + gameObject.name);
        }

        Debug.Log("BuildableNode: Tower built on " + gameObject.name);

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
        if (rend != null && rend.material != null)
        {
            rend.material.color = color;
        }
    }

    #endregion
}