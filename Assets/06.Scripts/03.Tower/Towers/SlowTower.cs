using UnityEngine;
using System.Collections;

public class SlowTower : Tower
{
    [Header("시각 효과")]
    [SerializeField] private GameObject slowPrefab;     // 화살 프리팹
    [SerializeField] private GameObject muzzleEffectPrefab;   // 발사 효과
    [SerializeField] private GameObject hitEffectPrefab;      // 타격 효과

    [Header("둔화 타워 설정")]
    [SerializeField] private float slowDuration = 5f;
    [SerializeField] private float slowAmount = 0.5f; // 50% 감속

    protected override void Attack(Enemy target)
    {
        if (target == null) return;
        
        // 발사 사운드 재생
        SoundManager.Instance.PlaySound3D("SlowTowerShoot", firePoint.position);
        
        // 발사 효과 생성
        GameObject muzzleEffect = Instantiate(muzzleEffectPrefab, firePoint.position, firePoint.rotation);
        Destroy(muzzleEffect, 2f);
        
        // 투사체 생성 및 이동 시작
        GameObject projectile = Instantiate(slowPrefab, firePoint.position, Quaternion.identity);
        float travelTime = Vector3.Distance(firePoint.position, target.transform.position) / towerData.projectileSpeed;
        
        // 공통 코루틴 호출
        StartCoroutine(MoveProjectileWithPrediction(
            projectile, 
            target.transform, 
            towerData.projectileSpeed, 
            travelTime, 
            hitEffectPrefab));
        
        // 둔화 효과는 별도 코루틴으로 처리
        StartCoroutine(DelayedSlow(target, travelTime));
    }

    private IEnumerator DelayedSlow(Enemy target, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (target != null && target.gameObject.activeInHierarchy && target.IsAlive())
        {
            target.TakeDamage(towerData.attackDamage);
            target.ApplySlowEffect(slowDuration, slowAmount);
        }
    }
}