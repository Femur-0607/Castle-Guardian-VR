using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

// 투사체 기초가 되는 추상 클래스
// IProjectile 인터페이스를 구현하여 투사체 특성을 정의
// 투사체 풀링, 물리 충돌, 피해 처리 등을 관리
// 구현해야할 화살들은 상속을 해서 구현
public abstract class Projectile : MonoBehaviour, IProjectile
{
    #region 필드 변수
    [Header("참조")]
    [SerializeField] protected ProjectileData projectileData;
    [SerializeField] protected Transform projectileChildPoint;
    [SerializeField] protected Rigidbody projectileRigidbody;
    [SerializeField] protected MeshCollider projectileMeshCollider;

    protected IObjectPool<Projectile> pool;      // 자신을 관리하는 풀
    protected float spawnTime;
    protected float currentDamage;

    #endregion

    // 피해량 설정
    public virtual void SetDamage(float damage)
    {
        currentDamage = damage;
    }

    // 풀 참조 설정
    public void SetPoolReference(IObjectPool<Projectile> poolRef)
    {
        pool = poolRef;
    }

    // 상태 초기화
    public virtual void ResetState()
    {
        // 자식 오브젝트 위치 초기화
        if (projectileChildPoint != null)
        {
            projectileChildPoint.localPosition = Vector3.zero;
            projectileChildPoint.localRotation = Quaternion.identity;
        }

        // 물리 및 충돌 상태 초기화
        if (projectileRigidbody != null)
        {
            projectileRigidbody.linearVelocity = Vector3.zero;
            projectileRigidbody.angularVelocity = Vector3.zero;
            projectileRigidbody.WakeUp();
        }

        if (projectileMeshCollider != null)
        {
            projectileMeshCollider.enabled = false;
            projectileMeshCollider.enabled = true;
        }

        // 생성 시간 기록
        spawnTime = Time.time;

        // 기본 데미지 설정
        currentDamage = projectileData != null ? projectileData.damage : 0f;

        // 트레일 초기화
        var trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.Clear();
    }

    protected virtual void Update()
    {
        // 수명 체크
        float lifeTime = projectileData != null ? projectileData.lifeTime : 5f;
        if (Time.time - spawnTime > lifeTime)
        {
            ReturnToPool();
        }
    }

    // 프로젝타일 발사
    public virtual void Launch(Vector3 velocity)
    {
        // 충돌 감지 활성화
        if (projectileMeshCollider != null)
            projectileMeshCollider.enabled = true;

        // 중력 적용
        if (projectileRigidbody != null)
        {
            projectileRigidbody.useGravity = true;
            projectileRigidbody.AddForce(velocity, ForceMode.Impulse);
        }
    }

    // 충돌 시 처리 (추상 메서드)
    public abstract void OnImpact(Collision collision);

    // 충돌 이벤트 처리
    protected virtual void OnCollisionEnter(Collision collision)
    {
        OnImpact(collision);
    }

    // 풀로 반환
    protected virtual void ReturnToPool()
    {
        if (pool != null)
        {
            pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 지연 후 풀 반환
    protected IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }
}