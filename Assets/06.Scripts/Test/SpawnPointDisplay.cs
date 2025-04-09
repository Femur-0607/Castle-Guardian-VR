using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpawnPointDisplay : MonoBehaviour
{
    public Camera spawnCamera;           // 스폰 포인트를 촬영할 카메라
    public RawImage displayImage;        // 캔버스에 있는 Raw Image 컴포넌트
    public RenderTexture renderTexture;  // 렌더 텍스처 에셋
    public float displayDuration = 5f;   // 디스플레이 표시 시간
    public GameObject hudPanel;         // 디스플레이용 캔버스
    
    // Meta Quest 컨트롤러 입력 처리를 위한 변수
    private bool isDisplayActive = false;
    
    void Start()
    {
        // 카메라가 렌더 텍스처에 촬영하도록 설정
        spawnCamera.targetTexture = renderTexture;
        
        // Raw Image가 렌더 텍스처를 표시하도록 설정
        displayImage.texture = renderTexture;
        
        // 처음에는 카메라와 캔버스 비활성화
        spawnCamera.gameObject.SetActive(false);
        hudPanel.SetActive(false);
    }
    
    void Update()
    {
        // Meta Quest 컨트롤러의 X 버튼 입력 감지
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
        {
            ToggleDisplay();
        }
    }
    
    void ToggleDisplay()
    {
        // 현재 상태 반전
        isDisplayActive = !isDisplayActive;
        
        // 카메라와 캔버스 활성화/비활성화
        spawnCamera.gameObject.SetActive(isDisplayActive);
        hudPanel.SetActive(isDisplayActive);
        
        // 활성화된 경우 자동 비활성화 타이머 시작
        if (isDisplayActive)
        {
            StartCoroutine(HideDisplayAfterDelay());
        }
    }
    
    // 지연 후 숨기는 코루틴
    IEnumerator HideDisplayAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        
        // 타이머가 끝났을 때 아직 활성화 상태면 비활성화
        if (isDisplayActive)
        {
            isDisplayActive = false;
            spawnCamera.gameObject.SetActive(false);
            hudPanel.SetActive(false);
        }
    }
}