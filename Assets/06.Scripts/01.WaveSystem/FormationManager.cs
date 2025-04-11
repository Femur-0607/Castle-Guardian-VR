using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FormationManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private FormationArea[] formationAreas; // 배열로 변경
    
    // 각 FormationArea별로 포지션 관리를 위한 딕셔너리들
    private Dictionary<FormationArea, Dictionary<Vector3, Enemy>> areaPositionToEnemy = new Dictionary<FormationArea, Dictionary<Vector3, Enemy>>();
    private Dictionary<Enemy, (FormationArea area, Vector3 position)> enemyToAreaPosition = new Dictionary<Enemy, (FormationArea, Vector3)>();
    private Dictionary<FormationArea, List<Vector3>> areaAvailablePositions = new Dictionary<FormationArea, List<Vector3>>();

    private void Start()
    {
        InitializeAllAreas();
    }

    private void InitializeAllAreas()
    {
        foreach (var area in formationAreas)
        {
            InitializeArea(area);
        }
    }

    private void InitializeArea(FormationArea area)
    {
        // 각 영역별 초기화
        if (!areaPositionToEnemy.ContainsKey(area))
        {
            areaPositionToEnemy[area] = new Dictionary<Vector3, Enemy>();
            areaAvailablePositions[area] = new List<Vector3>();
        }

        // 포지션 초기화
        var positions = areaAvailablePositions[area];
        positions.Clear();

        int totalPositions = area.GetTotalPositions();
        for (int i = 0; i < totalPositions; i++)
        {
            positions.Add(area.GetPositionByIndex(i));
        }

        // 성문과 가까운 순서대로 정렬 - 로컬 좌표 기준으로 변경
        Transform areaTransform = area.transform;
        positions.Sort((a, b) => {
            // 월드 좌표를 로컬 좌표로 변환
            Vector3 localA = areaTransform.InverseTransformPoint(a);
            Vector3 localB = areaTransform.InverseTransformPoint(b);
            
            // 로컬 z축 기준 정렬 (로컬 z값이 큰 것이 성문 방향)
            return localB.z.CompareTo(localA.z);
        });
    }

    /// <summary>
    /// 적 유닛에게 포메이션 포지션을 할당
    /// </summary>
    public Vector3? GetFormationPosition(Enemy enemy, Transform target)
    {
        // 이미 할당된 포지션이 있다면 반환
        if (enemyToAreaPosition.ContainsKey(enemy))
        {
            return enemyToAreaPosition[enemy].position;
        }

        // 타겟에 가장 가까운 FormationArea 찾기
        var area = GetFormationAreaForTarget(target);
        if (area == null) return null;

        // 해당 영역에 사용 가능한 포지션이 없다면 null 반환
        if (!areaAvailablePositions.ContainsKey(area) || 
            areaAvailablePositions[area].Count == 0)
        {
            return null;
        }

        // 가장 앞쪽의 사용 가능한 포지션 할당
        var positions = areaAvailablePositions[area];
        Vector3 position = positions[0];
        positions.RemoveAt(0);
        
        // 할당 정보 저장
        areaPositionToEnemy[area][position] = enemy;
        enemyToAreaPosition[enemy] = (area, position);

        return position;
    }

    /// <summary>
    /// 적 유닛의 포메이션 포지션 해제
    /// </summary>
    public void ReleasePosition(Enemy enemy)
    {
        if (!enemyToAreaPosition.ContainsKey(enemy)) return;

        var (area, position) = enemyToAreaPosition[enemy];
        
        // 딕셔너리에서 제거
        enemyToAreaPosition.Remove(enemy);
        areaPositionToEnemy[area].Remove(position);
        
        // 사용 가능한 포지션 목록에 추가
        areaAvailablePositions[area].Add(position);
        
        // 포지션을 앞쪽부터 채우기 위해 정렬
        areaAvailablePositions[area].Sort((a, b) => a.z.CompareTo(b.z));
    }

    /// <summary>
    /// 특정 타겟(성문)에 대한 FormationArea 반환
    /// </summary>
    private FormationArea GetFormationAreaForTarget(Transform target)
    {
        // 가장 가까운 FormationArea 찾기
        return formationAreas.OrderBy(area => 
            Vector3.Distance(area.transform.position, target.position))
            .FirstOrDefault();
    }
}
