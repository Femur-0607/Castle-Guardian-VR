using UnityEngine;

public class FormationArea : MonoBehaviour
{
    [Header("Formation Settings")]
    [SerializeField] private Vector2 areaSize = new Vector2(18f, 18f);
    [SerializeField] private int maxUnitsPerRow = 5; // 한 줄당 최대 유닛 수
    [SerializeField] private int maxRows = 5; // 최대 줄 수
    [SerializeField] private float unitSpacing = 3.5f; // 유닛 간 간격
    [SerializeField] private Color gizmoColor = new Color(0, 1, 1, 0.3f);

    [Header("Debug")]
    [SerializeField] private bool showUnitPositions = true;
    [SerializeField] private float unitRadius = 1f;

    public Vector2 AreaSize => areaSize;

    private void OnDrawGizmos()
    {
        // 영역 표시
        Gizmos.color = gizmoColor;
        Vector3 size = new Vector3(areaSize.x, 0.1f, areaSize.y);
        Gizmos.DrawCube(transform.position, size);
        
        // 경계선
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, size);
        
        if (showUnitPositions)
        {
            // 유닛 위치 표시
            Gizmos.color = Color.yellow;
            
            for (int row = 0; row < maxRows; row++)
            {
                for (int col = 0; col < maxUnitsPerRow; col++)
                {
                    Vector3 pos = GetGridPosition(row, col);
                    Gizmos.DrawWireSphere(pos, unitRadius);
                }
            }
        }
    }

    private Vector3 GetGridPosition(int row, int col)
    {
        // 한 줄에 5개 유닛, 중앙 기준 좌우 +-3.5 범위
        float startX = -(maxUnitsPerRow - 1) * unitSpacing / 2f;
        float xPos = startX + (col * unitSpacing);
        
        // Z축도 X축처럼 중앙 기준으로 계산
        float startZ = -(maxRows - 1) * unitSpacing / 2f;
        float zPos = startZ + (row * unitSpacing);
        
        return transform.position + new Vector3(xPos, 0f, zPos);
    }

    // 사용 가능한 총 포지션 수 반환
    public int GetTotalPositions() => maxUnitsPerRow * maxRows;

    // 특정 인덱스의 포지션 가져오기 (0부터 시작)
    public Vector3 GetPositionByIndex(int index)
    {
        int row = index / maxUnitsPerRow;
        int col = index % maxUnitsPerRow;
        return GetGridPosition(row, col);
    }
}
