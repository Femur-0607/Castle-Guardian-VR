using UnityEngine;
	 
public class VRDebug : MonoBehaviour
{
    public GameObject DebugUI;
    private bool UIActive;
	 
    private void Start()
    {
        DebugUI.SetActive(false);
        UIActive = false;
    }
	 
    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            UIActive = !UIActive;
            DebugUI.SetActive(UIActive);
        }
    }
}