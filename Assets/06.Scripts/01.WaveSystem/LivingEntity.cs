using UnityEngine;

// 오브젝트의 체력을 관리하는 스크립트 (몬스터, 성문 등)
public class LivingEntity : MonoBehaviour, IDamageable
{
    protected float startingHealth;// 시작 체력 값
    public float currentHealth { get; protected set; } // 현재 체력 값
    public bool isAlive {get; protected set;} // 생존 여부 불타입
    
    protected virtual void Start()
    {
        // 체력과 생존 여부 초기화
        currentHealth = startingHealth;
        isAlive = true;
    }
    
    public bool IsAlive() => isAlive;

    // 데미지 입을 시
    public virtual void TakeDamage(float damage)
    {
        if (!isAlive) return;
        
        currentHealth -= damage;

        if (currentHealth <= 0f) Die();
    }
    
    // 체력 회복
    public virtual void RestoreHealth(float newHealth)
    {
        if (!isAlive) return;
        
        currentHealth += newHealth;
    }

    // 사망 시
    public virtual void Die()
    {
        isAlive = false;
    }
}
