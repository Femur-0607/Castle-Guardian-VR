// Assets/06.Scripts/BowArrow/NormalArrow.cs
using UnityEngine;

public class NormalArrow : Projectile
{
    [Header("일반 화살 설정")]
    [SerializeField] private AudioClip hitSound;
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
            
            // 충돌 사운드
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, collision.contacts[0].point);
            }
        }
        
        // 충돌 후 지연 반환
        StartCoroutine(ReturnAfterDelay(1f));
    }
}