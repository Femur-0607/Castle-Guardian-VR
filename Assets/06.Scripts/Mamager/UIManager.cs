using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region 필드 변수
    
    public static UIManager Instance;
    public TextMeshProUGUI currentWaveTMP;
    
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
        EventManager.Instance.OnWaveStart +=  HandleWaveStartUI;
        EventManager.Instance.OnWaveEnd +=  HandleWaveEndUI;
    }

    private void OnDisable()
    {
        EventManager.Instance.OnWaveStart -= HandleWaveStartUI;
        EventManager.Instance.OnWaveEnd -= HandleWaveEndUI;
    }

    #endregion

    #region UI 업데이트 함수
    
    // 현재 웨이브 시작 시 작동되는 메서드
    private void HandleWaveStartUI(int _, int __)
    {
        HideShopUI();
    }
 
    // 현재 웨이브 종료 시 작동되는 메서드
    private void HandleWaveEndUI(int wave)
    {
        currentWaveTMP.text = $"Wave : {wave}"; // UI 텍스트 갱신
        ShowShopUI();
    }
    
    /// <summary>
    /// 상점 UI 표시 
    /// </summary>
    public void ShowShopUI() => shopUIPanel.SetActive(true);
    
    /// <summary>
    /// 상점 UI 숨김
    /// </summary>
    public void HideShopUI() => shopUIPanel.SetActive(false);

    #endregion
}
