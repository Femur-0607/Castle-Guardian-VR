using UnityEngine;

/// <summary>
/// 독 화살 클래스: 충돌 시 초기 데미지와 함께 시간에 따른 지속 데미지(DoT)를 주는 특수 화살
/// </summary>
public class PoisonArrow : Projectile
{
    [Header("독 화살 설정")]
    [SerializeField] private GameObject poisonEffect;  // 독 지속 효과 파티클 프리팹 (적 몸에 붙는 효과)
    [SerializeField] private GameObject impactEffect;  // 충돌 즉시 효과 파티클 프리팹
    
    private float dotDamage = 10f;    // 시간당 지속 데미지(독 효과) 양
    private float dotDuration = 3f;   // 독 효과 지속 시간(초)
    
    /// <summary>
    /// 화살이 충돌했을 때 호출되는 메서드 (부모 클래스의 추상 메서드 구현)
    /// 직접 데미지를 주고 독 효과를 적용함
    /// </summary>
    /// <param name="collision">충돌 정보</param>
    public override void OnImpact(Collision collision)
    {
        // 이미 비활성화된 상태면 무시 (중복 처리 방지)
        if (!gameObject.activeSelf) return;
        
        // 충돌 효과 재생 (충돌 지점에 파티클 생성)
        if (impactEffect != null)
        {
            ParticleEffectPool.Instance.PlayEffect(impactEffect, collision.contacts[0].point, Quaternion.identity);
        }
        
        // 데미지를 받을 수 있는 객체인지 확인 (IDamageable 인터페이스 구현 여부)
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            // 초기 데미지 적용 (즉시 데미지)
            damageable.TakeDamage(currentDamage);
            
            // 적 유형의 객체인지 확인하여 독 효과 적용
            if (collision.gameObject.TryGetComponent<Enemy>(out var enemy))
            {
                // 이미 독 효과가 있는지 확인 (중복 적용 방지)
                DoTEffect existingEffect = enemy.GetComponent<DoTEffect>();
                if (existingEffect != null)
                {
                    // 기존 효과가 있으면 지속시간만 갱신 (스택 X, 시간 연장 O)
                    existingEffect.RefreshEffect(dotDuration);
                }
                else
                {
                    // 새 독 효과 추가 (DoTEffect 컴포넌트 부착)
                    DoTEffect newEffect = enemy.gameObject.AddComponent<DoTEffect>();
                    
                    // 독 효과 초기화 (데미지, 지속시간, 시각효과)
                    newEffect.Initialize(dotDamage, dotDuration, poisonEffect);
                }
            }
        }
        
        // 화살 풀에 반환 (재사용을 위함)
        ReturnToPool();
    }
    
    /// <summary>
    /// 화살 데이터에서 설정 정보 가져와 적용 (부모 클래스 메서드 확장)
    /// </summary>
    /// <param name="data">프로젝타일 데이터 스크립터블 오브젝트</param>
    public override void SetProjectileData(ProjectileData data)
    {
        // 부모 클래스의 기본 설정 적용 (데미지 등)
        base.SetProjectileData(data);
        
        // 독 데미지 설정 (데이터에 값이 있는 경우에만)
        if (data.dotDamage > 0)
        {
            dotDamage = data.dotDamage;
        }
        
        // 독 지속시간 설정 (데이터에 값이 있는 경우에만)
        if (data.effectDuration > 0)
        {
            dotDuration = data.effectDuration;
        }
    }
}
