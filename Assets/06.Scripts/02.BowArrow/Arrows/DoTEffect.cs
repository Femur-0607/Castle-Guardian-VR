using System.Collections;
using UnityEngine;

/// <summary>
/// 시간에 따른 데미지(DoT, Damage over Time) 효과를 처리하는 컴포넌트
/// 적에게 부착되어 일정 시간 동안 주기적으로 데미지를 입히는 역할을 함
/// </summary>
public class DoTEffect : MonoBehaviour
{
    // 주요 상태 변수들
    private float damagePerTick = 10f;    // 매 틱당(한 번의 데미지 적용) 입히는 데미지 양
    private float totalDuration = 3f;     // 효과의 총 지속 시간(초)
    private float tickInterval = 0.5f;    // 데미지가 적용되는 간격(초)
    private float remainingTime = 0f;     // 남은 효과 지속 시간(초)
    
    private GameObject effectInstance;    // 시각적 효과(파티클)를 가진 게임오브젝트 인스턴스
    private IDamageable damageableTarget; // 데미지를 받을 수 있는 대상(인터페이스)
    
    private Coroutine dotCoroutine;       // 도트 데미지 적용 코루틴 참조(중복 방지용)
    
    
    /// <summary>
    /// 독 효과 초기화 - 독 화살이 적에게 맞았을 때 호출됨
    /// </summary>
    /// <param name="damage">틱당 데미지 양</param>
    /// <param name="duration">효과 지속 시간(초)</param>
    /// <param name="effectPrefab">시각적 효과 프리팹(파티클)</param>
    public void Initialize(float damage, float duration, GameObject effectPrefab)
    {
        // 기본 속성 설정
        damagePerTick = damage;
        totalDuration = duration;
        remainingTime = duration;

        // 데미지 받을 수 있는 대상 컴포넌트 가져오기
        damageableTarget = GetComponent<IDamageable>();

        // 시각 효과(파티클) 생성 - 적 오브젝트의 자식으로 배치
        if (effectPrefab != null)
        {
            effectInstance = Instantiate(effectPrefab, transform.position, Quaternion.identity, transform);
        }

        // 기존 코루틴이 있다면 정지 후 새로운 코루틴 시작
        // 이미 독 효과가 적용 중인 경우, 기존 효과를 중지하고 새로운 효과 시작
        if (dotCoroutine != null)
        {
            StopCoroutine(dotCoroutine);
        }

        // 데미지 적용 코루틴 시작
        dotCoroutine = StartCoroutine(ApplyDamageOverTime());
    }
    
    /// <summary>
    /// 효과 갱신(지속시간 연장) - 이미 독 효과가 있는 적에게 다시 독 화살이 맞았을 때 호출됨
    /// </summary>
    /// <param name="duration">연장할 지속 시간(초)</param>
    public void RefreshEffect(float duration)
    {
        // 남은 시간과 새로운 지속시간 중 더 긴 시간으로 설정 (최대 지속시간 보장)
        remainingTime = Mathf.Max(remainingTime, duration);
        
        // 만약 코루틴이 없다면(종료되었다면) 새로 시작
        if (dotCoroutine == null)
        {
            dotCoroutine = StartCoroutine(ApplyDamageOverTime());
        }
    }
    
    /// <summary>
    /// 일정 간격으로 데미지를 적용하는 코루틴
    /// </summary>
    private IEnumerator ApplyDamageOverTime()
    {
        // 남은 시간이 0보다 큰 동안 반복
        while (remainingTime > 0)
        {
            // 데미지 적용 (대상이 유효한지 확인)
            if (damageableTarget != null)
            {
                // IDamageable 인터페이스의 TakeDamage 메서드 호출하여 데미지 적용
                damageableTarget.TakeDamage(damagePerTick);
                
                // 데미지 텍스트 표시 (선택 사항) - 구현 시 주석 해제
                // DamageTextManager.Instance?.ShowDamageText(transform.position + Vector3.up, damagePerTick, Color.green);
            }
            
            // 다음 틱까지 대기
            yield return new WaitForSeconds(tickInterval);
            
            // 남은 시간 감소
            remainingTime -= tickInterval;
        }
        
        // 효과 종료 처리 - 파티클 효과 제거
        if (effectInstance != null)
        {
            Destroy(effectInstance);
        }
        
        // 효과 컴포넌트 자체도 제거 (효과 완전 종료)
        Destroy(this);
    }
    
    /// <summary>
    /// 컴포넌트가 파괴될 때 호출 - 파티클 효과 정리
    /// </summary>
    private void OnDestroy()
    {
        // 시각 효과(파티클) 오브젝트가 있다면 제거
        if (effectInstance != null)
        {
            Destroy(effectInstance);
        }
    }
}