using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    #region 필드 변수

    [Header("빌드 모드 셋팅")]
    [SerializeField] private UIManager uiManager;
    public BuildModeType currentBuildMode = BuildModeType.None;
    private TowerType upgradeTargetType;

    [Header("카메라 관련")]
    public Camera normalCamera;     // 평소 플레이 시 사용하는 카메라
    public Camera buildCamera;      // 빌드 모드에서 사용하는 전체 맵 시야 카메라

    [Header("노드 관리")]
    [SerializeField] private List<BuildableNode> buildableNodes = new List<BuildableNode>(); // 건설 전 노드
    [SerializeField] private List<BuildableNode> tier1Nodes = new List<BuildableNode>(); // 1단계 타워 노드
    [SerializeField] private List<BuildableNode> tier2Nodes = new List<BuildableNode>(); // 2단계 타워 노드

    private BuildableNode selectedNode;  // 선택된 노드

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

    // BuildModeType 열거형 추가
    public enum BuildModeType
    {
        None,           // 빌드 모드 아님
        NewTower,       // 새 타워 건설 모드
        UpgradeTower    // 타워 업그레이드 모드
    }

    // isBuildMode 프로퍼티로 변경
    public bool isBuildMode => currentBuildMode != BuildModeType.None;

    // 빌드 모드 진입 메서드 수정
    public void EnterBuildMode(bool isUpgradeMode = false, TowerType upgradeType = TowerType.Normal)
    {
        currentBuildMode = isUpgradeMode ? BuildModeType.UpgradeTower : BuildModeType.NewTower;
        upgradeTargetType = upgradeType;
        
        // 카메라 전환
        if (buildCamera != null && normalCamera != null)
        {
            buildCamera.gameObject.SetActive(true);
            normalCamera.gameObject.SetActive(false);
        }

        // 노드 가시성 설정
        if (isUpgradeMode)
        {
            // 업그레이드 모드에서는 1단계 타워만 표시
            foreach (BuildableNode node in tier1Nodes)
            {
                node.SetNodeVisibility(true);
                node.OnBuildModeEnter();
            }
        }
        else
        {
            // 새 타워 모드에서는 건설 가능한 노드만 표시
            foreach (BuildableNode node in buildableNodes)
            {
                node.SetNodeVisibility(true);
            }
        }
        
        // 상점 UI 숨김
        uiManager.HideShopUI();
    }

    // 타워 건설 완료 후 호출하여 빌드 모드 종료
    public void ExitBuildMode()
    {
        currentBuildMode = BuildModeType.None;

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
        
        // 건설된 모든 노드에 빌드 모드 종료 알림
        foreach (BuildableNode node in tier1Nodes)
        {
            node.OnBuildModeExit();
        }

        // 상점 UI 다시 표시
        uiManager.ShowShopUI();
    }

    #endregion

    #region 노드 관리 메서드

    // 건설 전 -> 1단계 타워로 이동
    public void RegisterTier1Node(BuildableNode node)
    {
        if (buildableNodes.Contains(node))
        {
            buildableNodes.Remove(node);
            tier1Nodes.Add(node);
        }
    }

    // 1단계 -> 2단계 타워로 이동
    public void UpgradeToTier2(BuildableNode node)
    {
        if (tier1Nodes.Contains(node))
        {
            tier1Nodes.Remove(node);
            tier2Nodes.Add(node);
        }
    }
    
    // 건설이 완료된 노드를 관리 리스트에서 제거
    public void RegisterConstructedNode(BuildableNode node)
    {
        if (buildableNodes.Contains(node))
        {
            buildableNodes.Remove(node);
            tier1Nodes.Add(node);
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
    
    // 건설 가능한 노드가 있는지 확인
    public bool HasBuildableNodes() => buildableNodes.Count > 0;

    // 업그레이드 가능한 1단계 타워가 있는지 확인
    public bool HasUpgradeableTowers() => tier1Nodes.Count > 0;

    // 선택된 노드가 1단계 타워인지 확인
    public bool IsSelectedTier1Tower() => selectedNode != null && tier1Nodes.Contains(selectedNode);

    // GetUpgradeTargetType 메서드 추가
    public TowerType GetUpgradeTargetType()
    {
        return upgradeTargetType;
    }
    
    #endregion
}