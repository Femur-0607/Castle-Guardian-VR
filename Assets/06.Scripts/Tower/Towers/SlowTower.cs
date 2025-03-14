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
        
        // 발사체 이동 시간 계산
        float distance = Vector3.Distance(firePoint.position, target.transform.position);
        float travelTime = distance / towerData.projectileSpeed;
        
        // 지연 둔화 효과
        StartCoroutine(DelayedSlow(target, travelTime));
        
        // 여기서부터 추가된 시각 효과 부분
        // 발사 효과
        GameObject muzzleEffect = Instantiate(muzzleEffectPrefab, firePoint.position, firePoint.rotation);
        Destroy(muzzleEffect, 2f);
        
        // 투사체 생성
        GameObject projectileObj = Instantiate(slowPrefab, firePoint.position, Quaternion.identity);
        
        // 투사체 이동 시작
        StartCoroutine(MoveProjectileWithPrediction(projectileObj, target.transform,
            towerData.projectileSpeed, travelTime, hitEffectPrefab));
    }

    private IEnumerator DelayedSlow(Enemy target, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 타겟 유효성 체크
        if (target != null && target.gameObject.activeInHierarchy && target.IsAlive())
        {
            // 데미지 적용 (낮은 데미지)
            target.TakeDamage(towerData.attackDamage);
            
            // 둔화 효과 적용
            target.ApplySlowEffect(slowDuration, slowAmount);
        }
    }

    // ArcherTower에서 가져온 투사체 이동 코루틴
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
            
            // 포물선 요소 추가 (높이) - 슬로우 타워는 낮은 포물선
            float height = Mathf.Sin(normalizedTime * Mathf.PI) * 0.8f;
            
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