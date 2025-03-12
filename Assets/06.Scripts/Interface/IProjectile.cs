using UnityEngine;

public interface IProjectile
{
    void Launch(Vector3 velocity);
    void OnImpact(Collision collision);
}