using UnityEngine;
using System.Collections.Generic;

public class GhostSpawner : MonoBehaviour
{
    [Header("유령 설정")]
    [SerializeField] private GameObject ghostPrefab;    // GhostEnemy 프리팹 
    [SerializeField] private Transform[] spawnPoints;   // 스폰 위치 배열 (없으면 자동 생성)
    [SerializeField] private GameObject ovrCameraRig;   // OVR 카메라 참조

    [Header("경계 설정")]
    [SerializeField] private Vector3 boundaryMin = new Vector3(-30, 35, -30);
    [SerializeField] private Vector3 boundaryMax = new Vector3(30, 35, 30);

    [Header("스폰 설정")]
    [SerializeField] private int maxGhosts = 3;         // 최대 유령 수

    private List<GameObject> activeGhosts = new List<GameObject>();

    private void Start()
    {
        // OVR 카메라 찾기
        if (ovrCameraRig == null)
        {
            ovrCameraRig = GameObject.FindWithTag("MainCamera");
        }

        // 스폰 포인트가 없으면 자동 생성
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            CreateDefaultSpawnPoints();
        }
    }

    private void CreateDefaultSpawnPoints()
    {
        spawnPoints = new Transform[4];
        GameObject spawnPointsObj = new GameObject("SpawnPoints");
        spawnPointsObj.transform.parent = transform;

        // 네 방향에 스폰 포인트 생성
        for (int i = 0; i < 4; i++)
        {
            GameObject point = new GameObject($"SpawnPoint_{i}");
            point.transform.parent = spawnPointsObj.transform;
            
            float angle = i * 90f;
            float radius = 25f;
            float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float z = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            
            point.transform.position = new Vector3(x, 35f, z);
            spawnPoints[i] = point.transform;
        }
    }

    private void Update()
    {
        // 왼쪽 컨트롤러 X 버튼(Button.One) 눌렀을 때 유령 스폰
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            if (activeGhosts.Count < maxGhosts)
            {
                SpawnGhost();
            }
            else
            {
                Debug.Log("최대 유령 수에 도달했습니다.");
            }
        }

        // Y 버튼 눌렀을 때 모든 유령 제거 (테스트용)
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch))
        {
            DestroyAllGhosts();
        }

        // 유효하지 않은 유령 정리
        CleanupGhostsList();
    }

    private void CleanupGhostsList()
    {
        for (int i = activeGhosts.Count - 1; i >= 0; i--)
        {
            if (activeGhosts[i] == null)
            {
                activeGhosts.RemoveAt(i);
            }
        }
    }

    public void SpawnGhost()
    {
        if (ghostPrefab == null)
        {
            Debug.LogError("Ghost 프리팹이 설정되지 않았습니다.");
            return;
        }

        // 랜덤 스폰 위치 선택
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // 유령 생성
        GameObject ghostObj = Instantiate(ghostPrefab, spawnPoint.position, Quaternion.identity);
        GhostAgent ghostAgent = ghostObj.GetComponent<GhostAgent>();

        if (ghostAgent != null)
        {
            // GhostAgent 스크립트 수정 필요 (OVR Camera Rig 참조 추가)
            ghostAgent.ovrCameraRig = ovrCameraRig;
            
            // 경계 설정
            ghostAgent.SetBoundaries(boundaryMin, boundaryMax);
            
            // 강제로 Y 위치 35로 고정
            Vector3 pos = ghostObj.transform.position;
            ghostObj.transform.position = new Vector3(pos.x, 35f, pos.z);
            
            // 활성 유령 목록에 추가
            activeGhosts.Add(ghostObj);
            
            Debug.Log("유령 스폰 성공: " + ghostObj.name);
        }
    }

    public void DestroyAllGhosts()
    {
        foreach (var ghost in activeGhosts)
        {
            if (ghost != null)
            {
                Destroy(ghost);
            }
        }
        activeGhosts.Clear();
        Debug.Log("모든 유령이 제거되었습니다.");
    }
}
