using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("Build Mode Settings")]
    public bool isBuildMode = false;
    public GameObject towerPrefab;  // 상점에서 선택된 타워 프리팹

    [Header("Camera References")]
    public Camera normalCamera;  // 평소 플레이 시 사용하는 카메라
    public Camera buildCamera;   // 빌드 모드에서 사용하는 전체 맵 시야 카메라

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 상점에서 타워 선택 시 호출하여 빌드 모드로 진입
    public void EnterBuildMode(GameObject selectedTowerPrefab)
    {
        towerPrefab = selectedTowerPrefab;
        isBuildMode = true;

        // 카메라 전환: 빌드 모드 카메라 활성화, 일반 카메라 비활성화
        if (buildCamera != null && normalCamera != null)
        {
            buildCamera.gameObject.SetActive(true);
            normalCamera.gameObject.SetActive(false);
        }
        
        // 상점 UI 숨김
        UIManager.Instance.HideShopUI();

        Debug.Log("Entered Build Mode.");
    }

    // 타워 건설 완료 후 호출하여 빌드 모드 종료
    public void ExitBuildMode()
    {
        isBuildMode = false;
        towerPrefab = null;

        // 카메라 전환: 일반 카메라 활성화, 빌드 모드 카메라 비활성화
        if (buildCamera != null && normalCamera != null)
        {
            buildCamera.gameObject.SetActive(false);
            normalCamera.gameObject.SetActive(true);
        }

        // 상점 UI 다시 표시
        UIManager.Instance.ShowShopUI();

        Debug.Log("Exited Build Mode.");
    }

    // Node 스크립트에서 호출: 선택한 노드 위치에 타워 생성
    public GameObject BuildTowerAt(Vector3 position)
    {
        if (towerPrefab == null)
        {
            Debug.LogError("No tower prefab set for build mode.");
            return null;
        }
        GameObject newTower = Instantiate(towerPrefab, position, Quaternion.identity);
        Debug.Log("Tower built at: " + position);
        return newTower;
    }
}