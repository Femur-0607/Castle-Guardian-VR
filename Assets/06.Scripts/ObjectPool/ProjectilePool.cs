using UnityEngine;
using UnityEngine.Pool;
using ProjectileCurveVisualizerSystem;

/// <summary>
/// 투사체(화살) 전용 풀 매니저
/// 씬에 붙여둔 뒤, GetProjectile(...) 으로 화살을 가져올 수 있음.
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    [Header("Projectile Prefab & Pool Settings")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private int defaultCapacity = 10;   // 풀 초기 생성 갯수
    [SerializeField] private int maxSize = 50;          // 풀 최대 갯수

    // Unity의 공식 오브젝트 풀 인터페이스
    private IObjectPool<Projectile> projectilePool;

    private void Awake()
    {
        // 풀 생성
        projectilePool = new ObjectPool<Projectile>(
            createFunc: CreatePooledItem,           //  풀에서 객체가 필요할 때 새 객체를 생성하는 함수
            actionOnGet: OnTakeFromPool,            //  풀에서 객체를 가져올 때 호출되는 함수
            actionOnRelease: OnReturnedToPool,      //  객체가 풀에 바환될 때 호출되는 함수
            actionOnDestroy: OnDestroyPoolObject,   //  객체가 풀에서 완전히 제거될 때 호출되는 함수
            collectionCheck: false,
            defaultCapacity: defaultCapacity,       //  풀의 초기 생성 개수
            maxSize: maxSize                        //  풀의 최대 허용 개수
        );
    }

    // -------------------- ObjectPool 콜백 구현부 --------------------
    private Projectile CreatePooledItem()
    {
        // 실제 Prefab을 Instantiate
        Projectile newProjectile = Instantiate(projectilePrefab);
        // Projectile 측에 풀 참조를 넘겨주어, 직접 Release 호출 가능하게 함
        newProjectile.SetPoolReference(projectilePool);
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
    /// 화살을 풀에서 꺼내와서 원하는 위치/회전, 발사 속도를 적용
    /// </summary>
    public Projectile GetProjectile()
    {
        return projectilePool.Get();
    }
    
    // 위치 정보만 받는 편의 메서드
    public Projectile GetProjectileAt(Vector3 position)
    {
        Projectile proj = projectilePool.Get();
        proj.transform.position = position;
        return proj;
    }
}
