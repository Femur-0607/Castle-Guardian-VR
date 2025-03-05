using UnityEngine;

namespace ObjectPlacementVFX
{

    public class Placement : MonoBehaviour
    {
        [SerializeField] private Vector2 panningSpeed;

        [SerializeField] private GameObject buildingBlueprint;
        [SerializeField] private GameObject buildingPrefab;

        [SerializeField] private Material[] materials;

        private Placeable placeable;

        private bool placing;

        private void Update()
        {
            float translationZ = Input.GetAxis("Vertical") * panningSpeed.x;
            float translationX = Input.GetAxis("Horizontal") * panningSpeed.y;

            if (placing & Input.GetMouseButtonDown(0))
            {
                Place();
            }

            buildingBlueprint.transform.position += new Vector3(translationX, 0f, translationZ);

            //Other Input
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartPlace();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeVersion(0);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeVersion(1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ChangeVersion(2);
            }
        }

        public void StartPlace()
        {
            buildingBlueprint.SetActive(true);
            placeable = buildingBlueprint.GetComponent<Placeable>();
            placing = true;
        }

        public void EndPlace()
        {
            buildingBlueprint.SetActive(false);
            placing = false;
        }

        public void Place()
        {
            if (placeable.CanPlace())
            {
                Instantiate(buildingPrefab, buildingBlueprint.transform.position, buildingBlueprint.transform.rotation);
            }
        }

        public void ChangeVersion(int version)
        {
            if (placeable == null) { return; }

            placeable.ChangeMaterial(materials[version]);
        }
    }
}
