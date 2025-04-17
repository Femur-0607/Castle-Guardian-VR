using UnityEngine;
using System.Collections;

public class ExplosiveTower : Tower
{
    [Header("시각 효과")]
    [SerializeField] private GameObject explosionPrefab;     // 화살 프리팹
    [SerializeField] private GameObject muzzleEffectPrefab;   // 발사 효과
    [SerializeField] private GameObject hitEffectPrefab;      // 타격 효과

    [Header("폭발 타워 설정")]
    [SerializeField] private float explosionRadius = 10f;

    protected override void Attack(Enemy target)
    {
        if (target == null) return;

        // 발사 사운드 재생
        SoundManager.Instance.PlaySound3D("ExplosiveTowerShoot", firePoint.position);

        // 발사 효과 생성 (ParticleEffectPool 사용)
        ParticleEffectPool.Instance.PlayEffect(muzzleEffectPrefab, firePoint.position, firePoint.rotation);

        // 투사체 생성 (ProjectilePool 사용)
        Projectile projectileComponent = ProjectilePool.Instance.GetProjectileAt(firePoint.position);
        GameObject projectileObj = projectileComponent.gameObject;
        float travelTime = Vector3.Distance(firePoint.position, target.transform.position) / towerData.projectileSpeed;

        // 공통 코루틴 호출
        StartCoroutine(MoveProjectileWithPrediction(
            projectileObj,
            target.transform,
            towerData.projectileSpeed,
            travelTime,
            hitEffectPrefab));

        // 폭발 효과는 별도 코루틴으로 처리
        StartCoroutine(DelayedExplosion(target, travelTime));
    }

    private IEnumerator DelayedExplosion(Enemy target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (target == null || !target.gameObject.activeInHierarchy) yield break;

        Vector3 explosionPos = target.transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, explosionRadius, LayerMask.GetMask("Enemy"));

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent<Enemy>(out var enemy) && enemy.IsAlive())
            {
                float distance = Vector3.Distance(explosionPos, enemy.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                float finalDamage = towerData.attackDamage * Mathf.Max(0.2f, damageMultiplier);
                enemy.TakeDamage(finalDamage);
            }
        }
    }
}