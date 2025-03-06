using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region 필드 변수
    
    // 초기화 순서가 중요한 경우 사용
    // - Instance가 먼저 설정되지 않으면 다른 스크립트에서 접근할 때 NullReferenceException 발생 가능
    // - 하지만 필요할 경우 다른 인스턴스로 교체할 수도 있음
    public static UIManager Instance { get; private set; }
    
    [Header("UI 참조")]
    public GameObject wavePanel;
    public GameObject shopUIPanel;
    public GameObject startUIPanel;
    public TextMeshProUGUI gameOverTMP;
    public TextMeshProUGUI currentWaveTMP;
    public Button startGameButton;
    
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
    }

    private void Start()
    {
        wavePanel.SetActive(false);
        HideShopUI();
        startUIPanel.SetActive(true);
        gameOverTMP.enabled = false;
        startGameButton.onClick.AddListener(GameManager.Instance.StartGame);
    }
    
    // 웨이브 이벤트 구독
    private void OnEnable()
    {
        EventManager.Instance.OnWaveStart +=  HandleWaveStartUI;
        EventManager.Instance.OnWaveEnd +=  HandleWaveEndUI;
        EventManager.Instance.OnGameStart += HandleGameStartUI;
        EventManager.Instance.OnGameEnd += HandleGameEndUI;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnWaveStart -= HandleWaveStartUI;
        EventManager.Instance.OnWaveEnd -= HandleWaveEndUI;
        EventManager.Instance.OnGameStart -= HandleGameStartUI;
        EventManager.Instance.OnGameEnd -= HandleGameEndUI;
    }

    #endregion

    #region UI 업데이트 함수
    
    // 게임 시작 시 UI 처리
    private void HandleGameStartUI()
    {
        startUIPanel.SetActive(false);
        wavePanel.SetActive(true);
    }
    
    // 게임 종료 시 UI 처리
    private void HandleGameEndUI(bool isVictory)
    {
        wavePanel.SetActive(false);
        shopUIPanel.SetActive(false);
        gameOverTMP.enabled = true;
    }
    
    // 현재 웨이브 시작 시 작동되는 메서드
    private void HandleWaveStartUI(int wave, int _)
    {
        currentWaveTMP.text = $"Wave : {wave}"; // UI 텍스트 갱신
        HideShopUI();
    }
 
    // 현재 웨이브 종료 시 작동되는 메서드
    private void HandleWaveEndUI(int wave)
    {
        
        ShowShopUI();
    }
    
    public void ShowShopUI() => shopUIPanel.SetActive(true);
    public void HideShopUI() => shopUIPanel.SetActive(false);

    #endregion
}
