using UnityEngine;

public class Castle : LivingEntity
{
    [SerializeField] private float maxHealth = 1000f;
    
    protected override void Start()
    {
        startingHealth = maxHealth;
        base.Start();
    }
    
    public override void TakeDamage(float damage) => base.TakeDamage(damage);
    
    public override void Die()
    {
        base.Die();
        GameManager.Instance.EndGame(false);
    }
}