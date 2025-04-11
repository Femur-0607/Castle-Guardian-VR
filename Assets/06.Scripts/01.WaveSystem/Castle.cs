using UnityEngine;

public class Castle : LivingEntity
{
    [SerializeField] private float maxHealth = 1000f;
    public float MaxHealth => maxHealth;  // 읽기 전용 프로퍼티

    // 모든 성문이 공유할 정적 변수들
    private static float sharedCurrentHealth;
    private static bool isInitialized = false;
    
    protected override void Start()
    {
        startingHealth = maxHealth;
        
        // 첫 번째 성문만 체력을 초기화하고 나머지는 공유
        if (!isInitialized)
        {
            sharedCurrentHealth = maxHealth;
            isInitialized = true;
            base.Start();  // LivingEntity의 초기화 진행
        }
        else
        {
            // 이미 초기화된 경우 공유 체력값 사용
            currentHealth = sharedCurrentHealth;
        }
        
        EventManager.Instance.TriggerOnCastleInitialized(this);
    }
    
    public override void TakeDamage(float damage)
    {
        // 공유 체력 감소
        sharedCurrentHealth -= damage;
        
        // 내부 체력 값 동기화
        currentHealth = sharedCurrentHealth;
        
        // 체력이 0 이하면 죽음 처리
        if (currentHealth <= 0 && isAlive)
        {
            Die();
        }
        
        // 체력 변경 이벤트 발생 - UI 등 업데이트
        EventManager.Instance.TriggerOnCastleHealthChanged(currentHealth);
    }
    
    public override void Die()
    {
        if (!isAlive) return;
        
        base.Die();  // LivingEntity의 죽음 처리
        GameManager.Instance.EndGame(false);
    }
    
    // 게임 재시작 시 호출할 정적 메서드
    public static void ResetSharedHealth()
    {
        isInitialized = false;
    }
    
    // 비활성화 후 다시 활성화될 때 체력 동기화
    private void OnEnable()
    {
        if (isInitialized)
        {
            currentHealth = sharedCurrentHealth;
        }
    }
}