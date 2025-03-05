using UnityEngine;

namespace ObjectPlacementVFX
{
    public class Placeable : MonoBehaviour
    {
        [Header("Can Place - Parameters")]
        [SerializeField] private Color placeableColor;
        [SerializeField] private float emissionPlaceable = 0.1f;

        [Header("Can't Place - Parameters")]
        [SerializeField] private Color unplaceableColor;
        [SerializeField] private float emissionUnplaceable = 0.5f;

        private Material material;
        private int collisionCount;
        private MeshRenderer mesh;

        private void Start()
        {
            mesh = GetComponent<MeshRenderer>();
            material = mesh.sharedMaterial;

            material.SetColor("_Color", placeableColor);
            material.SetFloat("_EmissionStrength", emissionPlaceable);
        }

        private void OnTriggerEnter(Collider other)
        {
            collisionCount += 1;
            if (collisionCount == 1)
            {
                material.SetColor("_Color", unplaceableColor);
                material.SetFloat("_EmissionStrength", emissionUnplaceable);

            }
        }

        private void OnTriggerExit(Collider other)
        {
            collisionCount -= 1;
            if (collisionCount == 0)
            {
                material.SetColor("_Color", placeableColor);
                material.SetFloat("_EmissionStrength", emissionPlaceable);
            }
        }

        public bool CanPlace()
        {
            return collisionCount == 0;
        }

        public void ChangeMaterial(Material mat)
        {
            material = mat;
            mesh.sharedMaterial = material;
        } 
    }
}
