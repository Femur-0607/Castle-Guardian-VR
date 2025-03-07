using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region 필드 변수
    
    [Header("UI 참조")]
    public GameObject waveUIPanel;
    public GameObject shopUIPanel;
    public GameObject startUIPanel;
    public GameObject gameOverUIPanel;
    public TextMeshProUGUI currentWaveTMP;
    public Button startGameButton;
    
    #endregion

    #region 유니티 이벤트 함수

    private void Start()
    {
        waveUIPanel.SetActive(false);
        HideShopUI();
        gameOverUIPanel.SetActive(false);
        startUIPanel.SetActive(true);
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

    #endregion
}
