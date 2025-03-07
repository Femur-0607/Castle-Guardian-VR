using UnityEngine;
using System.Collections;

public class SlowTower : Tower
{
    [Header("둔화 타워 설정")]
    [SerializeField] private Animator towerAnimator;
    [SerializeField] private AudioClip freezeSound;
    [SerializeField] private GameObject freezeEffectPrefab;
    [SerializeField] private float slowDuration = 3f;
    [SerializeField] private float slowAmount = 0.5f; // 50% 감속

    protected override void Attack(Enemy target)
    {
        // 애니메이션 재생
        if (towerAnimator != null)
        {
            towerAnimator.SetTrigger("Freeze");
        }
        
        // 사운드 효과
        if (freezeSound != null)
        {
            AudioSource.PlayClipAtPoint(freezeSound, transform.position);
        }
        
        // 발사체 이동 시간 계산
        float distance = Vector3.Distance(firePoint.position, target.transform.position);
        float travelTime = distance / towerData.projectileSpeed;
        
        // 지연 둔화 효과
        StartCoroutine(DelayedSlow(target, travelTime));
    }

    private IEnumerator DelayedSlow(Enemy target, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 타겟 유효성 체크
        if (target != null && target.gameObject.activeInHierarchy && target.IsAlive())
        {
            // 데미지 적용 (낮은 데미지)
            target.TakeDamage(towerData.attackDamage * 0.5f);
            
            // 둔화 효과 적용
            target.ApplySlowEffect(slowDuration, slowAmount);
            
            // 시각 효과
            if (freezeEffectPrefab != null)
            {
                GameObject effect = Instantiate(freezeEffectPrefab, 
                    target.transform.position, 
                    Quaternion.identity);
                effect.transform.SetParent(target.transform);
                Destroy(effect, slowDuration);
            }
        }
    }
}