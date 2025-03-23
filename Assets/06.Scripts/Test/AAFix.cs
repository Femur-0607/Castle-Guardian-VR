using System.Collections;
using UnityEngine;

public class AAFix : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(Fix());
    }

    IEnumerator Fix()
    {
        yield return new WaitForEndOfFrame();
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 0.7f;
        yield return new WaitForEndOfFrame();
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1.0f;
        yield return new WaitForEndOfFrame();

        // 필요한 경우 한 번 더 변경
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 0.95f;
        yield return new WaitForEndOfFrame();
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = 1.0f;
    }
    
    void Update()
    {
        Debug.Log("XR 정보: " + UnityEngine.XR.XRSettings.loadedDeviceName);
        Debug.Log("렌더링 모드: " + UnityEngine.XR.XRSettings.stereoRenderingMode);
        Debug.Log("현재 해상도: " + UnityEngine.XR.XRSettings.eyeTextureResolutionScale);
    }
}
