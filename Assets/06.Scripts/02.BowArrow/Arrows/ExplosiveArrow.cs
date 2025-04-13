using UnityEngine;

/// <summary>
/// 폭발 화살 클래스: 충돌 시 범위 데미지를 주는 특수 화살
/// </summary>
public class ExplosiveArrow : Projectile
{
    [Header("폭발 화살 설정")]
    [SerializeField] private GameObject explosionEffect;  // 폭발 효과 파티클 프리팹
    [SerializeField] private LayerMask targetLayer;       // 데미지를 줄 대상 레이어 (적만 타격하도록 설정)
    
    private float explosionRadius = 3f;                   // 폭발 범위 (미터 단위)
    
    /// <summary>
    /// 화살이 충돌했을 때 호출되는 메서드 (부모 클래스의 추상 메서드 구현)
    /// 충돌 지점을 중심으로 폭발 효과 생성 및 범위 내 적 타격
    /// </summary>
    /// <param name="collision">충돌 정보</param>
    public override void OnImpact(Collision collision)
    {
        // 이미 비활성화된 상태면 무시 (중복 처리 방지)
        if (!gameObject.activeSelf) return;
        
        // 폭발 효과 재생 (충돌 지점에 파티클 생성)
        if (explosionEffect != null)
        {
            ParticleEffectPool.Instance.PlayEffect(explosionEffect, transform.position, Quaternion.identity);
        }
        
        // 폭발 범위 내 적 탐지 및 데미지 적용 (Physics.OverlapSphere로 구체 범위 내 콜라이더 검출)
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, targetLayer);
        foreach (var hitCollider in hitColliders)
        {
            // 충돌한 객체가 데미지를 받을 수 있는지 확인 (IDamageable 인터페이스 구현 여부)
            if (hitCollider.TryGetComponent<IDamageable>(out var damageable))
            {
                // 거리에 따른 데미지 감소 계산 (폭발 중심에서 멀수록 데미지 감소)
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                float damageRatio = 1f - (distance / explosionRadius);  // 거리 비율에 따른 데미지 비율 (0~1)
                float finalDamage = currentDamage * Mathf.Max(0.5f, damageRatio);  // 최소 50% 데미지 보장
                
                // 계산된 데미지 적용
                damageable.TakeDamage(finalDamage);
            }
        }
        
        // 화살 풀에 반환 (재사용을 위함)
        ReturnToPool();
    }
    
    /// <summary>
    /// 화살 데이터에서 설정 정보 가져와 적용 (부모 클래스 메서드 확장)
    /// </summary>
    /// <param name="data">프로젝타일 데이터 스크립터블 오브젝트</param>
    public override void SetProjectileData(ProjectileData data)
    {
        // 부모 클래스의 기본 설정 적용 (데미지 등)
        base.SetProjectileData(data);
        
        // 폭발 반경 설정 (데이터에 값이 있는 경우에만)
        if (data.impactRadius > 0)
        {
            explosionRadius = data.impactRadius;
        }
    }
}
