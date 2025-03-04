using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region 필드 변수
    
    public static UIManager Instance;
    public string currentWave;
    
    [Header("Shop UI Panel")]
    public GameObject shopUIPanel;  // 인스펙터에서 상점 UI 패널 연결

    
    #endregion

    #region 유니티 이벤트 함수

    // 싱글톤 패턴 사용
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    // 웨이브 이벤트 구독
    private void OnEnable()
    {
        EventManager.Instance.OnWaveStart += HandleWaveEndUI;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnWaveStart -= HandleWaveEndUI;
    }

    #endregion

    #region UI 업데이트 함수

    private void HandleWaveEndUI(int wave, int _unusedSomething)
    {
        currentWave = $"Wave : {wave}";
        ShowShopUI();
    }
    
    // 상점 UI 표시
    public void ShowShopUI()
    {
        if (shopUIPanel != null)
            shopUIPanel.SetActive(true);
    }

    // 상점 UI 숨김
    public void HideShopUI()
    {
        if (shopUIPanel != null)
            shopUIPanel.SetActive(false);
    }

    #endregion
}
