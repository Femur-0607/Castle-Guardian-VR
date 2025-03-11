using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region 필드 변수

    [Header("참조")]
    [SerializeField] private WaveManager waveManager;

    [Header("UI 참조")]
    public GameObject waveUIPanel;
    public GameObject shopUIPanel;
    public GameObject startUIPanel;
    public GameObject gameOverUIPanel;
    public Button startGameButton;
    public TextMeshProUGUI currentWaveTMP;
    public TextMeshProUGUI goldAmountTMP;

    #endregion

    #region 유니티 이벤트 함수

    private void Start()
    {
        waveUIPanel.SetActive(false);
        HideShopUI();
        gameOverUIPanel.SetActive(false);
        startUIPanel.SetActive(true);
        startGameButton.onClick.AddListener(GameManager.Instance.StartGame);

        // 초기 골드 설정
        if (goldAmountTMP != null)
        {
            UpdateGoldUI(GameManager.Instance.gameMoney);
        }
    }

    // 웨이브 이벤트 구독
    private void OnEnable()
    {
        EventManager.Instance.OnWaveStart += HandleWaveStartUI;
        EventManager.Instance.OnWaveEnd += HandleWaveEndUI;
        EventManager.Instance.OnGameStart += HandleGameStartUI;
        EventManager.Instance.OnGameEnd += HandleGameEndUI;
        EventManager.Instance.OnMoneyChanged += UpdateGoldUI;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnWaveStart -= HandleWaveStartUI;
        EventManager.Instance.OnWaveEnd -= HandleWaveEndUI;
        EventManager.Instance.OnGameStart -= HandleGameStartUI;
        EventManager.Instance.OnGameEnd -= HandleGameEndUI;
        EventManager.Instance.OnMoneyChanged -= UpdateGoldUI;
    }

    #endregion

    #region UI 업데이트 함수

    // 게임 시작 시 UI 처리
    private void HandleGameStartUI()
    {
        startUIPanel.SetActive(false);
        waveUIPanel.SetActive(true);
    }

    // 게임 종료 시 UI 처리
    private void HandleGameEndUI(bool isVictory) => waveUIPanel.SetActive(false);

    // 현재 웨이브 시작 시 작동되는 메서드
    private void HandleWaveStartUI(int wave, int _)
    {
        currentWaveTMP.text = $"Wave : {wave}"; // UI 텍스트 갱신
        HideShopUI();
    }

    // 현재 웨이브 종료 시 작동되는 메서드
    private void HandleWaveEndUI(int wave) => ShowShopUI();

    public void ShowShopUI() => shopUIPanel.SetActive(true);
    public void HideShopUI() => shopUIPanel.SetActive(false);

    // 골드 UI 업데이트 메서드
    private void UpdateGoldUI(int amount)
    {
        if (goldAmountTMP != null)
        {
            goldAmountTMP.text = $"{amount} G";
        }
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

    #endregion
}
