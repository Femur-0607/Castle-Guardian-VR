using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class GhostTrainingManager : MonoBehaviour
{
    #region 필드 변수

    [Header("기본 설정")]
    public GameObject ghostPrefab;
    public Transform playerCamera;
    public int numGhosts = 10;               // 스폰할 유령 프리팹의 수
    public float respawnTime = 15f;
    public bool enableRandomCameraSwitch = true;
    
    [Header("경계 설정")]
    public Transform boundaryMinPoint;  // 이동 영역 왼쪽 아래 꼭지점
    public Transform boundaryMaxPoint;  // 이동 영역 오른쪽 위 꼭지점
    public Transform spawnMinPoint;     // 스폰 영역 왼쪽 아래 꼭지점 
    public Transform spawnMaxPoint;     // 스폰 영역 오른쪽 위 꼭지점
    
    // 경계 변수
    private Vector3 boundaryMin;
    private Vector3 boundaryMax;
    
    private List<GhostAgent> ghosts = new List<GhostAgent>();

    #endregion

    #region 유니티 이벤트 함수
    
    void Start()
    {
        // 필수 컴포넌트 초기화
        InitializeComponents();
        
        // 경계 계산
        CalculateBoundaries();
        
        // 초기 유령 생성
        for (int i = 0; i < numGhosts; i++)
        {
            SpawnGhost();
        }
        
        // 랜덤 카메라 전환 시작
        if (enableRandomCameraSwitch)
        {
            StartCoroutine(RandomCameraSwitch());
        }
    }
    
    void Update()
    {
        // 비활성화된 유령 재활성화
        for (int i = ghosts.Count - 1; i >= 0; i--)
        {
            if (ghosts[i] == null)
            {
                ghosts.RemoveAt(i);
                continue;
            }
            
            if (!ghosts[i].gameObject.activeSelf)
            {
                StartCoroutine(RespawnGhostAfterDelay(ghosts[i], respawnTime));
            }
        }
    }
    
    // 에디터에서 경계와 스폰 영역 시각화
    private void OnDrawGizmos()
    {
        // 경계 시각화 (검정색 플랜)
        if (boundaryMinPoint != null && boundaryMaxPoint != null)
        {
            Gizmos.color = Color.black;
            Vector3 center = (boundaryMinPoint.position + boundaryMaxPoint.position) * 0.5f;
            Vector3 size = boundaryMaxPoint.position - boundaryMinPoint.position;
            Gizmos.DrawWireCube(center, size);
            
            // 각 꼭지점 표시
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(boundaryMinPoint.position, 0.5f);
            Gizmos.DrawSphere(boundaryMaxPoint.position, 0.5f);
        }
        
        // 스폰 영역 시각화 (파란색 플랜)
        if (spawnMinPoint != null && spawnMaxPoint != null)
        {
            Gizmos.color = Color.blue;
            Vector3 center = (spawnMinPoint.position + spawnMaxPoint.position) * 0.5f;
            Vector3 size = spawnMaxPoint.position - spawnMinPoint.position;
            Gizmos.DrawWireCube(center, size);
            
            // 각 꼭지점 표시
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(spawnMinPoint.position, 0.5f);
            Gizmos.DrawSphere(spawnMaxPoint.position, 0.5f);
        }
        
        // 카메라 위치 시각화
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerCamera.position, 1f);
        }
    }

    #endregion

    #region 초기화 및 컴포넌트 설정
    
    private void InitializeComponents()
    {   
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
    }

    #endregion

    #region 경계 계산 및 설정
    
    private void CalculateBoundaries()
    {
        // 경계 계산 - 항상 실행하고 에러는 로그로만 표시
        if (boundaryMinPoint != null && boundaryMaxPoint != null)
        {
            // 경계 설정 - 이동 영역
            boundaryMin = boundaryMinPoint.position;
            boundaryMax = boundaryMaxPoint.position;
        }
        
        // 기존 유령 에이전트들에게 경계 설정 적용
        foreach (var ghost in ghosts)
        {
            if (ghost != null)
            {
                ghost.SetBoundaries(boundaryMin, boundaryMax);
            }
        }
    }

    #endregion

    #region 스폰 및 재스폰 관리
    
    private void SpawnGhost()
    {
        if (ghostPrefab == null) return;
        
        // 유령 인스턴스 생성
        GameObject ghostObj = Instantiate(ghostPrefab);
        GhostAgent ghostAgent = ghostObj.GetComponent<GhostAgent>();
        
        if (ghostAgent != null)
        {
            // 경계 설정 전달 - 반드시 위치 설정 전에 경계부터 설정
            if (boundaryMin != Vector3.zero || boundaryMax != Vector3.zero)
            {
                ghostAgent.SetBoundaries(boundaryMin, boundaryMax);
            }
            
            // 초기 위치 설정
            Vector3 spawnPosition = GetRandomSpawnPosition();
            ghostObj.transform.position = spawnPosition;
            
            // ML-Agents 에피소드 시작 전 위치가 초기화되도록 강제 에피소드 종료
            ghostAgent.EndEpisode();
            
            ghosts.Add(ghostAgent);
        }
    }
    
    private IEnumerator RespawnGhostAfterDelay(GhostAgent ghost, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (ghost != null && !ghost.gameObject.activeSelf)
        {
            // 위치 재설정 후 활성화
            Vector3 newPosition = GetRandomSpawnPosition();
            ghost.transform.position = newPosition;
            Debug.Log($"유령 재스폰 위치 설정: {newPosition}");
            
            ghost.gameObject.SetActive(true);
            
            // 위치가 초기화되도록 강제 에피소드 종료
            ghost.EndEpisode();
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnMin = spawnMinPoint.position;
        Vector3 spawnMax = spawnMaxPoint.position;
        
        float x = Random.Range(spawnMin.x, spawnMax.x);
        float z = Random.Range(spawnMin.z, spawnMax.z);
        float y = 34f;  // y축은 항상 34로 고정
        
        Vector3 spawnPos = new Vector3(x, y, z);
        return spawnPos;
    }

    #endregion

    #region 카메라 제어
    
    private IEnumerator RandomCameraSwitch()
    {
        SimpleCameraController cameraController = FindFirstObjectByType<SimpleCameraController>();
        if (cameraController == null) yield break;
        
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(30f, 60f));
            
            // 랜덤 카메라 위치 선택
            int randomPos = Random.Range(0, 3);
            switch (randomPos)
            {
                case 0:
                    cameraController.SwitchToLeftCamera();
                    break;
                case 1:
                    cameraController.SwitchToCenterCamera();
                    break;
                case 2:
                    cameraController.SwitchToRightCamera();
                    break;
            }
        }
    }

    #endregion
}
