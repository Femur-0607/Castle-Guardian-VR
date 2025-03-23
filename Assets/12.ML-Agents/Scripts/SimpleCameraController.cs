using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    public Camera leftCamera;
    public Camera centerCamera;
    public Camera rightCamera;
    
    private Camera currentCamera;
    
    private void Start()
    {  
        // 기본 중앙 카메라 활성화
        SwitchToCenterCamera();
    }
    
    public void SwitchToLeftCamera()
    {
        leftCamera.gameObject.SetActive(true);
        centerCamera.gameObject.SetActive(false);
        rightCamera.gameObject.SetActive(false);
        
        currentCamera = leftCamera;
        EventManager.Instance.CameraChangedEvent(currentCamera, "Left");
    }
    
    public void SwitchToCenterCamera()
    {
        leftCamera.gameObject.SetActive(false);
        centerCamera.gameObject.SetActive(true);
        rightCamera.gameObject.SetActive(false);
        
        currentCamera = centerCamera;
        EventManager.Instance.CameraChangedEvent(currentCamera, "Center");
    }
    
    public void SwitchToRightCamera()
    {
        leftCamera.gameObject.SetActive(false);
        centerCamera.gameObject.SetActive(false);
        rightCamera.gameObject.SetActive(true);
        
        currentCamera = rightCamera;
        EventManager.Instance.CameraChangedEvent(currentCamera, "Right");
    }
    
    public Camera GetCurrentCamera()
    {
        return currentCamera;
    }
}
