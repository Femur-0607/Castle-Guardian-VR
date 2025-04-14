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
    [SerializeField] private MultiImageHoverManager hoverManager;

    [Header("타이틀 및 게임 관련 UI")]
    public GameObject startUIPanel;
    public TextMeshProUGUI goldAmountTMP;
    [SerializeField] private Button startButton;

    [Header("상점 UI")]
    public GameObject shopUIPanel;
    public Button[] tabButtons;
    private Image[] lineFocusImages;
    private TextMeshProUGUI[] tabTexts;
    public GameObject[] shopPanels;

    [Header("다이얼로그 UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject[] dialogueImages; // 0, 1: 인트로용, 2: 튜토리얼용

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
        startUIPanel.SetActive(true);

        // 다이얼로그 UI 초기화
        HideAllDialogueImages();

        // 초기 골드 설정
        UpdateGoldUI(GameManager.Instance.gameMoney);

        // 시작 버튼에 대화 시작 메서드 연결
        startButton.onClick.AddListener(() => GameManager.Instance.StartIntroDialogue());

        // 상점 탭 초기화
        InitializeShopTabs();

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

    // 이벤트 구독
    private void OnEnable()
    {
        EventManager.Instance.OnDialogueEnded += HandleDialogueEnded;
        EventManager.Instance.OnDialogueStarted += HandleDialogueStarted;
        EventManager.Instance.OnWaveEnd += HandleWaveEndUI;
        EventManager.Instance.OnMoneyChanged += UpdateGoldUI;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
        EventManager.Instance.OnDialogueStarted -= HandleDialogueStarted;
        EventManager.Instance.OnWaveEnd -= HandleWaveEndUI;
        EventManager.Instance.OnMoneyChanged -= UpdateGoldUI;
    }

    #endregion

    #region UI 업데이트 함수

    // 현재 웨이브 종료 시 작동되는 메서드
    private void HandleWaveEndUI(int waveNumber)
    {
        // 상점 UI 표시
        ShowShopUI();
    }

    // 상점 UI 표시 메서드
    public void ShowShopUI()
    {
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
        // 호버 효과 초기화
        if (hoverManager != null)
        {
            hoverManager.ResetAllHoverEffects();
        }

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

    public void ShowArrowUpgradePopup()
    {
        // ArrowManager에서 현재 화살 데이터 가져오기
        ProjectileData normalArrow = ArrowManager.Instance.GetNormalArrowData();

        / // 업그레이드 전후 데이터 계산 - 배수까지 적용된 damage 속성 사용
        float beforeDamage = normalArrow.damage / (1 + normalArrow.baseMultiplierIncreasePerLevel); // 이전 배수 계산
        float afterDamage = normalArrow.damage; // 현재 배수가 적용된 데미지
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