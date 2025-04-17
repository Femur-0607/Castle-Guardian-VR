using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VRUIHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("호버 효과 설정")]
    public float hoverScale = 1.1f;
    public Color hoverColor = new Color(1f, 1f, 0.8f);

    private Vector3 originalScale;
    private Color originalColor;
    private Image targetImage;

    private void Start()
    {
        targetImage = GetComponent<Image>();
        if (targetImage != null)
        {
            originalScale = transform.localScale;
            originalColor = targetImage.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetImage == null) return;

        // 크기 변경
        transform.localScale = originalScale * hoverScale;
        // 색상 변경
        targetImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage == null) return;

        // 원래 크기로
        transform.localScale = originalScale;
        // 원래 색상으로
        targetImage.color = originalColor;
    }

    private void OnDisable()
    {
        // UI가 비활성화될 때 원래 상태로 복원
        if (targetImage != null)
        {
            transform.localScale = originalScale;
            targetImage.color = originalColor;
        }
    }
} 