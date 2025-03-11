using UnityEngine;
using System.Collections;

public class ExplosiveTower : Tower
{
    [Header("폭발 타워 설정")]
    [SerializeField] private Animator towerAnimator;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionRadius = 3f;

    protected override void Attack(Enemy target)
    {
        // 애니메이션 재생
        if (towerAnimator != null)
        {
            towerAnimator.SetTrigger("Fire");
        }
        
        // 사운드 효과
        if (fireSound != null)
        {
            AudioSource.PlayClipAtPoint(fireSound, transform.position);
        }
        
        // 발사체 이동 시간 계산
        float distance = Vector3.Distance(firePoint.position, target.transform.position);
        float travelTime = distance / towerData.projectileSpeed;
        
        // 지연 폭발 데미지
        StartCoroutine(DelayedExplosion(target, travelTime));
    }

    private IEnumerator DelayedExplosion(Enemy target, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 타겟이 여전히 존재하는지 확인
        if (target == null || !target.gameObject.activeInHierarchy) yield break;
        
        Vector3 explosionPos = target.transform.position;
        
        // 폭발 이펙트
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, 
                explosionPos, 
                Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // 범위 내 모든 적에게 데미지
        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius, LayerMask.GetMask("Enemy"));
        
        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent<Enemy>(out var enemy) && enemy.IsAlive())
            {
                // 거리에 따른 데미지 감소
                float distance = Vector3.Distance(explosionPos, enemy.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                float finalDamage = towerData.attackDamage * Mathf.Max(0.2f, damageMultiplier);
                
                enemy.TakeDamage(finalDamage);
            }
        }
    }
}