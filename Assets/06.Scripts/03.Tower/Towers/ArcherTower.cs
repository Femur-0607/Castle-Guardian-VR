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

        // 발사 효과 생성
        GameObject muzzleEffect = Instantiate(muzzleEffectPrefab, firePoint.position, firePoint.rotation);
        Destroy(muzzleEffect, 2f);

        // 투사체 생성 및 이동 시작
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
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