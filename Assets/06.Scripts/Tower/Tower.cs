// Assets/06.Scripts/Tower/Tower.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Tower : MonoBehaviour, IUpgradeable
{
    [Header("기본 참조")]
    [SerializeField] protected TowerData towerData;
    [SerializeField] protected Transform firePoint;
    
    // 공격 관련 변수
    protected float lastAttackTime;
    protected List<Enemy> enemiesInRange = new List<Enemy>();
    protected Enemy currentTarget;
    
    // 타겟 감지 최적화 변수
    protected float detectionInterval = 1f;
    
    protected virtual void Start()
    {
        // 초기 스탯 설정
        lastAttackTime = -towerData.attackCooldown; // 바로 공격 가능하도록 설정

        // 주기적으로 적 감지 수행
        StartCoroutine(DetectEnemiesRoutine());
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
    
    // 주기적으로 적 감지
    protected IEnumerator DetectEnemiesRoutine()
    {
        while (true)
        {
            DetectEnemies();
            yield return new WaitForSeconds(detectionInterval);
        }
    }
    
    // 범위 내 모든 적 감지
    protected virtual void DetectEnemies()
    {
        enemiesInRange.Clear();
    
        // Physics.OverlapSphere로 범위 내 콜라이더 감지
        Collider[] colliders = Physics.OverlapSphere(transform.position, towerData.attackRange, LayerMask.GetMask("Enemy"));
        
        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent<Enemy>(out var enemy) && enemy.IsAlive())
            {
                enemiesInRange.Add(enemy);
            }
        }
    }
    
    // 새 타겟 찾기
    protected virtual void FindNewTarget()
    {
        if (enemiesInRange.Count == 0) return;
        
        // 가장 가까운 적 찾기
        float closestDistance = float.MaxValue;
        Enemy closestEnemy = null;
        
        foreach (Enemy enemy in enemiesInRange)
        {
            if (enemy == null || !enemy.gameObject.activeSelf || !enemy.IsAlive()) continue;
            
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
    
    // 공격 구현 (추상 메서드)
    protected abstract void Attack(Enemy target);
    
    // IUpgradeable 인터페이스 구현
    public virtual void UpgradeLevel()
    {
        towerData.upgradeLevel++;
        towerData.attackDamage += towerData.damageIncreasePerLevel;
        towerData.attackRange += towerData.rangeIncreasePerLevel;
        towerData.attackCooldown = Mathf.Max(0.1f, towerData.attackCooldown - towerData.cooldownDecreasePerLevel);
    }
    
    public virtual int GetUpgradeCost()
    {
        return towerData.upgradeCost;
    }
    
    // 디버깅용 공격 범위 시각화
    protected virtual void OnDrawGizmosSelected()
    {
        if (towerData != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, towerData.attackRange);
        }
    }
}