using System;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region 필드 변수

    // 초기화 순서가 중요한 경우 사용
    // - Instance가 먼저 설정되지 않으면 다른 스크립트에서 접근할 때 NullReferenceException 발생 가능
    // - 하지만 필요할 경우 다른 인스턴스로 교체할 수도 있음
    public static GameManager Instance { get; private set; }

    [Header("참조")]
    [SerializeField] private CameraController cameraController;
    // 게임 머니(골드) 프로퍼티로 변경
    private int _gameMoney = 200;
    public int gameMoney 
    { 
        get => _gameMoney; 
        private set 
        { 
            if (_gameMoney != value) 
            { 
                _gameMoney = value; 
                EventManager.Instance.MoneyChangedEvent(_gameMoney); 
            } 
        } 
    }
    
    public bool gameStarted { get; private set; } = false;
    
    [Header("화살 쿨타임 관련")]
    private bool _isArrowCooldown = false; // 화살 쿨타임 상태
    private float _arrowCooldownTime = 1f; // 화살 쿨타임 시간 (초)
    private float _arrowCooldownStartTime; // 쿨타임 시작 시간
    private const float COOLDOWN_DECREASE_PER_LEVEL = 0.1f; // 레벨당 쿨타임 감소량
    
    /// <summary>
    /// 화살 쿨타임 여부 확인
    /// </summary>
    public bool IsArrowCooldown => _isArrowCooldown;

    #endregion

    #region 화살 쿨타임 관련

    /// <summary>
    /// 화살 쿨타임 시작
    /// </summary>
    public void StartArrowCooldown()
    {
        // 이미 쿨타임 중이면 무시
        if (_isArrowCooldown) return;

        _isArrowCooldown = true;
        _arrowCooldownStartTime = Time.time;

        // 쿨타임 시작 이벤트 발생
        EventManager.Instance.ArrowCooldownStartEvent();

        // 쿨타임 코루틴 시작
        StartCoroutine(ArrowCooldownCoroutine());
    }
    
    /// <summary>
    /// 화살 쿨타임 코루틴
    /// </summary>
    private IEnumerator ArrowCooldownCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < _arrowCooldownTime)
        {
            elapsedTime = Time.time - _arrowCooldownStartTime;
            
            yield return null;
        }
        
        // 쿨타임 종료
        _isArrowCooldown = false;
        
        // 쿨타임 종료 이벤트 발생
        EventManager.Instance.ArrowCooldownEndEvent();
    }

    /// <summary>
    /// 레벨업 시 쿨타임 감소
    /// </summary>
    private void HandleLevelUp(int newLevel)
    {
        // 레벨업마다 쿨타임 0.1초 감소 (최소 0.1초)
        _arrowCooldownTime = Mathf.Max(0.1f, 1f - (newLevel - 1) * COOLDOWN_DECREASE_PER_LEVEL);
    }
    
    #endregion

    #region 유니티 이벤트 함수

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 시작 시 컴포넌트 비활성화
        DisablePlayerControls();

        SoundManager.Instance.PlaySound("TitleBGM");
    }

    private void OnEnable()
    {
        EventManager.Instance.OnWaveStart += HandleWaveStart;
        EventManager.Instance.OnWaveEnd += HandleWaveEnd;
        EventManager.Instance.OnLevelUp += HandleLevelUp;
        
        // 다이얼로그매니저 이벤트 구독 추가
        // DialogueManager가 아직 초기화되지 않았을 수 있으므로 코루틴으로 시도
        StartCoroutine(TrySubscribeToDialogueManager());
    }

    private IEnumerator TrySubscribeToDialogueManager()
    {
        // DialogueManager가 초기화될 때까지 대기
        yield return new WaitForSeconds(2f);
        
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.conversationEnded += HandleConversationEnded;
        }
        else
        {
            yield return new WaitForSeconds(1f);
            
            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.conversationEnded += HandleConversationEnded;
            }
        }
    }

    private void OnDisable()
    {
        EventManager.Instance.OnWaveStart -= HandleWaveStart;
        EventManager.Instance.OnWaveEnd -= HandleWaveEnd;
        EventManager.Instance.OnLevelUp -= HandleLevelUp;
    }

    #endregion

    #region 게임 관리 메서드

    /// <summary>
    /// 게임 시작 처리 메서드 (다이얼로그 종료 후 호출)
    /// </summary>
    private void OnPostRender()
    {
        throw new NotImplementedException();
    }

    private void StartGame()
    {
        if (!gameStarted)
        {
            gameStarted = true;

            SoundManager.Instance.PlaySound("MainBGM");

            // 게임 시작 이벤트 발생 - UI 업데이트 등
            EventManager.Instance.GameStartEvent();

            // 이 시점에서는 카메라 컨트롤러 활성화하지 않음
            // 다이얼로그 종료 후 활성화됨
        }
    }
    
    /// <summary>
    /// 게임 종료 메서드
    /// </summary>
    public void EndGame(bool isVictory)
    {
        gameStarted = false;

        // 승리 시 다른 기능 추가
        EventManager.Instance.GameEndEvent(isVictory);
    }
    
    /// <summary>
    /// 게임 재시작 메서드 - 게임오버 UI의 재시작 버튼에 연결
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene("MainScene 1");
    }

    #endregion

    #region 돈 관련 메서드
    
    // 돈이 충분한지 여부 확인
    public bool HasEnoughMoney(int cost) => gameMoney >= cost;
    
    // 돈 차감
    public void DeductMoney(int cost)
    {
        gameMoney -= cost;
    }
    
    // 돈 추가 메서드 (웨이브 클리어 보상 등에 사용)
    public void AddMoney(int amount)
    {
        gameMoney += amount;
    }

    #endregion
    
    #region 웨이브 관련 핸들러

    /// <summary>
    /// 웨이브 시작 시 호출될 핸들러
    /// </summary>
    private void HandleWaveStart(int waveNumber)
    {
        if (gameStarted)
        {
            // 첫 번째 웨이브인 경우 조작법 설명 대화 시작
            if (waveNumber == 1)
            {
                // 잠시 지연 후 대화 시작 (UI가 제대로 설정될 수 있도록)
                StartCoroutine(StartTutorialDialogueDelayed());
            }
            else
            {
                // 첫 번째 웨이브가 아닌 경우 바로 플레이어 컨트롤 활성화
                EnablePlayerControls();
            }
        }
    }
    
    /// <summary>
    /// 지연 후 튜토리얼 대화 시작 코루틴
    /// </summary>
    private IEnumerator StartTutorialDialogueDelayed()
    {
        // 1초 대기
        yield return new WaitForSeconds(1f);
        
        // 다이얼로그 시작 이벤트 발생 (튜토리얼 타입)
        EventManager.Instance.DialogueStartedEvent(EventManager.DialogueType.Tutorial);
        
        // Tutorial 대화 시작 (ID는 DSU 에디터에서 설정한 값에 맞게 조정)
        DialogueManager.StartConversation("Tutorial");
        
        // 대화 중에는 플레이어 컨트롤 비활성화 상태 유지
        // 대화가 끝나면 HandleConversationEnded에서 플레이어 컨트롤 활성화됨
    }

    /// <summary>
    /// 웨이브 종료 시 호출될 핸들러
    /// </summary>
    private void HandleWaveEnd(int waveNumber)
    {
        // 웨이브 종료 시 플레이어 컨트롤 비활성화
        DisablePlayerControls();
        
        // 첫 번째 웨이브가 끝났을 때만 상점 튜토리얼 표시
        if (waveNumber == 1)
        {
            // 약간의 지연 후 상점 튜토리얼 표시
            StartCoroutine(StartShopTutorialDelayed());
        }
        
        if (waveNumber == 9)
        {
            // 9웨이브 종료 시 게임 종료
            // 승리 다이얼로그 표시
            StartCoroutine(StartVictoryDialogueDelayed());
        }
    }
    
    // 지연 후 상점 튜토리얼 시작 코루틴
    private IEnumerator StartShopTutorialDelayed()
    {
        // 상점 UI가 표시될 때까지 잠시 대기
        yield return new WaitForSeconds(0.5f);
    
        // 상점 튜토리얼 대화 시작
        DialogueManager.StartConversation("ShopTutorial");
    }
    
    private IEnumerator StartVictoryDialogueDelayed()
    {
        // 약간의 지연
        yield return new WaitForSeconds(1f);
    
        // Victory 대화 시작
        DialogueManager.StartConversation("Victory");
    }
    
    #endregion
    
    #region 헬퍼 메서드
    
    /// <summary>
    /// 플레이어 제어 컴포넌트 활성화
    /// </summary>
    private void EnablePlayerControls()
    {
        if (cameraController != null)
        {
            // CameraPosition을 CameraController를 통해 접근
            cameraController.SwitchCamera(CameraController.CameraPosition.Center);
        }
    }
    
    /// <summary>
    /// 플레이어 제어 컴포넌트 비활성화
    /// </summary>
    private void DisablePlayerControls()
    {
        if (cameraController != null)
        {
            // VR 환경에서는 카메라를 중앙 위치로 이동
            cameraController.SwitchCamera(CameraController.CameraPosition.Center);
        }
    }
    
    #endregion

    #region 대화 시스템 이벤트 핸들러
    
    /// <summary>
    /// 인트로 대화 시작 메서드 - 시작 버튼에 할당
    /// </summary>
    public void StartIntroDialogue()
    {
        // 다이얼로그 시작 이벤트 발생 (인트로 타입)
        EventManager.Instance.DialogueStartedEvent(EventManager.DialogueType.Intro);
        
        // Intro 대화 시작 (ID는 DSU 에디터에서 설정한 값에 맞게 조정)
        DialogueManager.StartConversation("Intro");
        
        // 참고: 게임 시작 처리는 대화 종료 후 HandleConversationEnded에서 처리됨
    }
    
    // 대화 종료 시 호출될 메서드
    private void HandleConversationEnded(Transform actor)
    {   
        // 튜토리얼 인트로 대화가 끝났을 때
        if (DialogueManager.lastConversationID == 1)
        {
            // 다이얼로그 종료 이벤트 발생 (인트로 타입)
            EventManager.Instance.DialogueEndedEvent(EventManager.DialogueType.Intro);
            
            // 비동기로 1초 후에 처리
            StartCoroutine(HandleConversationEndedDelayed());
        }
        // 조작법 튜토리얼 대화가 끝났을 때
        else if (DialogueManager.lastConversationID == 2)
        {
            // 다이얼로그 종료 이벤트 발생 (튜토리얼 타입)
            EventManager.Instance.DialogueEndedEvent(EventManager.DialogueType.Tutorial);
            
            // 지연 후 플레이어 컨트롤 활성화 (1초 후)
            StartCoroutine(DelayedEnablePlayerControls());
        }
        // Victory 대화가 끝났을 때
        else if (DialogueManager.lastConversationID == 4) // 승리 대화 ID
        {
            // 게임 클리어 처리
            EndGame(true);
        }
    }
    
    // 대화 종료 후 지연 처리를 위한 코루틴
    private IEnumerator HandleConversationEndedDelayed()
    {
        // 1초 대기
        yield return new WaitForSeconds(1f);
        
        // 게임오버 변수 확인
        bool isGameOver = DialogueLua.GetVariable("IsGameOver").asBool;
        
        if (isGameOver)
        {
            // 게임오버 처리
            EndGame(false);
        }
        else
        {
            // 게임 시작 처리
            StartGame();
        }
    }
    
    /// <summary>
    /// 지연 후 플레이어 컨트롤 활성화 코루틴
    /// </summary>
    private IEnumerator DelayedEnablePlayerControls()
    {
        // 1초 대기
        yield return new WaitForSeconds(1f);
        
        // 대화가 끝나면 플레이어 컨트롤 활성화
        EnablePlayerControls();
    }
    
    #endregion
}
