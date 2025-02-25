using UnityEngine;

namespace ProjectileCurveVisualizerSystem
{
    public class Projectile : MonoBehaviour
    {
        // Rigidbody 컴포넌트를 통해 물리 계산을 수행합니다.
        public Rigidbody projectileRigidbody;
        // MeshCollider를 이용하여 충돌 판정을 합니다.
        public MeshCollider projectileMeshCollider;

        // 화살(또는 투사체)을 발사하는 메서드입니다.
        // 인자로 전달받은 'velocity'를 이용해 물리 엔진에서 속도를 부여합니다.
        public void Throw(Vector3 velocity)
        {
            // 충돌 감지를 위해 Collider를 활성화합니다.
            projectileMeshCollider.enabled = true;
            // 중력을 사용하도록 설정하여 투사체가 자연스럽게 낙하하도록 합니다.
            projectileRigidbody.useGravity = true;
            // Rigidbody에 직접 선형 속도를 설정합니다.
            projectileRigidbody.linearVelocity = velocity;
        }
    }
}