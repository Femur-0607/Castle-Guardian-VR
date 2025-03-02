using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

namespace ProjectileCurveVisualizerSystem
{
    public class Projectile : MonoBehaviour
    {
        private IObjectPool<Projectile> pool;      // 자신을 관리하는 풀
        
        public Transform projectileSpawnPoint;
        // Rigidbody 컴포넌트를 통해 물리 계산을 수행합니다.
        public Rigidbody projectileRigidbody;
        // MeshCollider를 이용하여 충돌 판정을 합니다.
        public MeshCollider projectileMeshCollider;

        private float lifeTime = 5f; // 화살 쏜 이후 최대 생존 시간
        private float spawnTime;
        private float projectileDamage = 999999f; // 무조건 한방 화살 대미지
        
        public void SetPoolReference(IObjectPool<Projectile> poolRef)
        {
            pool = poolRef;
        }
        
        public void ResetState()
        {
            // 자식 오브젝트 위치 초기화
            projectileSpawnPoint.localPosition = Vector3.zero;
            projectileSpawnPoint.localRotation = Quaternion.identity;
            
            // 오브젝트가 풀에서 Get될 때마다 상태 초기화
            projectileRigidbody.linearVelocity = Vector3.zero;
            projectileRigidbody.angularVelocity = Vector3.zero;
            
            // 물리 엔진이 오브젝트를 "잠자기" 상태에서 깨어나도록 강제로 깨움
            projectileRigidbody.WakeUp();
            
            // 충돌 판정을 위한 Collider 재설정: 이전 충돌 잔재 제거
            projectileMeshCollider.enabled = false;
            projectileMeshCollider.enabled = true;
            
            spawnTime = Time.time;

            // 트레일 도 초기화
            var trail = GetComponent<TrailRenderer>();
            if (trail != null) trail.Clear();
        }
        
        private void Update()
        {
            // (옵션) 발사 후 일정 시간 지나면 풀로 반환
            if (Time.time - spawnTime > lifeTime)
            {
                pool.Release(this);
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log(collision.gameObject.name);
            // 충돌한 오브젝트에 IDamageable가 있을경우 가져오기
            if(collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(projectileDamage);
                Debug.Log(projectileDamage);
            }
            
            // 충돌 후 1초 뒤 풀로 반환
            StartCoroutine(ReturnAfterDelay(1f));
        }
        
        IEnumerator ReturnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // 풀로 반환
            if (pool != null)
            {
                pool.Release(this);
            }
            else
            {
                // 풀이 없는 경우(디버그용)
                Destroy(gameObject);
            }
        }

        // 화살(또는 투사체)을 발사하는 메서드입니다.
        // 인자로 전달받은 'velocity'를 이용해 물리 엔진에서 속도를 부여합니다.
        public void Throw(Vector3 velocity)
        {
            // 충돌 감지를 위해 Collider를 활성화합니다.
            projectileMeshCollider.enabled = true;
            // 중력을 사용하도록 설정하여 투사체가 자연스럽게 낙하하도록 합니다.
            projectileRigidbody.useGravity = true;
            // Rigidbody에 직접 선형 속도를 설정합니다.
            // projectileRigidbody.linearVelocity = velocity;
            // AddForce를 사용해 Impulse 모드로 초기 속도를 부여합니다.
            projectileRigidbody.AddForce(velocity, ForceMode.Impulse);
        }
    }
}

