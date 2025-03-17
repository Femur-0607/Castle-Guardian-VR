using UnityEngine;

// Enemy, Castle Gate 등 공격을 받을 수 있는 모든 대상에게 적용, 피해를 입었을때 TakeDamage()가 호출
public interface IDamageable
{
    bool IsAlive(); // 생존여부

    void TakeDamage(float damage); // 데미지 적용
}
    
