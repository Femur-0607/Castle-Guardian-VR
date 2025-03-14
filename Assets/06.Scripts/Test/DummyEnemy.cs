// DummyEnemy.cs - 테스트용 간단한 Enemy 구현
using UnityEngine;

public class DummyEnemy : MonoBehaviour
{
    // Enemy 인터페이스 필수 메서드 구현
    public bool IsAlive()
    {
        return true; // 항상 살아있는 상태
    }
    
    public void TakeDamage(float damage)
    {
        Debug.Log($"[DummyEnemy] 데미지 받음: {damage}");
        // 아무 동작 없음 (데미지 로그만 출력)
    }
}