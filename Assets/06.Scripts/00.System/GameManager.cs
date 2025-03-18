using UnityEngine;
using System;
using PixelCrushers.DialogueSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region 필드 변수

    // 초기화 순서가 중요한 경우 사용
    // - Instance가 먼저 설정되지 않으면 다른 스크립트에서 접근할 때 NullReferenceException 발생 가능
    // - 하지만 필요할 경우 다른 인스턴스로 교체할 수도 있음
    public static GameManager Instance { get; private set; }

    [Header("참조")]
    [SerializeField] private ArrowShooter arrowShooter;
    [SerializeField] private PlayerLookController playerLookController;

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
        
        // 다이얼로그매니저 이벤트 구독 추가
        // DialogueManager가 아직 초기화되지 않았을 수 있으므로 코루틴으로 시도
        StartCoroutine(TrySubscribeToDialogueManager());
    }

    private System.Collections.IEnumerator TrySubscribeToDialogueManager()
    {
        // DialogueManager가 초기화될 때까지 대기
        yield return new WaitForSeconds(0.5f);
        
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
        
        // 다이얼로그매니저 이벤트 구독 해제
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.conversationEnded -= HandleConversationEnded;
        }
    }

    #endregion

    #region 게임 관리 메서드

    /// <summary>
    /// 게임 시작 버튼에 할당할 메서드
    /// </summary>
    public void StartGame()
    {
        if (!gameStarted)
        {
            gameStarted = true;

            SoundManager.Instance.PlaySound("MainBGM");

            EventManager.Instance.GameStartEvent();
        }
    }

    /// <summary>
    /// 게임 종료 메서드
    /// </summary>
    public void EndGame(bool isVictory)
    {
        gameStarted = false;

        // 게임 종료 이벤트 발생
        EventManager.Instance.GameEndEvent(isVictory);
    }
    
    /// <summary>
    /// 게임 재시작 메서드 - 게임오버 UI의 재시작 버튼에 연결
    /// </summary>
    public void RestartGame()
    {
        // 게임 상태 초기화
        gameStarted = false;
        
        // 돈 초기화
        _gameMoney = 200;
        EventManager.Instance.MoneyChangedEvent(_gameMoney);
        
        // 플레이어 컨트롤 비활성화
        DisablePlayerControls();
        
        // 타이틀 음악 재생
        SoundManager.Instance.PlaySound("TitleBGM");
        
        // 게임 재시작 이벤트 발생 (필요하다면 EventManager에 추가)
        // EventManager.Instance.GameRestartEvent();
        
        // 웨이브 매니저 초기화 등 필요한 다른 시스템 초기화
        // 여기에 추가 초기화 코드 작성
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
    private void HandleWaveStart(int _, int __)
    {
        if (gameStarted)
        {
            // 웨이브 시작 시 플레이어 컨트롤 활성화
            EnablePlayerControls();
        }
    }
    
    /// <summary>
    /// 웨이브 종료 시 호출될 핸들러
    /// </summary>
    private void HandleWaveEnd(int _)
    {
        // 웨이브 종료 시 플레이어 컨트롤 비활성화
        DisablePlayerControls();
    }
    
    #endregion
    
    #region 헬퍼 메서드
    
    /// <summary>
    /// 플레이어 제어 컴포넌트 활성화
    /// </summary>
    private void EnablePlayerControls()
    {
        arrowShooter.enabled = true;
        playerLookController.enabled = true;
    }
    
    /// <summary>
    /// 플레이어 제어 컴포넌트 비활성화
    /// </summary>
    private void DisablePlayerControls()
    {
        arrowShooter.enabled = false;
        playerLookController.enabled = false;
    }
    
    #endregion

    #region 대화 시스템 이벤트 핸들러
    
    // 대화 종료 시 호출될 메서드
    private void HandleConversationEnded(Transform actor)
    {
        Debug.Log("대화 종료됨: " + DialogueManager.lastConversationID);
        
        // 튜토리얼 대화가 끝났을 때
        if (DialogueManager.lastConversationID == 1)
        {
            // 비동기로 1초 후에 처리
            StartCoroutine(HandleConversationEndedDelayed());
        }
    }
    
    // 대화 종료 후 지연 처리를 위한 코루틴
    private System.Collections.IEnumerator HandleConversationEndedDelayed()
    {
        // 1초 대기
        yield return new WaitForSeconds(1f);
        
        // 게임오버 변수 확인
        bool isGameOver = DialogueLua.GetVariable("IsGameOver").asBool;
        
        if (isGameOver)
        {
            // 게임오버 처리
            Debug.Log("게임오버 선택지 선택됨");
            EndGame(false);
        }
        else
        {
            // 게임 시작 처리
            Debug.Log("게임 계속 진행 선택지 선택됨");
            StartGame();
        }
    }
    
    #endregion
}
