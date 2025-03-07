using UnityEngine;
using System.Collections;

public class ArcherTower : Tower
{
    [Header("타워 고유 설정")]
    [SerializeField] private Animator towerAnimator;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private GameObject hitEffectPrefab;

    protected override void Attack(Enemy target)
    {
        // 애니메이션 재생
        if (towerAnimator != null)
        {
            towerAnimator.SetTrigger("Attack");
        }
        
        // 사운드 효과
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, transform.position);
        }
        
        // 화살이 적에게 도달하는 시간 계산
        float distance = Vector3.Distance(firePoint.position, target.transform.position);
        float travelTime = distance / towerData.projectileSpeed;
        
        // 지연 데미지 처리
        StartCoroutine(DelayedDamage(target, travelTime));
    }

    private IEnumerator DelayedDamage(Enemy target, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 타겟 유효성 체크
        if (target != null && target.gameObject.activeInHierarchy && target.IsAlive())
        {
            // 데미지 적용
            target.TakeDamage(towerData.attackDamage);
            
            // 히트 이펙트
            if (hitEffectPrefab != null)
            {
                GameObject hitEffect = Instantiate(hitEffectPrefab, 
                    target.transform.position + Vector3.up * 0.5f, 
                    Quaternion.identity);
                Destroy(hitEffect, 1f);
            }
        }
    }
}