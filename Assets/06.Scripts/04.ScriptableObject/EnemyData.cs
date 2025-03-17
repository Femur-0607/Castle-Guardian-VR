using UnityEngine;

public enum EnemyType
{
    Normal,     // 일반
    Archer,     // 원거리
    Scout,      // 속도
    Tanker,     // 탱커
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("기초 스탯")]
    public float startingHealth = 100f;
    public float moveSpeed = 3.5f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float goldDropAmount = 50f;

    // 추가: 적 타입 설정
    [Header("적 유형")]
    public EnemyType enemyType = EnemyType.Normal;
    
    // 특수 적 속성 (필요한 경우에만 사용)
    [Header("특수 속성")]
    // 궁수형 적 속성
    public float projectileSpeed = 15f;
    public float preferredAttackDistance = 8f;
    public GameObject projectilePrefab;
    
    // 정찰병 적 속성
    public float burstSpeedMultiplier = 1.5f;
    public float burstCooldown = 8f;
    
    // 탱커형 적 속성
    public float armorValue = 0.3f;
    public float buffRadius = 8f;
    
    // 유령형 적 속성
    public float phaseInDuration = 1.5f;
    public float playerStunDuration = 3f;
}