using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    #region 필드 변수

    [Header("참조")]
    [SerializeField] private WaveManager waveManager;
    
    [Header("웨이브 UI")]
    public GameObject waveUIPanel;
    public TextMeshProUGUI currentWaveTMP;

    [Header("타이틀 및 게임 관련 UI")]
    public GameObject startUIPanel;
    public GameObject gameOverUIPanel;
    public GameObject gameVictoryUIPanel;
    public TextMeshProUGUI goldAmountTMP;
    [SerializeField] private Button startButton;

    [Header("상점 UI")]
    public GameObject shopUIPanel;
    public Button[] tabButtons;
    private Image[] lineFocusImages;
    private TextMeshProUGUI[] tabTexts;
    public GameObject[] shopPanels;

    [Header("성문 UI")]
    [SerializeField] private Slider castleHealthSlider;

    [Header("카메라 UI")]
    [SerializeField] private GameObject CameraIndicatorUIPanel;
    [SerializeField] private GameObject leftArrow;  // 좌측 카메라 전환 화살표
    [SerializeField] private GameObject rightArrow; // 우측 카메라 전환 화살표

    [Header("다이얼로그 UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject[] dialogueImages; // 0, 1: 인트로용, 2: 튜토리얼용

    [Header("화살 쿨타임 UI")]
    [SerializeField] private GameObject cooldownImageObject; // 쿨타임 이미지 오브젝트
    
    
    [Header("레벨업 UI")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private float levelUpPanelDuration = 3f;
    
    [Header("화살 업그레이드 UI")]
    [SerializeField] private GameObject arrowUpgradePopup;
    [SerializeField] private TextMeshProUGUI beforeDamageText;
    [SerializeField] private TextMeshProUGUI afterDamageText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    
    [Header("사운드 UI")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI bgmValueText;
    [SerializeField] private TextMeshProUGUI sfxValueText;
    private static float bgmVolume = 1f;
    private static float sfxVolume = 1f;

    #endregion
    
    #region 유니티 이벤트 함수

    private void Start()
    {
        
        HideShopUI();
        gameOverUIPanel.SetActive(false);
        startUIPanel.SetActive(true);
        leftArrow.SetActive(false);
        rightArrow.SetActive(false);
        CameraIndicatorUIPanel.SetActive(false);
        waveUIPanel.SetActive(false);

        // 다이얼로그 UI 초기화
        HideAllDialogueImages();

        // 초기 골드 설정
        UpdateGoldUI(GameManager.Instance.gameMoney);

        // 시작 버튼에 대화 시작 메서드 연결
        startButton.onClick.AddListener(() => GameManager.Instance.StartIntroDialogue());

        // 상점 탭 초기화
        InitializeShopTabs();

        // 화살표 초기 상태 (기본: 중앙 카메라 = 양쪽 화살표 모두 표시)
        UpdateCameraArrows(CameraController.CameraPosition.Center);

        // 쿨타임 UI 초기화
        if (cooldownImageObject != null)
        {
            cooldownImageObject.SetActive(false);
        }
        
        // 사운드 UI 볼륨 초기화
        InitializeSound();
    }
    
    // 상점 탭 초기화 메서드 (기존 코드 분리)
    private void InitializeShopTabs()
    {
        lineFocusImages = new Image[tabButtons.Length];
        tabTexts = new TextMeshProUGUI[tabButtons.Length];

        for (int i = 0; i < tabButtons.Length; i++)
        {
            lineFocusImages[i] = tabButtons[i].transform.GetChild(0).GetComponent<Image>();
            tabTexts[i] = tabButtons[i].GetComponent<TextMeshProUGUI>();
        }
        // 탭 버튼 이벤트 설정
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int tabIndex = i; // 클로저 문제 방지
            tabButtons[i].onClick.AddListener(() => SwitchTab(tabIndex));
        }
        
        // 초기 탭 설정
        SwitchTab(0);
    }

    private void InitializeSound()
    {
        // 정적 변수에서 값 로드
        bgmSlider.value = bgmVolume;
        sfxSlider.value = sfxVolume;
        
        // 텍스트 초기화 (0-10 범위로 변환)
        UpdateVolumeText(bgmValueText, bgmVolume);
        UpdateVolumeText(sfxValueText, sfxVolume);
        
        // 사운드 매니저에 현재 값 적용
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
        
        // 슬라이더 이벤트 리스너 등록
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // 웨이브 이벤트 구독
    private void OnEnable()
    {
        EventManager.Instance.OnDialogueEnded += HandleDialogueEnded;
        EventManager.Instance.OnDialogueStarted += HandleDialogueStarted;
        EventManager.Instance.OnGameEnd += HandleGameEndUI;
        EventManager.Instance.OnWaveStart += HandleWaveStartUI;
        EventManager.Instance.OnWaveEnd += HandleWaveEndUI;
        EventManager.Instance.OnMoneyChanged += UpdateGoldUI;
        EventManager.Instance.OnCameraChanged += HandleCameraChanged;
        EventManager.Instance.OnArrowCooldownStart += HandleArrowCooldownStart;
        EventManager.Instance.OnArrowCooldownEnd += HandleArrowCooldownEnd;
        EventManager.Instance.OnCastleHealthChanged += UpdateCastleHealthUI;
        EventManager.Instance.OnCastleInitialized += InitializeCastleHealthUI;
        EventManager.Instance.OnLevelUp += HandleLevelUp;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
        EventManager.Instance.OnDialogueStarted -= HandleDialogueStarted;
        EventManager.Instance.OnGameEnd -= HandleGameEndUI;
        EventManager.Instance.OnWaveStart -= HandleWaveStartUI;
        EventManager.Instance.OnWaveEnd -= HandleWaveEndUI;
        EventManager.Instance.OnMoneyChanged -= UpdateGoldUI;
        EventManager.Instance.OnCameraChanged -= HandleCameraChanged;
        EventManager.Instance.OnArrowCooldownStart -= HandleArrowCooldownStart;
        EventManager.Instance.OnArrowCooldownEnd -= HandleArrowCooldownEnd;
        EventManager.Instance.OnCastleHealthChanged -= UpdateCastleHealthUI;
        EventManager.Instance.OnCastleInitialized -= InitializeCastleHealthUI;
        EventManager.Instance.OnLevelUp -= HandleLevelUp;
    }

    #endregion

    #region UI 업데이트 함수

    // 게임 종료 시 UI 처리
    private void HandleGameEndUI(bool isVictory)
    {
        if (!isVictory)
        {
            waveUIPanel.SetActive(false);
            gameOverUIPanel.SetActive(true);
        }
        else
        {
            waveUIPanel.SetActive(false);
            gameVictoryUIPanel.SetActive(true);
        }
    }

    // 현재 웨이브 시작 시 작동되는 메서드
    private void HandleWaveStartUI(int waveNumber)
    {
        currentWaveTMP.text = $"Wave : {waveNumber}"; // UI 텍스트 갱신
        HideShopUI();
        
        // 웨이브 1은 다이얼로그로 인해 DelayedGameStartUI에서 처리됨
        if (waveNumber > 1)
        {
            // 웨이브 시작 시 카메라 인디케이터 패널 켜기
            CameraIndicatorUIPanel.SetActive(true);
            waveUIPanel.SetActive(true);
        }
    }

    // 현재 웨이브 종료 시 작동되는 메서드
    private void HandleWaveEndUI(int waveNumber)
    {
        // 웨이브 종료 시 카메라 인디케이터 패널 끄기
        CameraIndicatorUIPanel.SetActive(false);
        
        // 상점 UI 표시
        ShowShopUI();
    }

    // 상점 UI 표시 메서드
    public void ShowShopUI() 
    {
        waveUIPanel.SetActive(false);
        shopUIPanel.SetActive(true);
        
        // 상점이 열릴 때마다 첫 번째 탭(Arrow)으로 초기화
        SwitchTab(0);
    }

    public void HideShopUI()
    {
        shopUIPanel.SetActive(false);
    }

    // 골드 UI 업데이트 메서드
    private void UpdateGoldUI(int amount)
    {
        goldAmountTMP.text = $"{amount} G";
    }

    /// <summary>
    /// 상점 나가기 버튼 클릭 시 작동
    /// 다음 웨이브 시작 신호 보내기
    /// </summary>
    public void StartNextWave()
    {
        // 상점 패널 먼저 닫기
        HideShopUI();

        // WaveManager에게 다음 웨이브 시작을 요청
        waveManager.StartNextWaveEvent();
    }
    
    /// <summary>
    /// 탭 전환 메서드
    /// </summary>
    /// <param name="tabIndex">선택한 탭 인덱스 (0: Arrow, 1: Tower, 2: Soldier)</param>
    private void SwitchTab(int tabIndex)
    {
        // 색상 정의
        Color selectedColor;
        Color unselectedColor;
        ColorUtility.TryParseHtmlString("#F6E19C", out selectedColor);
        ColorUtility.TryParseHtmlString("#BEB5B6", out unselectedColor);

        // 모든 탭 버튼 순회
        for (int i = 0; i < tabButtons.Length; i++)
        {
            lineFocusImages[i].gameObject.SetActive(i == tabIndex);
            tabTexts[i].color = (i == tabIndex) ? selectedColor : unselectedColor;
        }

        // 모든 상점 패널 비활성화
        foreach (var panel in shopPanels)
        {
            panel.SetActive(false);
        }
        
        // 선택한 탭에 해당하는 상점 패널만 활성화
        shopPanels[tabIndex].SetActive(true);
    }
    
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
    
    public void ShowArrowUpgradePopup()
    {
        // ArrowManager에서 현재 화살 데이터 가져오기
        ProjectileData normalArrow = ArrowManager.Instance.GetNormalArrowData();
    
        // 업그레이드 전후 데이터 계산
        float beforeDamage = normalArrow.baseDamage - normalArrow.damageIncreasePerLevel;
        float afterDamage = normalArrow.baseDamage;
        float multiplier = normalArrow.baseMultiplier;

        // UI 텍스트 업데이트
        beforeDamageText.text = $"이전 데미지: {beforeDamage:F1}";
        afterDamageText.text = $"현재 데미지: {afterDamage:F1}";
        multiplierText.text = $"데미지 증폭: {multiplier:F2}x";

        // 애니메이션과 함께 팝업 표시
        arrowUpgradePopup.transform.localScale = Vector3.zero;
        arrowUpgradePopup.SetActive(true);
        arrowUpgradePopup.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        // 일정 시간 후 자동 숨김
        StartCoroutine(HideArrowUpgradePopup());
    }

    private IEnumerator HideArrowUpgradePopup()
    {
        yield return new WaitForSeconds(1f);
    
        arrowUpgradePopup.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
            .OnComplete(() => arrowUpgradePopup.SetActive(false));
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

    #region 다이얼로그 UI 함수

    // 다이얼로그 시작 이벤트 처리
    private void HandleDialogueStarted(EventManager.DialogueType type)
    {
        // 다이얼로그 시작 시 타이틀 패널 닫기
        if (startUIPanel != null && startUIPanel.activeSelf)
        {
            startUIPanel.SetActive(false);
        }
        
        // 다이얼로그 타입에 따라 적절한 이미지 표시
        switch (type)
        {
            case EventManager.DialogueType.Intro:
                ShowIntroDialogueImages();
                break;
                
            case EventManager.DialogueType.Tutorial:
                ShowTutorialDialogueImages();
                break;
            
            case EventManager.DialogueType.SpawnPointAdded:
                ShowTutorialDialogueImages();
                break;
        }
    }

    // 다이얼로그 종료 이벤트 처리
    private void HandleDialogueEnded(EventManager.DialogueType type)
    {
        // 다이얼로그 이미지 숨기기
        HideAllDialogueImages();
        
        // 필요한 경우 추가 UI 처리
        if (type == EventManager.DialogueType.Intro)
        {
            // 코루틴을 통해 1초 지연 후 UI 업데이트
            StartCoroutine(DelayedGameStartUI());
        }
    }

    // 지연된 게임 시작 UI 처리 코루틴
    private IEnumerator DelayedGameStartUI()
    {
        // 1초 대기
        yield return new WaitForSeconds(1.0f);
        
        // UI 업데이트
        startUIPanel.SetActive(false);
        waveUIPanel.SetActive(true);
        CameraIndicatorUIPanel.SetActive(true);
    }

    // 모든 다이얼로그 이미지 숨기기
    private void HideAllDialogueImages()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        if (dialogueImages != null)
        {
            foreach (GameObject image in dialogueImages)
            {
                if (image != null)
                {
                    image.SetActive(false);
                }
            }
        }
    }

    // 인트로 다이얼로그 이미지 표시 (0번, 1번 이미지)
    private void ShowIntroDialogueImages()
    {
        // 모든 이미지 비활성화
        if (dialogueImages != null)
        {
            foreach (GameObject image in dialogueImages)
            {
                if (image != null)
                {
                    image.SetActive(false);
                }
            }
        }
        
        // 패널 활성화
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        // 0번, 1번 이미지만 활성화
        if (dialogueImages != null && dialogueImages.Length > 0)
        {
            if (dialogueImages[0] != null) dialogueImages[0].SetActive(true);
            if (dialogueImages.Length > 1 && dialogueImages[1] != null) dialogueImages[1].SetActive(true);
        }
    }

    // 튜토리얼 다이얼로그 이미지 표시 (2번 이미지)
    private void ShowTutorialDialogueImages()
    {
        // 모든 이미지 비활성화
        if (dialogueImages != null)
        {
            foreach (GameObject image in dialogueImages)
            {
                if (image != null)
                {
                    image.SetActive(false);
                }
            }
        }
        
        // 패널 활성화
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        // 2번 이미지만 활성화
        if (dialogueImages != null && dialogueImages.Length > 2 && dialogueImages[2] != null)
        {
            dialogueImages[2].SetActive(true);
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

    #region 사운드 UI 함수
    
    // 볼륨 텍스트 업데이트 (0-1 값을 0-10 정수로 변환)
    private void UpdateVolumeText(TextMeshProUGUI text, float volume)
    {
        if (text != null)
        {
            int displayValue = Mathf.RoundToInt(volume * 10); // 0-1 값을 0-10 정수로 변환
            text.text = displayValue.ToString();
        }
    }

    private void SetBGMVolume(float volume)
    {
        bgmVolume = volume; // 정적 변수에 저장
        SoundManager.Instance.SetBGMVolume(volume);
        UpdateVolumeText(bgmValueText, volume);
    }
    
    private void SetSFXVolume(float volume)
    {
        sfxVolume = volume; // 정적 변수에 저장
        SoundManager.Instance.SetSFXVolume(volume);
        UpdateVolumeText(sfxValueText, volume);
    }

    #endregion
}