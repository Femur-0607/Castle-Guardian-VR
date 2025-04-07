using UnityEngine;

public class Castle : LivingEntity
{
    [SerializeField] private float maxHealth = 1000f;
    public float MaxHealth => maxHealth;  // 읽기 전용 프로퍼티 추가
    
    protected override void Start()
    {
        startingHealth = maxHealth;
        base.Start();

        EventManager.Instance.TriggerOnCastleInitialized(this);
    }
    
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        
        // 체력 변경 이벤트 발생
        EventManager.Instance.TriggerOnCastleHealthChanged(currentHealth);
    }
    
    public override void Die()
    {
        base.Die();
        GameManager.Instance.EndGame(false);
    }
}