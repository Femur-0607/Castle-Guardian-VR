using UnityEngine;
using System.Collections;
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

    #endregion
}