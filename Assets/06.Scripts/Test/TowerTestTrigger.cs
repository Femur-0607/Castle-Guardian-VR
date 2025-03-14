using UnityEngine;

[RequireComponent(typeof(Tower))]
public class TowerTestTrigger : MonoBehaviour
{
    private Tower tower;
    public GameObject targetCube; // 인스펙터에서 큐브 참조 설정
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tower = GetComponent<Tower>();
    }

    // Update is called once per frame
    void Update()
    {
        // 스페이스바를 누르면 공격 테스트
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (targetCube != null && targetCube.TryGetComponent<DummyEnemy>(out var enemy))
            {
                // 타워 공격 메서드 직접 호출 (private이라면 reflection 사용)
                // 예시로 ArcherTower의 경우
                if (tower is ArcherTower archerTower)
                {
                    // Attack 메서드가 protected이므로 reflection으로 호출
                    System.Type type = archerTower.GetType();
                    System.Reflection.MethodInfo method = type.GetMethod("Attack", 
                        System.Reflection.BindingFlags.NonPublic | 
                        System.Reflection.BindingFlags.Instance);
                    
                    method?.Invoke(archerTower, new object[] { enemy });
                }
            }
        }
    }
}
