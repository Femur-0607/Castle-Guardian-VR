using UnityEngine;

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

    public int gameMoney = 200;
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
    }

    private void OnEnable()
    {
        EventManager.Instance.OnWaveStart += HandleWaveStart;
        EventManager.Instance.OnWaveEnd += HandleWaveEnd;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnWaveStart -= HandleWaveStart;
        EventManager.Instance.OnWaveEnd -= HandleWaveEnd;
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

    #endregion

    #region 돈 관련 메서드
    
    // 돈이 충분한지 여부 확인
    public bool HasEnoughMoney(int cost) => gameMoney >= cost;
    // 돈 차감
    public void DeductMoney(int cost) => gameMoney -= cost;

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
}
