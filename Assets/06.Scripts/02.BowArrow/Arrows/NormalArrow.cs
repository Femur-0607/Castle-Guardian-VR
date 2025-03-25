using UnityEngine;

/// <summary>
/// 기본 화살 클래스: 가장 기본적인 화살로, 직접 타격한 적에게만 단일 데미지를 입힘
/// </summary>
public class NormalArrow : Projectile
{
    [Header("일반 화살 설정")]
    [SerializeField] private GameObject hitEffect;  // 화살이 충돌했을 때 생성되는 효과 프리팹
    
    /// <summary>
    /// 화살이 충돌했을 때 호출되는 메서드 (부모 클래스의 추상 메서드 구현)
    /// 단일 타겟에게 데미지를 입히고 충돌 효과를 표시함
    /// </summary>
    /// <param name="collision">충돌 정보</param>
    public override void OnImpact(Collision collision)
    {
        // 이미 비활성화된 상태면 무시 (중복 처리 방지)
        if (!gameObject.activeSelf) return;
        
        // 충돌한 객체가 데미지를 받을 수 있는지 확인 (IDamageable 인터페이스 구현 여부)
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            // 데미지 적용 (Projectile 클래스의 currentDamage 사용)
            damageable.TakeDamage(currentDamage);

            // 충돌 효과 재생 (충돌 지점에 파티클 생성)
            if (hitEffect != null)
            {
                ParticlePool.Instance.PlayEffect(hitEffect.name, collision.contacts[0].point, Quaternion.identity);
            }
        }
        
        // 화살 풀에 반환 (재사용을 위함)
        ReturnToPool();
    }
}