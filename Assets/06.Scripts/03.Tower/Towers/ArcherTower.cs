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

        Debug.Log($"[ArcherTower] 공격 시작 - 타겟: {target.name}, 위치: {target.transform.position}");
        
        // 화살 발사 사운드를 3D로 재생 (타워의 위치에서)
        SoundManager.Instance.PlaySound3D("ArrowTowerShoot", firePoint.position);
        Debug.Log($"[ArcherTower] 사운드 재생 시도 - 위치: {firePoint.position}");

        // 화살이 적에게 도달하는 시간 계산
        float distance = Vector3.Distance(firePoint.position, target.transform.position);
        float travelTime = distance / towerData.projectileSpeed;

        // 지연 데미지 처리
        StartCoroutine(DelayedDamage(target, travelTime));

        // 시각적 효과 (투사체 생성)
        GameObject muzzleEffect = Instantiate(muzzleEffectPrefab, firePoint.position, firePoint.rotation);
        Destroy(muzzleEffect, 2f);

        // 투사체 생성
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // 추적 로직 적용
        StartCoroutine(MoveProjectileWithPrediction(projectileObj, target.transform,
            towerData.projectileSpeed, travelTime, hitEffectPrefab));
    }

    private IEnumerator DelayedDamage(Enemy target, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 타겟 유효성 체크
        if (target != null && target.gameObject.activeInHierarchy && target.IsAlive())
        {
            // 데미지 적용
            target.TakeDamage(towerData.attackDamage);
        }
    }
    
    // 예측과 추적을 결합한 투사체 이동
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
            
            // 포물선 요소 추가 (높이)
            float height = Mathf.Sin(normalizedTime * Mathf.PI) * 1.5f;
            
            // 최종 위치 계산 (직선 이동 + 추적)
            Vector3 newPosition = (directPath * directWeight + followPath * followWeight);
            newPosition.y += height; // 포물선 높이 적용
            
            // 투사체 이동 및 회전
            projectile.transform.position = newPosition;
            
            yield return null;
        }
        
        // 시간 초과 시 투사체 제거
        if (projectile != null)
            Destroy(projectile);
    }
}