using UnityEngine;
using Unity.MLAgents;

public class GhostTrainingManager : MonoBehaviour
{
    [Header("기본 설정")]
    public GameObject ghostPrefab;
    
    [Header("경계 설정")]
    public Transform boundaryMinPoint;
    public Transform boundaryMaxPoint;
    public Transform spawnMinPoint;
    public Transform spawnMaxPoint;
    
    private Vector3 boundaryMin;
    private Vector3 boundaryMax;
    private GhostAgent ghostAgent;
    [SerializeField] private SimpleCameraController cameraController;

    void Start()
    {
        // 경계 계산
        CalculateBoundaries();
        
        // 초기 유령 생성
        SpawnGhost();
    }

    // 경계 계산
    private void CalculateBoundaries()
    {
        if (boundaryMinPoint != null && boundaryMaxPoint != null)
        {
            boundaryMin = boundaryMinPoint.position;
            boundaryMax = boundaryMaxPoint.position;
        }
    }

        // 유령 생성
    private void SpawnGhost()
    {
        if (ghostPrefab == null) return;
        
        // 유령 인스턴스 생성
        GameObject ghostObj = Instantiate(ghostPrefab);
        ghostAgent = ghostObj.GetComponent<GhostAgent>();
        
        if (ghostAgent != null)
        {
            // 경계 설정
            ghostAgent.SetBoundaries(boundaryMin, boundaryMax);
            
            // 초기 위치 설정
            Vector3 spawnPosition = GetRandomSpawnPosition();
            ghostObj.transform.position = spawnPosition;
            
            // ML-Agents 에피소드 시작
            ghostAgent.EndEpisode();
        }
    }
    
    // 유령 에이전트 랜덤 위치 생성
    public Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnMin = spawnMinPoint.position;
        Vector3 spawnMax = spawnMaxPoint.position;
        
        float x = Random.Range(spawnMin.x, spawnMax.x);
        float z = Random.Range(spawnMin.z, spawnMax.z);
        float y = 35f;  // y축은 항상 35로 고정
        
        return new Vector3(x, y, z);
    }

    // 카메라 트리거와 충돌 시 호출될 메서드
    public void OnGhostTriggeredCamera()
    {
        if (cameraController != null)
        {
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

        // 유령 재스폰
        if (ghostAgent != null)
        {
            Vector3 newPosition = GetRandomSpawnPosition();
            ghostAgent.transform.position = newPosition;
            ghostAgent.EndEpisode();
        }
    }

    // 씬 뷰에서 경계와 스폰 영역 시각화
    private void OnDrawGizmos()
    {
        // 경계 영역 시각화 (빨간색)
        if (boundaryMinPoint != null && boundaryMaxPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 center = (boundaryMinPoint.position + boundaryMaxPoint.position) * 0.5f;
            Vector3 size = boundaryMaxPoint.position - boundaryMinPoint.position;
            Gizmos.DrawWireCube(center, size);
            
            // 각 꼭지점 표시
            Gizmos.DrawSphere(boundaryMinPoint.position, 0.5f);
            Gizmos.DrawSphere(boundaryMaxPoint.position, 0.5f);
        }
        
        // 스폰 영역 시각화 (파란색)
        if (spawnMinPoint != null && spawnMaxPoint != null)
        {
            Gizmos.color = Color.blue;
            Vector3 center = (spawnMinPoint.position + spawnMaxPoint.position) * 0.5f;
            Vector3 size = spawnMaxPoint.position - spawnMinPoint.position;
            Gizmos.DrawWireCube(center, size);
            
            // 각 꼭지점 표시
            Gizmos.DrawSphere(spawnMinPoint.position, 0.5f);
            Gizmos.DrawSphere(spawnMaxPoint.position, 0.5f);
        }
    }
}
