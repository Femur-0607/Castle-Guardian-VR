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

         // 발사 효과 생성 (머즐 이펙트)
        ParticleEffectPool.Instance.PlayEffect(muzzleEffectPrefab, firePoint.position, firePoint.rotation);

        // 투사체 효과 생성 (화살 이펙트)
        GameObject projectileObj = ParticleEffectPool.Instance.GetEffectInstance(projectilePrefab, firePoint.position, firePoint.rotation);
        float travelTime = Vector3.Distance(firePoint.position, target.transform.position) / towerData.projectileSpeed;

        StartCoroutine(MoveProjectileEffect(
            projectileObj, 
            target.transform, 
            towerData.projectileSpeed, 
            travelTime, 
            hitEffectPrefab
        ));
    }

}