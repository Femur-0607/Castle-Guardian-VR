using UnityEngine;
using UnityEngine.Pool;

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
            createFunc: CreatePooledItem,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
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
        projectile.gameObject.SetActive(true);
        projectile.ResetState();  // 물리값, TrailRenderer 등 초기화
    }

    private void OnReturnedToPool(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
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
    public Projectile GetProjectile(Vector3 spawnPosition, Quaternion rotation, Vector3 launchVelocity)
    {
        // 풀에서 객체 하나를 Get
        Projectile proj = projectilePool.Get();

        // 위치/회전/속도 설정
        proj.transform.SetPositionAndRotation(spawnPosition, rotation);
        proj.Launch(launchVelocity);

        return proj;
    }

    /// <summary>
    /// 외부 스크립트에서 수동으로 반환하고 싶을 때 호출
    /// (일반적으로 Projectile 내부에서 충돌 후 풀로 반환하므로 잘 안 쓸 수도 있음)
    /// </summary>
    public void ReleaseProjectile(Projectile proj)
    {
        projectilePool.Release(proj);
    }
}
