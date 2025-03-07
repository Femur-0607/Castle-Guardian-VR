// Assets/06.Scripts/ScriptableObject/ProjectileData.cs
using UnityEngine;

public enum ProjectileType
{
    Normal,     // 기본 화살
    Fire,       // 화염 화살
    Poison      // 독 화살
}

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Projectiles/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    [Header("기본 속성")]
    public ProjectileType projectileType = ProjectileType.Normal;
    public string projectileName = "기본 화살";
    public float damage = 50f;
    public float speed = 20f;
    public float lifeTime = 5f;
    
    [Header("특수 효과")]
    public float impactRadius = 0f;     // 폭발 반경 (0=단일 타겟)
    public float effectDuration = 0f;   // 지속 효과 시간 (둔화, 화상 등)
    public float dotDamage = 0f;        // 도트 데미지 (초당)
    
    [Header("업그레이드 정보")]
    public int upgradeLevel = 1;
    public int upgradeCost = 100;
    
    [Header("업그레이드 증가량")]
    public float damageIncreasePerLevel = 10f;
    public float radiusIncreasePerLevel = 0.5f;
    public float durationIncreasePerLevel = 0.5f;
}