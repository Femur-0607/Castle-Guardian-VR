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

        // 발사체 이동 시간 계산
        float distance = Vector3.Distance(firePoint.position, target.transform.position);
        float travelTime = distance / towerData.projectileSpeed;

        // 지연 폭발 데미지
        StartCoroutine(DelayedExplosion(target, travelTime));

        GameObject muzzleEffect = Instantiate(muzzleEffectPrefab, firePoint.position, firePoint.rotation);
        Destroy(muzzleEffect, 2f);

        // 투사체 생성
        GameObject projectileObj = Instantiate(explosionPrefab, firePoint.position, Quaternion.identity);

        // 투사체 이동 시작
        StartCoroutine(MoveProjectileWithPrediction(projectileObj, target.transform,
            towerData.projectileSpeed, travelTime, hitEffectPrefab));
    }



    private IEnumerator DelayedExplosion(Enemy target, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 타겟이 여전히 존재하는지 확인
        if (target == null || !target.gameObject.activeInHierarchy) yield break;

        Vector3 explosionPos = target.transform.position;

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
    
    private IEnumerator MoveProjectileWithPrediction(GameObject projectile, Transform target, 
    float speed, float estimatedTime, GameObject hitEffect = null)
{
    if (projectile == null || target == null) yield break;
    
    float startTime = Time.time;
    float elapsedTime = 0;
    Vector3 startPos = projectile.transform.position;
    
    // 초기 목표 지점 (직선으로 갈 경우 도달할 포인트)
    Vector3 initialTargetPos = target.position;
    
    while (projectile != null && target != null && elapsedTime < estimatedTime * 1.5f)
    {
        elapsedTime = Time.time - startTime;
        float normalizedTime = elapsedTime / estimatedTime; // 0~1 범위로 정규화된 시간
        
        if (normalizedTime >= 0.99f) // 도착 지점 도달 (약간 여유 두기)
        {
            // 도착 지점에 도달 (히트 이펙트 및 투사체 제거)
            if (projectile != null)
            {
                projectile.transform.position = target.position;
                
                // 히트 이펙트 생성
                if (hitEffect != null)
                {
                    Instantiate(hitEffect, target.position, Quaternion.identity);
                }
                
                Destroy(projectile);
            }
            yield break;
        }
        
        // 투사체 위치 계산 (곡선형 이동 패턴)
        Vector3 currentTargetPos = target.position;
        
        // 가중치 계산 (시간에 따라 변화)
        float directWeight = 1.0f - normalizedTime; // 초기에는 직선 이동 가중치가 높음
        float followWeight = normalizedTime;        // 나중에는 추적 가중치가 높음
        
        // 직선 이동 경로
        Vector3 directPath = Vector3.Lerp(startPos, initialTargetPos, normalizedTime);
        
        // 추적 이동 경로 (현재 위치에서 목표 방향으로 일정 비율 이동)
        Vector3 followDirection = (currentTargetPos - projectile.transform.position).normalized;
        Vector3 followPath = projectile.transform.position + 
            followDirection * (speed * Time.deltaTime * followWeight);
        
        // 포물선 요소 추가 (높이) - 폭발 타워는 높은 포물선
        float height = Mathf.Sin(normalizedTime * Mathf.PI) * 2.5f;
        
        // 최종 위치 계산 (직선 이동 + 추적)
        Vector3 newPosition = (directPath * directWeight + followPath * followWeight);
        newPosition.y += height; // 포물선 높이 적용
        
        // 투사체 이동 및 회전
        if (projectile != null)
        {
            projectile.transform.position = newPosition;
            
            // 진행 방향을 향해 회전
            Vector3 moveDirection = (newPosition - projectile.transform.position).normalized;
            if (moveDirection != Vector3.zero)
            {
                projectile.transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }
        
        yield return null;
    }
    
    // 시간 초과 시 투사체 제거
    if (projectile != null)
        Destroy(projectile);
}
}