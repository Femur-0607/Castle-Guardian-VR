using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Assets.SimpleSpinner
{
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
        private float previousHealth = -1; // 이전 체력값 (-1은 초기화되지 않은 상태)

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
        private Coroutine levelUpCoroutine;

        [Header("스폰 포인트 프리뷰")]
        public Camera spawnCamera;           // 스폰 포인트를 촬영할 카메라
        public Transform spawnPoint1;
        public Transform spawnPoint2;
        private bool isSpawnPointChange = false;
        public RawImage spawnDisplayImage;   // 캔버스에 있는 Raw Image 컴포넌트
        public RenderTexture renderTexture;  // 렌더 텍스처 에셋
        public GameObject spawnHudPanel;     // 스폰 디스플레이용 캔버스

        [Header("스폰 포인트 UI 컴포넌트")]
        public Image spawnBackgroundImage;    // 배경 이미지
        public Image leftSpawnAlarmImage;     // 왼쪽 스폰 알람 이미지
        public Image rightSpawnAlarmImage;    // 오른쪽 스폰 알람 이미지
        public TextMeshProUGUI spawnInfoText; // TMP 텍스트
        [SerializeField] private float spawnPreviewDuration = 2.0f;  // 표시 지속 시간
        [SerializeField] private float spawnFadeDuration = 1.0f;     // 페이드 아웃 시간
        private Coroutine spawnFadeCoroutine; // 현재 실행 중인 페이드 코루틴

        [Header("화살 선택 UI")]
        [SerializeField] private GameObject arrowSelectionDisplayPanel; // 화살 선택 패널
        [SerializeField] private TextMeshProUGUI arrowNameText;         // 화살 이름 텍스트
        [SerializeField] private Image spinnerImage;                    // 스피너 이미지 (원형 배경)
        [SerializeField] private SimpleSpinner spinner;                 // 스피너 컴포넌트
        [SerializeField] private float arrowDisplayDuration = 4f;       // 화살 선택 UI 표시 지속 시간
        private Coroutine arrowDisplayCoroutine;                        // 화살 선택 UI 표시 코루틴

        // 화살 타입별 색상 정의
        private readonly Color normalArrowColor = new Color(0.663f, 0.698f, 0.765f); // 은회색
        private readonly Color explosiveArrowColor = new Color(1f, 0.3f, 0.3f);   // 빨간색
        private readonly Color poisonArrowColor = new Color(0.3f, 1f, 0.3f);      // 초록색

        // 화살 타입별 텍스트 색상 정의
        private readonly Color normalTextColor = new Color(0.647f, 0.694f, 0.761f); // 은회색
        private readonly Color explosiveTextColor = new Color(1f, 0.8f, 0f);      // 노란색
        private readonly Color poisonTextColor = new Color(0.8f, 1f, 0.4f);       // 연두색

        // UI 페이드 효과 타입 열거형
        public enum UIFadeType
        {
            Scale,  // 크기 변화 (팝업)
            Fade    // 알파값 변화 (페이드)
        }

        [Header("보스 경고 UI")]
        [SerializeField] private GameObject bossWarningPanel;
        [SerializeField] private RectTransform topWarningStrip;
        [SerializeField] private RectTransform bottomWarningStrip;
        [SerializeField] private float warningStripMoveDuration = 2f;  // 경고 띠 이동 시간
        [SerializeField] private string warningSound = "BossWarning"; // 경고 사운드

        private Coroutine bossWarningCoroutine;
        
        [Header("데미지 경고 UI")]
        [SerializeField] private GameObject damageWarningPanel;
        [SerializeField] private float damageWarningDuration = 1.0f; // 경고 표시 시간
        [SerializeField] private float damageThreshold = 100f; // 데미지 임계값
        private float lastDamageAmount = 0f; // 마지막으로 받은 데미지
        private Coroutine damageWarningCoroutine;

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

            // 화살 선택 UI 초기화
            arrowSelectionDisplayPanel.SetActive(false);

            // 레벨업 UI 초기화
            levelUpPanel.SetActive(false);

            // 보스 UI 초기화
            bossWarningPanel.SetActive(false);
            
            // 데미지 경고 UI 초기화
            damageWarningPanel.SetActive(false);
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
            EventManager.Instance.OnArrowTypeChanged += HandleArrowTypeChanged;
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
            EventManager.Instance.OnArrowTypeChanged -= HandleArrowTypeChanged;
        }

        #endregion

        #region 통합 UI 패널 표시/숨김 함수

        /// <summary>
        /// UI 패널을 표시하고 지정된 시간 후에 자동으로 숨기는 메서드
        /// </summary>
        /// <param name="panel">표시할 UI 패널</param>
        /// <param name="duration">표시 지속 시간(초)</param>
        /// <param name="fadeType">페이드 효과 타입 (스케일, 알파 등)</param>
        /// <returns>실행 중인 코루틴</returns>
        private Coroutine ShowUIPanel(GameObject panel, float duration, UIFadeType fadeType = UIFadeType.Scale)
        {
            // 패널이 없으면 null 반환
            if (panel == null) return null;

            // 패널 활성화
            panel.SetActive(true);

            // 페이드 타입에 따른 시작 애니메이션
            switch (fadeType)
            {
                case UIFadeType.Scale:
                    // 스케일 0에서 1로 (팝업 효과)
                    panel.transform.localScale = Vector3.zero;
                    panel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
                    break;

                case UIFadeType.Fade:
                    // 알파값 0에서 1로 (페이드 인)
                    CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = panel.AddComponent<CanvasGroup>();
                    }
                    canvasGroup.alpha = 0f;
                    canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutQuad);
                    break;
            }

            // 지정된 시간 후에 사라지는 코루틴 시작
            return StartCoroutine(HideUIPanelAfterDelay(panel, duration, fadeType));
        }

        /// <summary>
        /// 지정된 시간 후에 UI 패널을 숨기는 코루틴
        /// </summary>
        private IEnumerator HideUIPanelAfterDelay(GameObject panel, float delay, UIFadeType fadeType)
        {
            // 지정된 시간만큼 대기
            yield return new WaitForSeconds(delay);

            // 페이드 타입에 따른 종료 애니메이션
            switch (fadeType)
            {
                case UIFadeType.Scale:
                    // 스케일 1에서 0으로 (축소 효과)
                    panel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                        .OnComplete(() => panel.SetActive(false));
                    break;

                case UIFadeType.Fade:
                    // 알파값 1에서 0으로 (페이드 아웃)
                    CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.InQuad)
                            .OnComplete(() => panel.SetActive(false));
                    }
                    else
                    {
                        panel.SetActive(false);
                    }
                    break;
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

            if (waveNumber == 10)
            {
                ShowBossWarningUI();
            }
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

            // 통합된 UI 패널 표시 메서드 사용
            levelUpCoroutine = ShowUIPanel(levelUpPanel, levelUpPanelDuration, UIFadeType.Scale);
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
                
                // 초기 체력값 저장
                previousHealth = castle.currentHealth;
            }
        }

        // 성문 체력 UI 업데이트 메서드
        private void UpdateCastleHealthUI(float currentHealth)
        {
            if (castleHealthSlider != null)
            {
                castleHealthSlider.value = currentHealth;
                
                // 이전 체력값이 초기화되었고, 체력이 감소했을 때만 데미지 처리
                if (previousHealth >= 0 && currentHealth < previousHealth)
                {
                    float damageTaken = previousHealth - currentHealth;
            
                    // 일정 이상의 데미지를 받았을 때만 경고 표시
                    if (damageTaken >= damageThreshold)
                    {
                        ShowDamageWarning(damageTaken);
                    }
                }
        
                // 현재 체력값 저장
                previousHealth = currentHealth;
            }
        }
        
        // 데미지 경고 UI 표시
        private void ShowDamageWarning(float damageAmount)
        {
            // 이미 실행 중인 코루틴이 있으면 중지
            if (damageWarningCoroutine != null)
            {
                StopCoroutine(damageWarningCoroutine);
            }
    
            // 데미지 정보 저장
            lastDamageAmount = damageAmount;
    
            // 통합 UI 패널 표시 메서드 사용 (페이드 효과)
            damageWarningCoroutine = ShowUIPanel(damageWarningPanel, damageWarningDuration, UIFadeType.Fade);
        }

        #endregion

        #region 스폰 포인트 UI 함수
        
        // 알람 상태를 관리할 enum 추가
        public enum SpawnAlarmState
        {
            None,       // 알람 없음 (둘 다 비활성화)
            LeftOnly,   // 왼쪽 알람만 활성화
            RightOnly   // 오른쪽 알람만 활성화
        }

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
                // 튜토리얼 종료 후에는 알람 없이 표시
                ShowSpawnPreview(SpawnAlarmState.None);
                SetGameplayUIActive(true);
            }
            else if (type == EventManager.DialogueType.SpawnPointAdded)
            {
                if (isSpawnPointChange)
                {
                    // 두 번째 스폰 포인트 추가 시 오른쪽 알람만 활성화
                    spawnCamera.transform.SetPositionAndRotation(spawnPoint2.position, spawnPoint2.rotation);
                    ShowSpawnPreview(SpawnAlarmState.RightOnly);
                    SetGameplayUIActive(true);
                }
                else if (!isSpawnPointChange)
                {
                    // 첫 번째 스폰 포인트 추가 시 왼쪽 알람만 활성화
                    spawnCamera.transform.SetPositionAndRotation(spawnPoint1.position, spawnPoint1.rotation);
                    ShowSpawnPreview(SpawnAlarmState.LeftOnly);
                    isSpawnPointChange = true;
                    SetGameplayUIActive(true);
                }
            }
        }

        /// <summary>
        /// 스폰 포인트 프리뷰 표시
        /// </summary>
        void ShowSpawnPreview(SpawnAlarmState alarmState = SpawnAlarmState.None)
        {
            // 모든 UI 요소의 알파값을 1로 초기화
            SetSpawnUIAlpha(1.0f);
    
            // 알람 상태에 따라 이미지 활성화 설정
            if (leftSpawnAlarmImage != null)
                leftSpawnAlarmImage.gameObject.SetActive(alarmState == SpawnAlarmState.LeftOnly);

            if (rightSpawnAlarmImage != null)
                rightSpawnAlarmImage.gameObject.SetActive(alarmState == SpawnAlarmState.RightOnly);

            // 카메라와 패널 활성화
            spawnCamera.gameObject.SetActive(true);
            spawnHudPanel.SetActive(true);

            // 스폰 패널은 특별한 처리가 필요하므로 별도 코루틴 사용
            // (캔버스가 아닌 RenderTexture를 사용하기 때문)
            spawnFadeCoroutine = StartCoroutine(FadeOutSpawnPreview());
        }

        /// <summary>
        /// 스폰 포인트 프리뷰 숨김
        /// </summary>
        void HideSpawnPreview()
        {
            // 카메라와 패널 비활성화
            spawnCamera.gameObject.SetActive(false);
            spawnHudPanel.SetActive(false);
        }

        /// <summary>
        /// UI 요소의 알파값 설정
        /// </summary>
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
            
            // 왼쪽 알람 이미지 알파값 설정
            if (leftSpawnAlarmImage != null && leftSpawnAlarmImage.gameObject.activeSelf)
            {
                Color color = leftSpawnAlarmImage.color;
                color.a = alpha;
                leftSpawnAlarmImage.color = color;
            }

            // 오른쪽 알람 이미지 알파값 설정
            if (rightSpawnAlarmImage != null && rightSpawnAlarmImage.gameObject.activeSelf)
            {
                Color color = rightSpawnAlarmImage.color;
                color.a = alpha;
                rightSpawnAlarmImage.color = color;
            }

            // 텍스트 알파값 설정
            if (spawnInfoText != null)
            {
                Color color = spawnInfoText.color;
                color.a = alpha;
                spawnInfoText.color = color;
            }
        }

        /// <summary>
        /// 지정된 시간 후에 페이드 아웃하는 코루틴
        /// </summary>
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

        #region 화살 선택 UI 함수

        /// <summary>
        /// 화살 타입 변경 이벤트 처리
        /// </summary>
        private void HandleArrowTypeChanged(ProjectileData.ProjectileType newType)
        {
            UpdateArrowSelectionUI(newType);
        }

        /// <summary>
        /// 화살 선택 UI 업데이트
        /// </summary>
        private void UpdateArrowSelectionUI(ProjectileData.ProjectileType arrowType)
        {
            // 화살 타입에 따라 UI 설정
            switch (arrowType)
            {
                case ProjectileData.ProjectileType.Normal:
                    SetArrowUI("Normal Arrow", normalArrowColor, normalTextColor, 1.0f, false, arrowType);
                    break;

                case ProjectileData.ProjectileType.Explosive:
                    SetArrowUI("Explosive Arrow", explosiveArrowColor, explosiveTextColor, 1.8f, true, arrowType);
                    break;

                case ProjectileData.ProjectileType.Poison:
                    SetArrowUI("Poison Arrow", poisonArrowColor, poisonTextColor, 0.7f, true, arrowType);
                    break;
            }

            // 화살 선택 UI 표시 (통합 UI 패널 표시 메서드 사용)
            arrowDisplayCoroutine = ShowUIPanel(arrowSelectionDisplayPanel, arrowDisplayDuration, UIFadeType.Scale);
        }

        /// <summary>
        /// 화살 UI 설정
        /// </summary>
        private void SetArrowUI(string arrowName, Color spinnerColor, Color textColor, float rotationSpeed, bool enableRainbow, ProjectileData.ProjectileType arrowType = ProjectileData.ProjectileType.Normal)
        {
            // 텍스트 설정
            if (arrowNameText != null)
            {
                arrowNameText.text = arrowName;
                arrowNameText.color = textColor;

                // 글로우 효과 설정 (TextMeshPro 기능 사용)
                if (arrowNameText.fontSharedMaterial != null)
                {
                    arrowNameText.fontSharedMaterial.SetColor("_GlowColor", spinnerColor);
                    arrowNameText.fontSharedMaterial.SetFloat("_GlowPower", 0.5f);
                    arrowNameText.fontSharedMaterial.SetFloat("_GlowOuter", 0.05f);
                }
            }

            // 스피너 이미지 색상 설정
            if (spinnerImage != null)
            {
                spinnerImage.color = spinnerColor;
            }

            // 스피너 설정
            if (spinner != null)
            {
                spinner.Rotation = true;
                spinner.RotationSpeed = rotationSpeed;
                spinner.Rainbow = enableRainbow;

                if (enableRainbow)
                {
                    // 화살 타입에 따라 무지개 색상 설정
                    if (arrowType == ProjectileData.ProjectileType.Explosive)
                    {
                        // 폭발 화살은 빨간색-노란색 계열로 제한
                        spinner.RainbowSpeed = 0.5f;
                        spinner.RainbowSaturation = 0.8f;
                    }
                    else if (arrowType == ProjectileData.ProjectileType.Poison)
                    {
                        // 독 화살은 초록색-청록색 계열로 제한
                        spinner.RainbowSpeed = 0.3f;
                        spinner.RainbowSaturation = 0.7f;
                    }
                }
            }
        }

        #endregion

        #region 보스 UI 함수

        // 보스 경고 UI 표시 메서드
        public void ShowBossWarningUI()
        {
            // 카메라 전환 및 페이드 효과가 완료될 때까지 대기하는 코루틴 시작
            StartCoroutine(DelayedBossWarning());
            
        }

        private IEnumerator DelayedBossWarning()
        {
            // OVRScreenFade 효과가 완료될 때까지 충분한 지연 시간 부여
            // fadeTime의 2배 정도로 설정하면 페이드 아웃-인이 모두 끝난 후 경고창이 표시됨
            yield return new WaitForSeconds(1.0f);
            
            // 경고 패널 활성화
            if (bossWarningPanel != null)
            {
                bossWarningPanel.SetActive(true);
            }

            // 경고 애니메이션 코루틴 시작
            bossWarningCoroutine = StartCoroutine(PlayBossWarningAnimation());
        }

        // 보스 경고 애니메이션 코루틴
        private IEnumerator PlayBossWarningAnimation()
        {
            // 경고 사운드 재생
            SoundManager.Instance.PlaySound(warningSound);

            // 1. 경고 스트립 이동 애니메이션
            float elapsed = 0f;

            while (elapsed < warningStripMoveDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / warningStripMoveDuration;

                // 상단과 하단 스트립 이동
                if (topWarningStrip != null)
                {
                    Vector2 pos = topWarningStrip.anchoredPosition;
                    pos.x = Mathf.Lerp(-Screen.width, 0, t);
                    topWarningStrip.anchoredPosition = pos;
                }

                if (bottomWarningStrip != null)
                {
                    Vector2 pos = bottomWarningStrip.anchoredPosition;
                    pos.x = Mathf.Lerp(Screen.width, 0, t);
                    bottomWarningStrip.anchoredPosition = pos;
                }

                yield return null;
            }

            // 2. 경고 표시 지속 (1초 정도)
            yield return new WaitForSeconds(1f);

            // 3. 페이드 아웃 (기존 ShowUIPanel 메서드를 활용)
            bossWarningPanel.SetActive(false);

            // 코루틴 레퍼런스 초기화
            bossWarningCoroutine = null;
        }
    }
    
    #endregion
}