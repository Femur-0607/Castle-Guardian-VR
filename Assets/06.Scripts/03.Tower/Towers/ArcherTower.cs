using UnityEngine;
using System.Collections;

public class ArcherTower : Tower
{
    [Header("시각 효과")]
    [SerializeField] private GameObject projectilePrefab;     // 화살 프리팹
    [SerializeField] private GameObject muzzleEffectPrefab;   // 발사 효과
    [SerializeField] private GameObject hitEffectPrefab;      // 타격 효과

    protected override void Attack(Enemy target)
    {
        if (target == null) return;

        // 화살 발사 사운드를 3D로 재생 (타워의 위치에서)
        SoundManager.Instance.PlaySound3D("ArrowTowerShoot", firePoint.position);

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
            hitEffectPrefab
        ));
    }

}