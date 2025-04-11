using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HUDManager : MonoBehaviour
{
    #region 필드 변수

    [Header("웨이브 UI")]
    public GameObject waveUIPanel;
    public TextMeshProUGUI currentWaveTMP;

    [Header("게임 상태 UI")]
    public GameObject gameOverUIPanel;
    public GameObject gameVictoryUIPanel;

    [Header("성문 UI")]
    [SerializeField] private Slider castleHealthSlider;

    [Header("카메라 UI")]
    [SerializeField] private GameObject CameraIndicatorUIPanel;
    [SerializeField] private GameObject leftArrow;  // 좌측 카메라 전환 화살표
    [SerializeField] private GameObject rightArrow; // 우측 카메라 전환 화살표

    [Header("화살 쿨타임 UI")]
    [SerializeField] private GameObject cooldownImageObject; // 쿨타임 이미지 오브젝트

    [Header("레벨업 UI")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private float levelUpPanelDuration = 3f;

    [Header("스폰 포인트 프리뷰")]
    public Camera spawnCamera;           // 스폰 포인트를 촬영할 카메라
    public Transform spawnPoint1;
    public Transform spawnPoint2;
    private bool isSpawnPointChange = false;
    public RawImage spawnDisplayImage;   // 캔버스에 있는 Raw Image 컴포넌트
    public RenderTexture renderTexture;  // 렌더 텍스처 에셋
    public GameObject spawnHudPanel;     // 스폰 디스플레이용 캔버스
    
    [Header("스폰 포인트 UI 컴포넌트")]
    public Image spawnBackgroundImage;  // 배경 이미지
    public TextMeshProUGUI spawnInfoText; // TMP 텍스트
    [SerializeField] private float spawnPreviewDuration = 2.0f;  // 표시 지속 시간
    [SerializeField] private float spawnFadeDuration = 1.0f;     // 페이드 아웃 시간
    
    private Coroutine spawnFadeCoroutine;  // 현재 실행 중인 페이드 코루틴

    #endregion

    #region 유니티 이벤트 함수

    private void Start()
    {
        IntitalizeUI();
        
        // 스폰 포인트 프리뷰 초기화
        InitializeSpawnPointPreview();

        // 카메라 화살표 초기 상태 (기본: 중앙 카메라 = 양쪽 화살표 모두 표시)
        UpdateCameraArrows(CameraController.CameraPosition.Center);
    }

    private void IntitalizeUI()
    {
        // 게임 종료 UI 초기화
        gameOverUIPanel.SetActive(false);
        gameVictoryUIPanel.SetActive(false);
        
        // 카메라 UI 초기화
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
        CameraIndicatorUIPanel.SetActive(false);
        
        // 웨이브 UI 초기화
        waveUIPanel.SetActive(false);
        
        // 화살 쿨타임 UI 초기화
        cooldownImageObject.SetActive(false);
    }

    private void InitializeSpawnPointPreview()
    {
        // 카메라가 렌더 텍스처에 촬영하도록 설정
        if (spawnCamera != null && renderTexture != null)
        {
            spawnCamera.targetTexture = renderTexture;
        }

        // Raw Image가 렌더 텍스처를 표시하도록 설정
        if (spawnDisplayImage != null && renderTexture != null)
        {
            spawnDisplayImage.texture = renderTexture;
        }

        // 처음에는 카메라와 캔버스 비활성화
        if (spawnCamera != null)
        {
            spawnCamera.gameObject.SetActive(false);
        }
        
        if (spawnHudPanel != null)
        {
            spawnHudPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // 이벤트 구독
        EventManager.Instance.OnGameEnd += HandleGameEndUI;
        EventManager.Instance.OnWaveStart += HandleWaveStartUI;
        EventManager.Instance.OnWaveEnd += HandleWaveEndUI;
        EventManager.Instance.OnCameraChanged += HandleCameraChanged;
        EventManager.Instance.OnArrowCooldownStart += HandleArrowCooldownStart;
        EventManager.Instance.OnArrowCooldownEnd += HandleArrowCooldownEnd;
        EventManager.Instance.OnCastleHealthChanged += UpdateCastleHealthUI;
        EventManager.Instance.OnCastleInitialized += InitializeCastleHealthUI;
        EventManager.Instance.OnLevelUp += HandleLevelUp;
        EventManager.Instance.OnDialogueStarted += HandleDialogueStarted;
        EventManager.Instance.OnDialogueEnded += HandleDialogueEnd;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        EventManager.Instance.OnGameEnd -= HandleGameEndUI;
        EventManager.Instance.OnWaveStart -= HandleWaveStartUI;
        EventManager.Instance.OnWaveEnd -= HandleWaveEndUI;
        EventManager.Instance.OnCameraChanged -= HandleCameraChanged;
        EventManager.Instance.OnArrowCooldownStart -= HandleArrowCooldownStart;
        EventManager.Instance.OnArrowCooldownEnd -= HandleArrowCooldownEnd;
        EventManager.Instance.OnCastleHealthChanged -= UpdateCastleHealthUI;
        EventManager.Instance.OnCastleInitialized -= InitializeCastleHealthUI;
        EventManager.Instance.OnLevelUp -= HandleLevelUp;
        EventManager.Instance.OnDialogueStarted -= HandleDialogueStarted;
        EventManager.Instance.OnDialogueEnded -= HandleDialogueEnd;
        
        // 코루틴이 실행 중이라면 중지
        if (spawnFadeCoroutine != null)
        {
            StopCoroutine(spawnFadeCoroutine);
            spawnFadeCoroutine = null;
        }
    }

    #endregion

    #region UI 업데이트 함수
    
    /// <summary>
    /// 게임플레이 UI (웨이브 UI와 카메라 인디케이터)를 함께 활성화/비활성화하는 메서드
    /// </summary>
    private void SetGameplayUIActive(bool isActive)
    {
        waveUIPanel.SetActive(isActive);
        CameraIndicatorUIPanel.SetActive(isActive);
    }

    // 게임 종료 시 UI 처리
    private void HandleGameEndUI(bool isVictory)
    {
        // 게임플레이 UI 비활성화
        SetGameplayUIActive(false);
        
        if (!isVictory)
        {
            gameOverUIPanel.SetActive(true);
        }
        else
        {
            gameVictoryUIPanel.SetActive(true);
        }
    }

    // 현재 웨이브 시작 시 작동되는 메서드
    private void HandleWaveStartUI(int waveNumber)
    {
        // UI 텍스트 갱신
        currentWaveTMP.text = $"Wave : {waveNumber}";
        
        // 게임플레이 UI 활성화
        SetGameplayUIActive(true);
    }

    // 현재 웨이브 종료 시 작동되는 메서드
    private void HandleWaveEndUI(int waveNumber)
    {
        // 게임플레이 UI 비활성화
        SetGameplayUIActive(false);
    }

    #endregion

    #region 레벨업 UI 함수

    /// <summary>
    /// 레벨업 UI 표시
    /// </summary>
    private void HandleLevelUp(int newLevel)
    {
        // 레벨업 UI 업데이트
        levelText.text = $"Level {newLevel}";
        damageText.text = $"Damage: {PlayerExperienceSystem.Instance.CurrentDamage:F1}";
        attackSpeedText.text = $"Attack Speed: {PlayerExperienceSystem.Instance.CurrentAttackSpeed:F1}";

        // 레벨업 UI 표시
        levelUpPanel.SetActive(true);
        
        // 애니메이션 효과
        levelUpPanel.transform.localScale = Vector3.zero;
        levelUpPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        // 일정 시간 후 자동으로 숨김
        StartCoroutine(HideLevelUpPanel());
    }

    /// <summary>
    /// 레벨업 UI 자동 숨김
    /// </summary>
    private IEnumerator HideLevelUpPanel()
    {
        yield return new WaitForSeconds(levelUpPanelDuration);
        
        // 애니메이션 효과
        levelUpPanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
            .OnComplete(() => levelUpPanel.SetActive(false));
    }

    #endregion

    #region 카메라 UI 함수

    /// <summary>
    /// 카메라 변경 이벤트 처리
    /// </summary>
    private void HandleCameraChanged(CameraController.CameraPosition position)
    {
        // 위치에 따라 화살표 UI 업데이트
        UpdateCameraArrows(position);
    }
    
    /// <summary>
    /// 카메라 위치에 따라 화살표 업데이트
    /// </summary>
    private void UpdateCameraArrows(CameraController.CameraPosition position)
    {
        if (leftArrow == null || rightArrow == null) return;
        
        switch (position)
        {
            case CameraController.CameraPosition.Left:
                // 왼쪽 카메라일 때는 오른쪽(중앙으로 이동) 화살표만 활성화
                leftArrow.SetActive(true);
                rightArrow.SetActive(false);
                break;
                
            case CameraController.CameraPosition.Center:
                // 중앙 카메라일 때는 양쪽 화살표 모두 활성화
                leftArrow.SetActive(true);
                rightArrow.SetActive(true);
                break;
                
            case CameraController.CameraPosition.Right:
                // 오른쪽 카메라일 때는 왼쪽(중앙으로 이동) 화살표만 활성화
                leftArrow.SetActive(false);
                rightArrow.SetActive(true);
                break;
        }
    }
    
    #endregion

    #region 화살 쿨타임 UI 함수
    
    /// <summary>
    /// 화살 쿨타임 시작 시 처리
    /// </summary>
    private void HandleArrowCooldownStart()
    {
        if (cooldownImageObject != null)
        {
            cooldownImageObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// 화살 쿨타임 종료 시 처리
    /// </summary>
    private void HandleArrowCooldownEnd()
    {
        if (cooldownImageObject != null)
        {
            cooldownImageObject.SetActive(false);
        }
    }
    
    #endregion

    #region 성문 체력 관리 UI 함수

    // 성문 체력 UI 초기화 메서드
    private void InitializeCastleHealthUI(Castle castle)
    {
        if (castleHealthSlider != null && castle != null)
        {
            castleHealthSlider.maxValue = castle.MaxHealth;
            castleHealthSlider.value = castle.currentHealth;
        }
    }
    
    // 성문 체력 UI 업데이트 메서드
    private void UpdateCastleHealthUI(float currentHealth)
    {
        if (castleHealthSlider != null)
        {
            castleHealthSlider.value = currentHealth;
        }
    }

    #endregion

    #region 스폰 포인트 UI 함수

    /// <summary>
    /// 다이얼로그 시작 이벤트 처리
    /// </summary>
    private void HandleDialogueStarted(EventManager.DialogueType e)
    {
        // 다이얼로그 시작 시 게임플레이 UI 비활성화
        SetGameplayUIActive(false);
    }

    /// <summary>
    /// 다이얼로그 종료 이벤트 처리
    /// </summary>
    private void HandleDialogueEnd(EventManager.DialogueType type)
    {
        if (type == EventManager.DialogueType.Tutorial)
        {
            ShowSpawnPreview();
            SetGameplayUIActive(true);
        }
        if (type == EventManager.DialogueType.SpawnPointAdded)
        {
            if (isSpawnPointChange)
            {
                spawnCamera.transform.SetPositionAndRotation(spawnPoint2.position, spawnPoint2.rotation);
                ShowSpawnPreview();
                SetGameplayUIActive(true);
            }
            else if (!isSpawnPointChange)
            {
                spawnCamera.transform.SetPositionAndRotation(spawnPoint1.position, spawnPoint1.rotation);
                ShowSpawnPreview();
                isSpawnPointChange = true;
                SetGameplayUIActive(true);
            }
        }
    }

    void ShowSpawnPreview()
    {
        // 기존에 실행 중인 페이드 코루틴 중지
        if (spawnFadeCoroutine != null)
        {
            StopCoroutine(spawnFadeCoroutine);
        }
        
        // 모든 UI 요소의 알파값을 1로 초기화
        SetSpawnUIAlpha(1.0f);
        
        // 카메라와 패널 활성화
        spawnCamera.gameObject.SetActive(true);
        spawnHudPanel.SetActive(true);
        
        // 페이드 아웃 코루틴 시작
        spawnFadeCoroutine = StartCoroutine(FadeOutSpawnPreview());
    }

    void HideSpawnPreview()
    {
        // 카메라와 패널 비활성화
        spawnCamera.gameObject.SetActive(false);
        spawnHudPanel.SetActive(false);
    }
    
    // UI 요소의 알파값 설정
    void SetSpawnUIAlpha(float alpha)
    {
        // 배경 이미지 알파값 설정
        if (spawnBackgroundImage != null)
        {
            Color color = spawnBackgroundImage.color;
            color.a = alpha;
            spawnBackgroundImage.color = color;
        }
        
        // 디스플레이 이미지 알파값 설정
        if (spawnDisplayImage != null)
        {
            Color color = spawnDisplayImage.color;
            color.a = alpha;
            spawnDisplayImage.color = color;
        }
        
        // 텍스트 알파값 설정
        if (spawnInfoText != null)
        {
            Color color = spawnInfoText.color;
            color.a = alpha;
            spawnInfoText.color = color;
        }
    }
    
    // 지정된 시간 후에 페이드 아웃하는 코루틴
    IEnumerator FadeOutSpawnPreview()
    {
        // 지정된 표시 시간만큼 대기
        yield return new WaitForSeconds(spawnPreviewDuration);
        
        // 페이드 아웃 시작 시간
        float startTime = Time.time;
        
        // 페이드 아웃 진행
        while (Time.time < startTime + spawnFadeDuration)
        {
            // 현재 진행률 계산 (0.0 ~ 1.0)
            float progress = (Time.time - startTime) / spawnFadeDuration;
            
            // 알파값 계산 (1.0 -> 0.0)
            float alpha = 1.0f - progress;
            
            // UI 요소 알파값 설정
            SetSpawnUIAlpha(alpha);
            
            yield return null; // 다음 프레임까지 대기
        }
        
        // 마지막으로 알파값을 0으로 설정
        SetSpawnUIAlpha(0.0f);
        
        // 패널과 카메라 비활성화
        HideSpawnPreview();
        
        // 코루틴 레퍼런스 초기화
        spawnFadeCoroutine = null;
    }
    
    #endregion
}