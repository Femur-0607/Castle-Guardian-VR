using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

  #region Enemy의 상태 표시 enum

public enum EnemyState
{
    Chasing, // 추적
    Attacking, // 공격
    Dead // 사망
}

#endregion

// Enemy에 상속할 베이스
public class Enemy : LivingEntity
{
    #region 필드 변수
    
    [Header("참조")]
    private IObjectPool<Enemy> pool;
    private NavMeshAgent agent;
    public Transform target;        // 성문
    public EnemyData enemyData;     // 'Enemy'의 스테이터스를 담당
    
    private float lastAttackTime;
    private EnemyState currentState;
    
    #endregion

    #region 유니티 이벤트 함수
    
    protected override void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // 먼저 스크립터블 오브젝트 불러오기
        if (enemyData != null)
        {
            startingHealth = enemyData.startingHealth;
            agent.speed = enemyData.moveSpeed;
            agent.stoppingDistance = enemyData.attackRange;
        }
        
        // 초기화 진행
        base.Start();
        
        
        currentState = EnemyState.Chasing; // 생성 즉시 추적 상태
    }
    
    void Update()
    {
        if (!isAlive) return;
        
        // FSM 구현
        switch (currentState)
        {
            case EnemyState.Chasing:
                Chase();
                break;
            case EnemyState.Attacking:
                Attack();
                break;
            case EnemyState.Dead:
                // 사망 상태에서는 행동 안함.
                break;
        }
    }
    
    #endregion

    #region 오브젝트 풀 관련

    public void SetPoolReference(IObjectPool<Enemy> poolRef)
    {
        pool = poolRef;
    }
    
    /// <summary>
    /// Pool에서 Get될 때마다 호출될 초기화 로직
    /// </summary>
    public void ResetState()
    {
        // LivingEntity에 체력 초기값이 있다면 여기서 재설정
        currentHealth = startingHealth;
        isAlive = true;

        if (agent != null)
        {
            agent.isStopped = false;
            // agent.enabled = true; // 필요시
        }

        // FSM, Animator 등을 초기 상태로 돌리는 로직
        // ...
    }

    #endregion

    #region FSM (Finite State Machine) 유한상태기계
    
    /// <summary>
    /// 추적 상태
    /// </summary>
    void Chase()
    {
        if (target != null)
        {
            agent.SetDestination(target.position); // 성문으로 이동
            
            float distance = Vector3.Distance(transform.position, target.position);
            
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) // 공격범위안에 성문이 들어오면
            {
                currentState = EnemyState.Attacking; // 공격상태로 전환
                agent.isStopped = true; // 공격 시에는 이동을 멈춤
            }
        }
    }
    
    /// <summary>
    /// 공격 상태
    /// </summary>
    void Attack()
    {
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            
            if (distance > agent.stoppingDistance)
            {
                // 목표가 범위를 벗어나면 다시 추격 상태로 전환
                currentState = EnemyState.Chasing;
                agent.isStopped = false;
                return;
            }

            // 현재시간에서 마지막시간을 뺀 값이 쿨타임보다 커질경우 공격 실행
            if (Time.time - lastAttackTime >= enemyData.attackCooldown)
            {
                // 리빙 엔티티 스크립트가 있어야지 공격 가능
                LivingEntity castle = target.GetComponent<LivingEntity>();
                
                if (castle != null)
                {
                    castle.TakeDamage(enemyData.attackDamage);
                }
                
                // 현재 시간을 할당
                lastAttackTime = Time.time;
            }
        }
    }

    /// <summary>
    /// 사망 상태
    /// </summary>
    public override void Die()
    {
        base.Die();
        
        currentState = EnemyState.Dead;
        
        agent.isStopped = true;
        // 추가 사망 애니메이션이나 이펙트 구현 가능
    }
    
    #endregion
}
