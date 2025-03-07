using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    #region 필드 변수

    [Header("빌드 모드 셋팅")]
    [SerializeField] private UIManager uiManager;
    public bool isBuildMode = false;

    [Header("카메라 관련")]
    public Camera normalCamera;     // 평소 플레이 시 사용하는 카메라
    public Camera buildCamera;      // 빌드 모드에서 사용하는 전체 맵 시야 카메라

    [Header("노드 관리")]
    [SerializeField] private List<BuildableNode> buildableNodes = new List<BuildableNode>();
    private List<BuildableNode> constructedNodes = new List<BuildableNode>();

    private BuildableNode selectedNode;  // 선택된 노드 추가

    #endregion

    #region 유니티 이벤트 함수

    private void Awake()
    {
        foreach (BuildableNode node in buildableNodes)
        {
            // 초기 상태에서는 모든 노드의 콜라이더와 렌더러 비활성화
            node.SetNodeVisibility(false);
        }
    }

    #endregion

    #region 빌드 모드 관련 로직

    // 상점에서 타워 선택 시 호출하여 빌드 모드로 진입
    public void EnterBuildMode()
    {
        isBuildMode = true;

        // 카메라 전환: 빌드 모드 카메라 활성화, 일반 카메라 비활성화
        if (buildCamera != null && normalCamera != null)
        {
            buildCamera.gameObject.SetActive(true);
            normalCamera.gameObject.SetActive(false);
        }

        // 건설 가능한 모든 노드 활성화
        foreach (BuildableNode node in buildableNodes)
        {
            node.SetNodeVisibility(true);
        }

        // 상점 UI 숨김
        uiManager.HideShopUI();
    }

    // 타워 건설 완료 후 호출하여 빌드 모드 종료
    public void ExitBuildMode()
    {
        isBuildMode = false;

        // 카메라 전환: 일반 카메라 활성화, 빌드 모드 카메라 비활성화
        if (buildCamera != null && normalCamera != null)
        {
            buildCamera.gameObject.SetActive(false);
            normalCamera.gameObject.SetActive(true);
        }

        // 모든 노드 비활성화
        foreach (BuildableNode node in buildableNodes)
        {
            node.SetNodeVisibility(false);
        }

        // 상점 UI 다시 표시
        uiManager.ShowShopUI();
    }

    #endregion

    #region 노드 관리 메서드

    // 건설이 완료된 노드를 관리 리스트에서 제거
    public void RegisterConstructedNode(BuildableNode node)
    {
        if (buildableNodes.Contains(node))
        {
            buildableNodes.Remove(node);
            constructedNodes.Add(node);
        }
    }
    
    // 선택된 노드 반환 메서드 추가
    public BuildableNode GetSelectedNode()
    {
        return selectedNode;
    }
    
    // 선택된 노드 설정 메서드 추가
    public void SelectNode(BuildableNode node)
    {
        selectedNode = node;
    }
    
    #endregion
}