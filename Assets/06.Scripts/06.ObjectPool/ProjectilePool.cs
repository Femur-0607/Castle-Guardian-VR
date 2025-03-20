using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using ProjectileCurveVisualizerSystem;

/// <summary>
/// 투사체(화살) 전용 풀 매니저
/// 씬에 붙여둔 뒤, GetProjectile(...) 으로 화살을 가져올 수 있음.
/// 화살 타입별로 서로 다른 풀을 관리합니다.
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    [System.Serializable]
    public class ProjectilePoolSetting
    {
        public ProjectileData.ProjectileType arrowType;
        public Projectile projectilePrefab;
    }

    [Header("Projectile Prefabs & Pool Settings")]
    [SerializeField] private List<ProjectilePoolSetting> projectilePrefabs = new List<ProjectilePoolSetting>();
    [SerializeField] private int defaultCapacity = 10;   // 풀 초기 생성 갯수
    [SerializeField] private int maxSize = 50;          // 풀 최대 갯수

    // 현재 선택된 화살 타입 (기본값은 일반 화살)
    private ProjectileData.ProjectileType currentArrowType = ProjectileData.ProjectileType.Normal;

    // 화살 타입별 풀을 관리하는 딕셔너리
    private Dictionary<ProjectileData.ProjectileType, IObjectPool<Projectile>> projectilePools = new Dictionary<ProjectileData.ProjectileType, IObjectPool<Projectile>>();
    // 화살 타입별 프리팹을 빠르게 찾기 위한 딕셔너리
    private Dictionary<ProjectileData.ProjectileType, Projectile> prefabDictionary = new Dictionary<ProjectileData.ProjectileType, Projectile>();

    private void Awake()
    {
        // 각 화살 타입별 프리팹 딕셔너리 초기화
        foreach (var setting in projectilePrefabs)
        {
            prefabDictionary[setting.arrowType] = setting.projectilePrefab;
        }

        // 각 화살 타입별 풀 초기화
        foreach (var setting in projectilePrefabs)
        {
            IObjectPool<Projectile> pool = new ObjectPool<Projectile>(
                createFunc: () => CreatePooledItem(setting.arrowType),
                actionOnGet: OnTakeFromPool,
                actionOnRelease: OnReturnedToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
            
            projectilePools[setting.arrowType] = pool;
        }
    }

    // -------------------- ObjectPool 콜백 구현부 --------------------
    private Projectile CreatePooledItem(ProjectileData.ProjectileType arrowType)
    {
        // 해당 타입의 프리팹이 없으면 기본 타입으로 폴백
        if (!prefabDictionary.ContainsKey(arrowType))
        {
            Debug.LogWarning($"화살 타입 {arrowType}에 대한 프리팹이 없습니다. 기본 화살로 대체합니다.");
            arrowType = ProjectileData.ProjectileType.Normal;
        }

        // 실제 Prefab을 Instantiate
        Projectile newProjectile = Instantiate(prefabDictionary[arrowType]);
        // Projectile 측에 풀 참조를 넘겨주어, 직접 Release 호출 가능하게 함
        newProjectile.SetPoolReference(projectilePools[arrowType]);
        // 화살 타입 태그 지정 (추적을 위해)
        newProjectile.gameObject.name = $"{arrowType}Arrow";
        
        return newProjectile;
    }

    private void OnTakeFromPool(Projectile projectile)
    {
        projectile.ResetState();  // 물리값, TrailRenderer 등 초기화
        projectile.gameObject.SetActive(true);
    }

    private void OnReturnedToPool(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
        projectile.ResetState();  // 물리값, TrailRenderer 등 초기화
    }

    private void OnDestroyPoolObject(Projectile projectile)
    {
        // maxSize 초과로 제거될 때나, Scene 종료 시점에 호출
        Destroy(projectile.gameObject);
    }

    // -------------------- 사용 편의 함수 --------------------
    /// <summary>
    /// 현재 화살 타입을 설정합니다.
    /// </summary>
    /// <param name="arrowType">화살 타입</param>
    public void SetCurrentArrowType(ProjectileData.ProjectileType arrowType)
    {
        // 등록된 타입인지 확인
        if (!projectilePools.ContainsKey(arrowType))
        {
            Debug.LogWarning($"화살 타입 {arrowType}에 대한 풀이 없습니다. 기본 화살로 대체합니다.");
            currentArrowType = ProjectileData.ProjectileType.Normal;
        }
        else
        {
            currentArrowType = arrowType;
        }
    }

    /// <summary>
    /// 현재 설정된 화살 타입에 맞는 화살을 풀에서 꺼내옵니다.
    /// </summary>
    public Projectile GetProjectile()
    {
        // 현재 타입의 풀이 없으면 기본 화살 풀 사용
        if (!projectilePools.ContainsKey(currentArrowType))
        {
            Debug.LogWarning($"화살 타입 {currentArrowType}에 대한 풀이 없습니다. 기본 화살로 대체합니다.");
            currentArrowType = ProjectileData.ProjectileType.Normal;
        }

        return projectilePools[currentArrowType].Get();
    }
    
    /// <summary>
    /// 지정된 화살 타입의 화살을 풀에서 꺼내옵니다.
    /// </summary>
    /// <param name="arrowType">화살 타입</param>
    public Projectile GetProjectile(ProjectileData.ProjectileType arrowType)
    {
        // 요청한 타입의 풀이 없으면 기본 화살 풀 사용
        if (!projectilePools.ContainsKey(arrowType))
        {
            Debug.LogWarning($"화살 타입 {arrowType}에 대한 풀이 없습니다. 기본 화살로 대체합니다.");
            arrowType = ProjectileData.ProjectileType.Normal;
        }

        return projectilePools[arrowType].Get();
    }
    
    /// <summary>
    /// 현재 설정된 화살 타입에 맞는 화살을 지정된 위치에 생성합니다.
    /// </summary>
    /// <param name="position">위치</param>
    public Projectile GetProjectileAt(Vector3 position)
    {
        Projectile proj = GetProjectile();
        proj.transform.position = position;
        return proj;
    }
    
    /// <summary>
    /// 지정된 화살 타입의 화살을 지정된 위치에 생성합니다.
    /// </summary>
    /// <param name="arrowType">화살 타입</param>
    /// <param name="position">위치</param>
    public Projectile GetProjectileAt(ProjectileData.ProjectileType arrowType, Vector3 position)
    {
        Projectile proj = GetProjectile(arrowType);
        proj.transform.position = position;
        return proj;
    }
}
