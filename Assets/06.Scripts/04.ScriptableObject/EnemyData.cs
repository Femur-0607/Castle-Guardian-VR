using UnityEngine;

public enum EnemyType
{
    Normal,     // 일반
    Archer,     // 원거리
    Scout,      // 속도
    Tanker,     // 탱커
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("기초 스탯")]
    public float startingHealth = 100f;
    public float moveSpeed = 3.5f;
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float goldDropAmount = 50f;

    // 추가: 적 타입 설정
    [Header("적 유형")]
    public EnemyType enemyType = EnemyType.Normal;
    
    [Header("웨이브 체력 증가")]
    public float healthIncreasePerWave = 10f;  // 웨이브마다 10씩 증가

    // 웨이브 번호에 따른 체력 계산 메서드
    public float CalculateHealthForWave(int waveNumber)
    {
        // 기본 체력 + (웨이브 번호 - 1) * 증가량
        return startingHealth + (waveNumber - 1) * healthIncreasePerWave;
    }
}