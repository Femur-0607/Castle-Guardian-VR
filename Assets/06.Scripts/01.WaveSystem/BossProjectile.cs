using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    private int damage;
    private float speed;
    private float lifetime = 5f;  // 최대 생존 시간
    private float damageInterval = 0.5f;  // 데미지 주는 간격
    private float damageRadius = 2f;  // 데미지 반경
    private float lastDamageTime;  // 마지막 데미지 처리 시간
    
    private void Start()
    {
        // 일정 시간 후 파괴
        Destroy(gameObject, lifetime);
        lastDamageTime = Time.time;
    }
    
    public void Initialize(int dmg, float spd)
    {
        damage = dmg;
        speed = spd;
    }
    
    private void Update()
    {
        // 앞으로 이동
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        
        // 일정 간격으로 데미지 영역 체크
        if (Time.time - lastDamageTime >= damageInterval)
        {
            CheckDamageArea();
            lastDamageTime = Time.time;
        }
    }
    
    private void CheckDamageArea()
    {
        // 주변의 모든 콜라이더 검사
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (var hitCollider in hitColliders)
        {
            // 성에 데미지
            if (hitCollider.CompareTag("Castle"))
            {
                Castle castle = hitCollider.GetComponent<Castle>();
                if (castle != null)
                {
                    castle.TakeDamage(damage);
                }
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어와 충돌 시
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
    
    // 디버그용 기즈모
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}