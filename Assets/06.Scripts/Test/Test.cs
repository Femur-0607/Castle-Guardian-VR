using UnityEngine;

public class Test : MonoBehaviour
{
    private void Update()
    {
        // 키보드 숫자키로 화살 전환
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ArrowManager.Instance.SwitchArrowType(ProjectileData.ProjectileType.Normal);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ArrowManager.Instance.SwitchArrowType(ProjectileData.ProjectileType.Explosive);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ArrowManager.Instance.SwitchArrowType(ProjectileData.ProjectileType.Poison);
        }
        
        // 마우스 휠로 화살 순환
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            ArrowManager.Instance.CycleNextArrow();
        }
    }
}
