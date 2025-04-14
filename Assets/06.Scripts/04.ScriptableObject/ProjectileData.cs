// Assets/06.Scripts/ScriptableObject/ProjectileData.cs
using UnityEngine;

/// <summary>
/// 화살 투사체 데이터 - 다양한 화살 타입의 속성 정의
/// 스크립터블 오브젝트로 구현되어 에디터에서 쉽게 수정 가능
/// </summary>
[CreateAssetMenu(fileName = "ProjectileData", menuName = "Projectiles/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    /// <summary>
    /// 화살 타입 열거형 - 게임에서 사용 가능한 화살 종류 정의
    /// </summary>
    public enum ProjectileType
    {
        Normal,     // 기본 화살 - 단일 타겟에 직접 데미지
        Explosive,       // 폭발 화살 - 범위 데미지와 넉백 효과
        Poison      // 독 화살 - 초기 데미지 + 시간에 따른 지속 데미지
    }

    [Header("기본 속성")]
    public ProjectileType projectileType = ProjectileType.Normal;  // 화살 타입 (기본, 폭발, 독 등)
    public float baseDamage = 50f;                                // 기본 데미지 (업그레이드 전)
    public float lifeTime = 5f;                                  // 화살 생존 시간 (초 단위)
    
    [Header("특수 효과")]
    public float baseImpactRadius = 0f;     // 기본 폭발 반경 (0=단일 타겟, 양수=범위 데미지)
    public float baseEffectDuration = 0f;   // 기본 지속 효과 시간 (독, 화상 등의 지속 시간)
    public float baseDotDamage = 0f;        // 기본 도트 데미지 (초당 입히는 지속 데미지 양)
    
    [Header("공통 업그레이드 변수")]
    public float baseMultiplier = 1.0f;     // 공통 강화 배수 (활 업그레이드로 증가)
    
    [Header("업그레이드 증가량")]
    public float damageIncreasePerLevel = 10f;    // 레벨당 기본 데미지 증가량
    public float radiusIncreasePerLevel = 0.5f;   // 레벨당 폭발 반경 증가량 (폭발 화살용)
    public float durationIncreasePerLevel = 0.5f; // 레벨당 지속 효과 시간 증가량 (독/화상용)
    public float baseMultiplierIncreasePerLevel = 0.1f; // 활 업그레이드 시 곱해지는 증가량
    
    // 실제 데미지 계산 (곱 적용) - 공통 업그레이드와 개별 업그레이드 모두 반영
    public float damage { 
        get { return baseDamage * baseMultiplier; } 
    }
    
    // 실제 폭발 반경 계산 (곱 적용)
    public float impactRadius { 
        get { return baseImpactRadius * baseMultiplier; } 
    }
    
    // 실제 효과 지속시간 계산 (곱 적용)
    public float effectDuration { 
        get { return baseEffectDuration * baseMultiplier; } 
    }
    
    // 실제 도트 데미지 계산 (곱 적용)
    public float dotDamage { 
        get { return baseDotDamage * baseMultiplier; } 
    }
}