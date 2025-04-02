using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BuildManager : MonoBehaviour
{
    #region 필드 변수

    [Header("빌드 모드 셋팅")]
    [SerializeField] private UIManager uiManager;
    public BuildModeType currentBuildMode = BuildModeType.None;
    private TowerType upgradeTargetType;

    [Header("카메라 관련")]
    [SerializeField] private CameraController cameraController;  // 카메라 컨트롤러 참조

    [Header("노드 관리")]
    [SerializeField] private List<BuildableNode> buildableNodes = new List<BuildableNode>(); // 건설 전 노드
    [SerializeField] private List<BuildableNode> tier1Nodes = new List<BuildableNode>(); // 1단계 타워 노드
    [SerializeField] private List<BuildableNode> tier2Nodes = new List<BuildableNode>(); // 2단계 타워 노드

    private BuildableNode selectedNode;  // 선택된 노드

    [Header("VR 설정")]
    [SerializeField] private float rayDistance = 10f;        // VR 레이캐스트 최대 거리
    [SerializeField] private LayerMask buildableLayer;       // 건설 가능한 레이어
    private bool isBuilding;                                 // 건설 모드 상태

    private BuildableNode currentHoveredNode; // 현재 레이캐스트가 가리키는 노드

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

    private void OnEnable()
    {
        EventManager.Instance.OnBuildNodeHit += HandleBuildNodeHit;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnBuildNodeHit -= HandleBuildNodeHit;
    }

    void Update()
    {
        if (!isBuilding) return;

        // 현재 활성화된 레이캐스트의 결과를 가져옴
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, transform.forward, out hit, rayDistance, buildableLayer))
        {
            BuildableNode node = hit.collider.GetComponent<BuildableNode>();
            if (node != null)
            {
                // 새로운 노드를 가리킬 때
                if (node != currentHoveredNode)
                {
                    // 이전 노드에서 벗어남
                    if (currentHoveredNode != null)
                        currentHoveredNode.OnRaycastExit();
                    
                    // 새 노드 진입
                    currentHoveredNode = node;
                    currentHoveredNode.OnRaycastEnter();
                }
            }
        }
        else if (currentHoveredNode != null)
        {
            // 레이캐스트가 노드에서 벗어남
            currentHoveredNode.OnRaycastExit();
            currentHoveredNode = null;
        }
    }

    private void HandleBuildNodeHit()
    {
        if (currentHoveredNode != null)
        {
            currentHoveredNode.OnRaycastHit();
            OVRInput.SetControllerVibration(0.3f, 0.3f, OVRInput.Controller.LTouch);
            StartCoroutine(StopVibration());
        }
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
            cameraController.SwitchToBuildMode();
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
            // 새 타워 모드에서는 건설 가능한 노드(0단계)만 표시
            foreach (BuildableNode node in buildableNodes)
            {
                node.SetNodeVisibility(true);
                node.OnBuildModeEnter();
            }
        }
        
        // 상점 UI 숨김
        uiManager.HideShopUI();
    }

    // 타워 건설 완료 후 호출하여 빌드 모드 종료
    public void ExitBuildMode()
    {
        // 빌드 모드에서 원래 위치로 복귀
        if (cameraController != null)
        {
            cameraController.RestoreFromBuildMode();
        }

        // 모든 노드에 빌드 모드 종료 알림
        foreach (BuildableNode node in buildableNodes)
        {
            node.OnBuildModeExit();
        }
        
        foreach (BuildableNode node in tier1Nodes)
        {
            node.OnBuildModeExit();
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

    // 햅틱 피드백 중지
    private IEnumerator StopVibration()
    {
        yield return new WaitForSeconds(0.1f);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }

    // 건설 모드 토글
    public void ToggleBuildMode(bool enable)
    {
        isBuilding = enable;
    }
}