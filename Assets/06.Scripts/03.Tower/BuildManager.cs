using UnityEngine;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    #region 필드 변수

    public static BuildManager instance;

    [Header("빌드 모드 셋팅")]
    [SerializeField] private UIManager uiManager;
    public BuildModeType currentBuildMode = BuildModeType.None;
    private TowerType upgradeTargetType;
    private bool isBuilding = false;   // 건설 모드 상태

    [Header("카메라 관련")]
    [SerializeField] private CameraController cameraController;  // 카메라 컨트롤러 참조

    [Header("노드 관리")]
    [SerializeField] private List<BuildableNode> buildableNodes = new List<BuildableNode>(); // 건설 전 노드
    [SerializeField] private List<BuildableNode> tier1Nodes = new List<BuildableNode>(); // 1단계 타워 노드
    [SerializeField] private List<BuildableNode> tier2Nodes = new List<BuildableNode>(); // 2단계 타워 노드

    private BuildableNode selectedNode;  // 선택된 노드
    private int currentNodeIndex = 0;  // 현재 선택된 노드 인덱스

    [Header("VR 설정")]
    [SerializeField] private Transform leftControllerAnchor;

    #endregion

    #region 유니티 이벤트 함수

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (BuildableNode node in buildableNodes)
        {
            // 초기 상태에서는 모든 노드의 콜라이더와 렌더러 비활성화
            node.SetNodeVisibility(false);
        }
    }

    private void OnEnable()
    {
        EventManager.Instance.OnBuildNodeHit += HandleBuildNodeHit;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnBuildNodeHit -= HandleBuildNodeHit;
    }

    #endregion

    #region VR 노드 선택 관련

    public void MoveNodeSelection(int direction)
    {
        List<BuildableNode> currentNodeList = GetCurrentNodeList();
        if (currentNodeList.Count == 0) return;

        // 현재 인덱스 갱신
        currentNodeIndex += direction;
        
        // 인덱스 순환 처리
        if (currentNodeIndex >= currentNodeList.Count)
        {
            currentNodeIndex = 0;
        }
        else if (currentNodeIndex < 0)
        {
            currentNodeIndex = currentNodeList.Count - 1;
        }
        
        // 새 노드 선택
        SelectNode(currentNodeList[currentNodeIndex]);
    }

    private List<BuildableNode> GetCurrentNodeList()
    {
        switch (currentBuildMode)
        {
            case BuildModeType.NewTower:
                return buildableNodes;
            case BuildModeType.UpgradeTower:
                return tier1Nodes;
            default:
                return buildableNodes;
        }
    }

    private void SelectNode(BuildableNode node)
    {
        // 이전 선택 노드 색상 초기화
        if (selectedNode != null)
        {
            selectedNode.SetColor(selectedNode.baseColor);
        }
        
        // 새 노드 선택 및 색상 변경
        selectedNode = node;
        selectedNode.SetColor(selectedNode.hoverColor);
    }

    private void HandleBuildNodeHit()
    {
        if (!isBuilding || selectedNode == null) return;

        // 빌드 모드에 따른 처리
        switch (currentBuildMode)
        {
            case BuildModeType.NewTower:
                selectedNode.BuildTower();
                break;
            case BuildModeType.UpgradeTower:
                if (upgradeTargetType == TowerType.Explosive)
                {
                    selectedNode.UpgradeToExplosiveTower();
                }
                else if (upgradeTargetType == TowerType.Slow)
                {
                    selectedNode.UpgradeToSlowTower();
                }
                break;
        }

        ExitBuildMode();
    }

    #endregion

    #region 빌드 모드 관련 로직

    public enum BuildModeType
    {
        None,           // 빌드 모드 아님
        NewTower,       // 새 타워 건설 모드
        UpgradeTower    // 타워 업그레이드 모드
    }

    public bool isBuildMode => currentBuildMode != BuildModeType.None;

    // 빌드 모드 진입 메서드 수정
    public void EnterBuildMode(bool isUpgradeMode = false, TowerType upgradeType = TowerType.Normal)
    {
        currentBuildMode = isUpgradeMode ? BuildModeType.UpgradeTower : BuildModeType.NewTower;
        upgradeTargetType = upgradeType;

        // 빌드 모드로 카메라 전환
        if (cameraController != null)
        {
            cameraController.SwitchCamera(CameraController.CameraPosition.Build);
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
            // 첫 번째 노드 선택
            if (tier1Nodes.Count > 0)
            {
                SelectNode(tier1Nodes[0]);
            }
        }
        else
        {
            // 새 타워 모드에서는 건설 가능한 노드(0단계)만 표시
            foreach (BuildableNode node in buildableNodes)
            {
                node.SetNodeVisibility(true);
                node.OnBuildModeEnter();
            }
            // 첫 번째 노드 선택
            if (buildableNodes.Count > 0)
            {
                SelectNode(buildableNodes[0]);
            }
        }

        // 상점 UI 숨김
        uiManager.HideShopUI();
        
        // 빌드 모드 활성화
        ToggleBuildMode(true);
    }

    // 빌드 모드 종료
    public void ExitBuildMode()
    {
        // 빌드 모드 비활성화
        ToggleBuildMode(false);
        
        // 선택된 노드 초기화
        if (selectedNode != null)
        {
            selectedNode.SetColor(selectedNode.baseColor);
            selectedNode = null;
        }
        
        // 빌드 모드에서 원래 위치로 복귀
        if (cameraController != null)
        {
            cameraController.SwitchCamera(CameraController.CameraPosition.UI);
        }

        // 모든 노드에 빌드 모드 종료 알림
        foreach (BuildableNode node in buildableNodes)
        {
            node.OnBuildModeExit();
            node.SetNodeVisibility(false);  // 노드 비활성화 추가
        }
        
        foreach (BuildableNode node in tier1Nodes)
        {
            node.OnBuildModeExit();
            node.SetNodeVisibility(false);  // 노드 비활성화 추가
        }
        
        // 빌드 모드 상태 초기화
        currentBuildMode = BuildModeType.None;

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

    // 건설 모드 토글
    public void ToggleBuildMode(bool enable)
    {
        isBuilding = enable;
    }
}