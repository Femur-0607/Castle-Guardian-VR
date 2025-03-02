using UnityEngine;

public class SamplePositionExample : MonoBehaviour
{
    public Vector3 sourcePosition;  // 찾고 싶은 월드 좌표
    public float maxDistance = 5f;  // 찾고 싶은 범위(반경)
    
    void Start()
    {
        UnityEngine.AI.NavMeshHit hit;
        // sourcePosition 근처에서 maxDistance 반경 내의 NavMesh 상의 점을 찾는다.
        if (UnityEngine.AI.NavMesh.SamplePosition(sourcePosition, out hit, maxDistance, UnityEngine.AI.NavMesh.AllAreas))
        {
            // 성공 시 hit.position에 유효한 NavMesh 포인트가 들어있음
            Debug.Log("Found NavMesh position: " + hit.position);
            
            // 원하는 위치로 오브젝트 이동
            transform.position = hit.position;
        }
        else
        {
            // 근처에 NavMesh가 없다면 실패
            Debug.LogWarning("No valid NavMesh position found within maxDistance.");
        }
    }
}
