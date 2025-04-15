using UnityEngine;
using System.Collections.Generic;

public abstract class Tower : MonoBehaviour, IUpgradeable
{
    #region 필드 변수

    [Header("기본 참조")]
    [SerializeField] protected TowerData towerData;
    [SerializeField] protected Transform firePoint;

    // 공격 관련 변수
    protected float lastAttackTime;
    protected List<Enemy> enemiesInRange = new List<Enemy>();
    protected Enemy currentTarget;

    // 감지용 SphereCollider
    protected SphereCollider detectionCollider;

    #endregion

    #region 유니티 함수

    protected virtual void Awake()
    {
        detectionCollider = GetComponent<SphereCollider>();
        detectionCollider.isTrigger = true; // 트리거로 설정
    }

    protected virtual void Start()
    {
        // 초기 스탯 설정
        lastAttackTime = -towerData.attackCooldown; // 바로 공격 가능하도록 설정

        // SphereCollider 범위 설정
        UpdateDetectionRange();
    }

    protected virtual void Update()
    {
        // 현재 타겟 체크 및 공격 가능 여부 확인
        if (currentTarget != null && currentTarget.IsAlive() && IsInRange(currentTarget))
        {
            // 공격 쿨다운 체크
            if (Time.time >= lastAttackTime + towerData.attackCooldown)
            {
                Attack(currentTarget);
                lastAttackTime = Time.time;
            }
        }
        else
        {
            // 타겟이 없거나 죽었거나 범위를 벗어난 경우 새 타겟 찾기
            FindNewTarget();
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Enemy>(out var enemy) && enemy.IsAlive())
        {
            if (!enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Add(enemy);

                // 현재 타겟이 없으면 이 적을 타겟으로 설정
                if (currentTarget == null)
                {
                    currentTarget = enemy;
                }
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Enemy>(out var enemy))
        {
            enemiesInRange.Remove(enemy);

            // 현재 타겟이 범위를 벗어났다면 새 타겟 찾기
            if (currentTarget == enemy)
            {
                currentTarget = null;
                FindNewTarget();
            }
        }
    }

    #endregion

    #region 범위 및 타겟 관련 메서드

    // 범위 업데이트 (업그레이드 시에도 호출)
    protected void UpdateDetectionRange()
    {
        if (detectionCollider != null && towerData != null)
        {
            detectionCollider.radius = towerData.attackRange;
        }
    }

    // 새 타겟 찾기
    protected virtual void FindNewTarget()
    {
        if (enemiesInRange.Count == 0) return;

        // 리스트를 순회하며 유효한 적 찾기
        enemiesInRange.RemoveAll(e => e == null || !e.gameObject.activeSelf || !e.IsAlive());

        if (enemiesInRange.Count == 0) return;

        // 가장 가까운 적 찾기
        float closestDistance = float.MaxValue;
        Enemy closestEnemy = null;

        foreach (Enemy enemy in enemiesInRange)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        currentTarget = closestEnemy;
    }

    // 적이 타워 공격 범위 내에 있는지 확인
    protected virtual bool IsInRange(Enemy enemy)
    {
        if (enemy == null || !enemy.gameObject.activeSelf || !enemy.IsAlive()) return false;

        return Vector3.Distance(transform.position, enemy.transform.position) <= towerData.attackRange;
    }

    #endregion

    #region 업그레이드 및 공격 관련 메서드

    // 업그레이드 시 범위 업데이트
    public virtual void UpgradeLevel()
    {
        towerData.upgradeLevel++;
        towerData.attackDamage += towerData.damageIncreasePerLevel;
        towerData.attackRange += towerData.rangeIncreasePerLevel;
        towerData.attackCooldown = Mathf.Max(0.1f, towerData.attackCooldown - towerData.cooldownDecreasePerLevel);

        // 범위 업데이트
        UpdateDetectionRange();
    }

    // 공격 구현 (추상 메서드)
    protected abstract void Attack(Enemy target);

    // IUpgradeable 인터페이스 구현
    public virtual int GetUpgradeCost()
    {
        return towerData.upgradeCost;
    }

    protected IEnumerator<object> MoveProjectileWithPrediction(
        GameObject projectile,      // 이동시킬 투사체
        Transform target,          // 목표물
        float speed,              // 투사체 속도
        float estimatedTime,      // 예상 도착 시간
        GameObject hitEffect = null,  // 타격 이펙트
        float heightMultiplier = 1.5f)  // 포물선 높이 배수
    {
        // 투사체나 타겟이 없으면 코루틴 종료
        if (projectile == null || target == null) yield break;
        
        // Transform 캐싱으로 성능 최적화
        Transform projectileTransform = projectile.transform;
        
        // 시간 관련 변수 초기화
        float startTime = Time.time;      // 시작 시간
        float elapsedTime = 0;            // 경과 시간
        Vector3 startPos = projectileTransform.position;  // 시작 위치
        Vector3 initialTargetPos = target.position;       // 초기 타겟 위치
        
        // 투사체 이동 루프 (예상 시간의 1.5배까지)
        while (projectile != null && target != null && elapsedTime < estimatedTime * 1.5f)
        {
            // 경과 시간 계산
            elapsedTime = Time.time - startTime;
            float normalizedTime = elapsedTime / estimatedTime;  // 0~1 사이의 정규화된 시간
            
            // 타겟 도착 체크 (99% 이상 도달)
            if (normalizedTime >= 0.99f)
            {
                if (projectile != null)
                {
                    // 타겟에 데미지 적용
                    if (target.TryGetComponent<Enemy>(out var enemy))
                    {
                        enemy.TakeDamage(towerData.attackDamage);
                    }
                    
                    // 타격 이펙트 생성
                    if (hitEffect != null)
                    {
                        Instantiate(hitEffect, target.position, Quaternion.identity);
                    }
                    
                    // 투사체 제거
                    Destroy(projectile);
                }
                yield break;  // 코루틴 종료
            }
            
            // 투사체 위치 계산 (최적화)
            float directWeight = 1.0f - normalizedTime;  // 직선 경로 가중치
            float followWeight = normalizedTime;         // 추적 경로 가중치
            
            // 직선 경로 계산 (시작점에서 초기 타겟 위치로)
            Vector3 directPath = Vector3.Lerp(startPos, initialTargetPos, normalizedTime);
            
            // 추적 경로 계산 (현재 위치에서 타겟 방향으로)
            Vector3 followDirection = (target.position - projectileTransform.position).normalized;
            Vector3 followPath = projectileTransform.position + 
                followDirection * (speed * Time.deltaTime * followWeight);
            
            // 포물선 높이 계산 (사인 함수 사용)
            float height = Mathf.Sin(normalizedTime * Mathf.PI) * heightMultiplier;
            
            // 최종 위치 계산 (직선과 추적 경로의 가중 평균 + 높이)
            Vector3 newPosition = (directPath * directWeight + followPath * followWeight);
            newPosition.y += height;
            
            // 새로운 위치 적용
            projectileTransform.position = newPosition;
            
            yield return null;  // 다음 프레임까지 대기
        }
        
        // 시간 초과 시 투사체 제거
        if (projectile != null)
            Destroy(projectile);
    }

    #endregion
}