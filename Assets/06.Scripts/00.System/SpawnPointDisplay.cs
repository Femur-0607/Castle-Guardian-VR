using UnityEngine;
using UnityEngine.UI;

public class SpawnPointDisplay : MonoBehaviour
{
    public Camera spawnCamera;           // 스폰 포인트를 촬영할 카메라
    public Transform spawnPoint1;
    public Transform spawnPoint2;
    public bool isSpawnPointChange = false;
    public RawImage displayImage;        // 캔버스에 있는 Raw Image 컴포넌트
    public RenderTexture renderTexture;  // 렌더 텍스처 에셋
    public GameObject hudPanel;         // 디스플레이용 캔버스

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

    private void OnEnable()
    {
        EventManager.Instance.OnDialogueStarted += HandleDialogueStarted;
        EventManager.Instance.OnDialogueEnded += HandleDialogueEnd;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnDialogueStarted -= HandleDialogueStarted;
        EventManager.Instance.OnDialogueEnded -= HandleDialogueEnd;
    }


    /// <summary>
    /// 웨이브 종료 시 호출될 핸들러
    /// </summary>
    private void HandleDialogueStarted(EventManager.DialogueType type)
    {
        if (type == EventManager.DialogueType.SpawnPointAdded)
        {
            if (isSpawnPointChange)
            {
                spawnCamera.transform.SetPositionAndRotation(spawnPoint2.position, spawnPoint2.rotation);
                ShowSpawnPreview();
            }
            else if (!isSpawnPointChange)
            {
                spawnCamera.transform.SetPositionAndRotation(spawnPoint1.position, spawnPoint1.rotation);
                ShowSpawnPreview();
                isSpawnPointChange = true;
            }
        }
    }

    private void HandleDialogueEnd(EventManager.DialogueType type)
    {
        HideSpawnPreview();
    }

    void ShowSpawnPreview()
    {
        // 카메라와 패널 활성화
        spawnCamera.gameObject.SetActive(true);
        hudPanel.SetActive(true);
    }

    void HideSpawnPreview()
    {
        // 카메라와 패널 활성화
        spawnCamera.gameObject.SetActive(false);
        hudPanel.SetActive(false);
    }
}