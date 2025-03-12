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
    [SerializeField] protected TrailRenderer projectileTrail;
    
    [Header("화살 구조")]
    [SerializeField] protected Transform arrowTip;     // 화살촉 위치
    [SerializeField] protected Transform arrowTail;    // 화살깃 위치
    
    [Header("디버그")]
    [SerializeField] protected bool debugRotation = false; // 회전 디버그 활성화 여부

    protected IObjectPool<Projectile> pool;      // 자신을 관리하는 풀
    protected float spawnTime;                   // 생성 시간
    protected float currentDamage;               // 현재 데미지
    protected float projectileSpeed;            // 투사체 속도


    #endregion

    #region 유니티 함수

    protected virtual void Update()
    {
        // 수명 체크
        float lifeTime = projectileData.lifeTime;

        if (Time.time - spawnTime > lifeTime)
        {
            ReturnToPool();
        }
    }

    #endregion

    #region 오브젝트 풀 관련 로직

    // 풀 참조 설정
    public void SetPoolReference(IObjectPool<Projectile> poolRef)
    {
        pool = poolRef;
    }

    // 풀 사용 및 반환 시 투사체 상태 초기화
    public virtual void ResetState()
    {
        // 자식 오브젝트 위치 초기화
        if (projectileChildPoint != null)
        {
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.rotation = Quaternion.identity;
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
        currentDamage = projectileData.damage;

        // 트레일 초기화
        projectileTrail.Clear();
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

    #endregion

    #region 충돌 처리

    // 충돌 시 처리 (추상 메서드)
    public abstract void OnImpact(Collision collision);

    // 충돌 이벤트 처리
    protected virtual void OnCollisionEnter(Collision collision)
    {
        OnImpact(collision);
    }

    #endregion

    #region 투사체 발사 및 방향
    // 프로젝타일 발사
    public virtual void Launch(Vector3 velocity)
    {
        // 충돌 감지 활성화
        projectileMeshCollider.enabled = true;

        // 속도 크기 저장 (회전 계산용)
        projectileSpeed = velocity.magnitude;
        
        // 화살의 현재 방향 벡터
        Vector3 arrowDirection = (arrowTip.position - arrowTail.position).normalized;
        // 화살이 가야할 방향과 현재 방향 사이의 회전
        Quaternion rotationToTarget = Quaternion.FromToRotation(arrowDirection, velocity.normalized);
        // 현재 회전에 목표 회전을 적용
        transform.rotation = rotationToTarget * transform.rotation;

        // 중력 적용
        projectileRigidbody.useGravity = true;
        projectileRigidbody.AddForce(velocity, ForceMode.Impulse);
    }
    
    #endregion
}