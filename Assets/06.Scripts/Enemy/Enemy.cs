using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public enum EnemyState
{
    Idle,      // 초기 상태. NavMeshAgent가 활성화되기 전에는 아무 행동도 하지 않음.
    Chasing, // 추적
    Attacking, // 공격
    Dead // 사망
}

// Enemy에 상속할 베이스
public class Enemy : LivingEntity
{
    #region 필드 변수
    
    [Header("참조")]
    private IObjectPool<Enemy> pool;
    [SerializeField] private NavMeshAgent agent;
    public Transform target;        // 성문
    public EnemyData enemyData;     // 'Enemy'의 스테이터스를 담당
    public SpawnManager spawnManager;
    
    private float lastAttackTime;
    private EnemyState currentState;    // 현재 상태 알려주는 이넘 변수
    
    #endregion

    #region 유니티 이벤트 함수

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    protected override void Start()
    {
        // 먼저 스크립터블 오브젝트 불러오기
        if (enemyData != null)
        {
            startingHealth = enemyData.startingHealth;
            agent.speed = enemyData.moveSpeed;
            agent.stoppingDistance = enemyData.attackRange;
        }
        
        base.Start();   // 초기화 진행
        
        currentState = EnemyState.Idle;
    }
    
    void Update()
    {
        if (!isAlive) return;
        
        // FSM 구현
        switch (currentState)
        {
            case EnemyState.Idle:
                break;
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
        
        currentState = EnemyState.Idle;
        
        // NavMeshAgent를 딜레이 후 활성화하여 추적 시작
        StartCoroutine(DelayedAgentActivation(1f));
    }
    
    private IEnumerator DelayedAgentActivation(float delay)
    {
        agent.enabled = false;
        
        yield return new WaitForSeconds(delay);
        
        agent.enabled = true;
        
        BeginChase();
    }

    #endregion

    #region FSM (Finite State Machine) 유한상태기계
    
    /// <summary>
    /// 적의 추적을 시작하는 메서드
    /// NavMeshAgent가 활성화되고 유효한 NavMesh 위에 배치되었음을 전제로 함.
    /// </summary>
    public void BeginChase()
    {
        if (target != null && agent.isOnNavMesh)
        {
            currentState = EnemyState.Chasing;
            agent.isStopped = false;
        }
        else
        {
            Debug.LogWarning("[Enemy] BeginChase() 호출 시, agent가 NavMesh 위에 없거나 target이 null입니다.");
        }
    }
    
    /// <summary>
    /// 추적 상태
    /// </summary>
    void Chase()
    {
        if (target != null)
        {
            agent.SetDestination(target.position); // 성문으로 이동
            
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
        
        spawnManager.EnemyDied(this);

        pool?.Release(this);
        
        // 사망 시 게임매니저에게 골드 전달
        GameManager.Instance.gameMoney += (int)enemyData.goldDropAmount;
        
        // 추가 사망 애니메이션이나 이펙트 구현 가능
    }
    
    #endregion
}
