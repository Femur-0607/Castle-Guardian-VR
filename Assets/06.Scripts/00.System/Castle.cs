using UnityEngine;

public class Castle : MonoBehaviour, IDamageable
{
    private static float sharedMaxHealth;
    private static float sharedCurrentHealth;
    private static bool isInitialized = false;
    
    [Header("스테이터스")]
    [SerializeField] private float maxHealth = 1000f;
    
    public float MaxHealth => sharedMaxHealth;
    public float currentHealth => sharedCurrentHealth;

    private void Awake()
    {
        // 첫 번째 Castle 인스턴스에서만 초기화
        if (!isInitialized)
        {
            sharedMaxHealth = maxHealth;
            sharedCurrentHealth = maxHealth;
            isInitialized = true;
        }
    }
    
    protected void Start()
    {
        EventManager.Instance.TriggerOnCastleInitialized(this);
    }
    
    public void TakeDamage(float damage)
    {
        sharedCurrentHealth -= damage;
        sharedCurrentHealth = Mathf.Max(0f, sharedCurrentHealth);
        
        EventManager.Instance.TriggerOnCastleHealthChanged(sharedCurrentHealth);
        
        if (sharedCurrentHealth <= 0)
        {
            Die();
        }
    }
    
    public bool IsAlive()
    {
        return sharedCurrentHealth > 0;
    }
    
    private void Die()
    {
        GameManager.Instance.EndGame(false);
    }
}