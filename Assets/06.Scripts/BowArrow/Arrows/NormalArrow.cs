using UnityEngine;

public class NormalArrow : Projectile
{
    [Header("일반 화살 설정")]
    [SerializeField] private ParticleSystem hitEffect;
    
    // 충돌 효과 구현
    public override void OnImpact(Collision collision)
    {
        // 충돌한 오브젝트에 IDamageable 확인
        if(collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            // 데미지 적용
            damageable.TakeDamage(currentDamage);
            
            // 충돌 효과
            if (hitEffect != null)
            {
                hitEffect.transform.position = collision.contacts[0].point;
                hitEffect.Play();
            }
        }
        ReturnToPool();
    }
}