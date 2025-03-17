using UnityEngine;

public enum TowerType
{
    Normal,     // 기본 타워
    Explosive,  // 폭발 타워
    Slow,       // 둔화 타워
}

[CreateAssetMenu(fileName = "TowerData", menuName = "Tower/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("기본 속성")]
    public TowerType towerType = TowerType.Normal;
    public float attackDamage = 50f;
    public float attackRange = 10f;
    public float attackCooldown = 2f;
    public float projectileSpeed = 20f;
    
    [Header("업그레이드 정보")]
    public int upgradeLevel = 1;
    public int upgradeCost = 100;
    
    [Header("업그레이드 증가량")]
    public float damageIncreasePerLevel = 10f;
    public float rangeIncreasePerLevel = 1f;
    public float cooldownDecreasePerLevel = 0.1f;
}