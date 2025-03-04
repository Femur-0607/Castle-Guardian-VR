using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 감지용

public class Node : MonoBehaviour
{
    private GameObject tower;  // 이 노드에 건설된 타워 (없으면 null)

    [Header("Node Visual Settings")]
    public Color hoverColor = Color.yellow;   // 마우스 올렸을 때 색상
    private Color startColor = Color.white;   // 기본 색상

    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            startColor = rend.material.color;
        }
    }

    private void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!BuildManager.Instance.isBuildMode || tower != null) return;

        SetColor(hoverColor);
    }

    private void OnMouseExit()
    {
        SetColor(startColor);
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!BuildManager.Instance.isBuildMode) return;
        if (tower != null)
        {
            Debug.Log("Tower already built on this node.");
            return;
        }

        // 타워를 해당 노드 위치에 건설
        tower = BuildManager.Instance.BuildTowerAt(transform.position);

        // 타워 건설 완료 후 빌드 모드 종료(카메라 전환 및 상점 UI 재오픈)
        BuildManager.Instance.ExitBuildMode();
    }

    private void SetColor(Color color)
    {
        if (rend != null)
            rend.material.color = color;
    }
}
