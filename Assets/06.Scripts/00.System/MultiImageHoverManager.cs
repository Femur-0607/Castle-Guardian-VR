using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class MultiImageHoverManager : MonoBehaviour
{
    [System.Serializable]
    public class HoverableImage
    {
        public Image targetImage;
        
        [Header("크기 효과")]
        public float scaleFactor = 1.1f;
        
        [Header("색상 효과")]
        public Color hoverColor = new Color(1f, 1f, 0.8f);
        
        [Header("애니메이션 설정")]
        public float animationDuration = 0.3f;
        public Ease animationEase = Ease.OutBack;
        
        [Header("회전 효과 (선택사항)")]
        public bool useRotationEffect = false;
        public float rotationAmount = 15f;
        public Ease rotationEase = Ease.OutQuad;
        
        [HideInInspector] public Vector3 originalScale;
        [HideInInspector] public Color originalColor;
        [HideInInspector] public Quaternion originalRotation;
    }
    
    public List<HoverableImage> hoverableImages = new List<HoverableImage>();
    
    [Header("공통 설정 (개별 설정이 없는 경우)")]
    public float defaultAnimationDuration = 0.3f;
    public Ease defaultAnimationEase = Ease.OutBack;
    
    private Dictionary<Image, Sequence> activeSequences = new Dictionary<Image, Sequence>();
    
    void Start()
    {
        // 각 이미지의 원래 상태 저장
        foreach (var item in hoverableImages)
        {
            if (item.targetImage != null)
            {
                item.originalScale = item.targetImage.transform.localScale;
                item.originalColor = item.targetImage.color;
                item.originalRotation = item.targetImage.transform.rotation;
                
                // 이미지에 이벤트 트리거 추가
                AddEventTrigger(item);
            }
        }
    }
    
    private void AddEventTrigger(HoverableImage item)
    {
        // 이미지 게임오브젝트에 EventTrigger 컴포넌트 추가/가져오기
        EventTrigger trigger = item.targetImage.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = item.targetImage.gameObject.AddComponent<EventTrigger>();
        
        // 목록이 비어있으면 생성
        if (trigger.triggers == null)
            trigger.triggers = new List<EventTrigger.Entry>();
        
        // 기존 트리거 제거 (중복 방지)
        trigger.triggers.RemoveAll(t => t.eventID == EventTriggerType.PointerEnter || t.eventID == EventTriggerType.PointerExit);
        
        // PointerEnter 이벤트 추가
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) => { OnPointerEnter(item); });
        trigger.triggers.Add(enterEntry);
        
        // PointerExit 이벤트 추가
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) => { OnPointerExit(item); });
        trigger.triggers.Add(exitEntry);
    }
    
    public void OnPointerEnter(HoverableImage item)
    {
        if (item.targetImage == null) return;
        
        // 기존 애니메이션 중단
        if (activeSequences.ContainsKey(item.targetImage) && activeSequences[item.targetImage] != null)
            activeSequences[item.targetImage].Kill();
        
        // 새 애니메이션 시퀀스 생성
        Sequence sequence = DOTween.Sequence();
        
        // 크기 애니메이션 추가
        sequence.Join(item.targetImage.transform.DOScale(
            item.originalScale * item.scaleFactor, 
            item.animationDuration
        ).SetEase(item.animationEase));
        
        // 색상 애니메이션 추가
        sequence.Join(item.targetImage.DOColor(
            item.hoverColor, 
            item.animationDuration
        ).SetEase(item.animationEase));
        
        // 회전 효과 (선택사항)
        if (item.useRotationEffect)
        {
            Vector3 currentRotation = item.targetImage.transform.rotation.eulerAngles;
            sequence.Join(item.targetImage.transform.DORotate(
                new Vector3(currentRotation.x, currentRotation.y, currentRotation.z + item.rotationAmount), 
                item.animationDuration)
                .SetEase(item.rotationEase));
        }
        
        // 시퀀스 저장
        activeSequences[item.targetImage] = sequence;
    }
    
    public void OnPointerExit(HoverableImage item)
    {
        if (item.targetImage == null) return;
        
        // 기존 애니메이션 중단
        if (activeSequences.ContainsKey(item.targetImage) && activeSequences[item.targetImage] != null)
            activeSequences[item.targetImage].Kill();
        
        // 새 애니메이션 시퀀스 생성
        Sequence sequence = DOTween.Sequence();
        
        // 원래 크기로 복원하는 애니메이션
        sequence.Join(item.targetImage.transform.DOScale(
            item.originalScale, 
            item.animationDuration
        ).SetEase(item.animationEase));
        
        // 원래 색상으로 복원하는 애니메이션
        sequence.Join(item.targetImage.DOColor(
            item.originalColor, 
            item.animationDuration
        ).SetEase(item.animationEase));
        
        // 회전 효과 복원 (선택사항)
        if (item.useRotationEffect)
        {
            sequence.Join(item.targetImage.transform.DORotate(
                item.originalRotation.eulerAngles,
                item.animationDuration)
                .SetEase(item.rotationEase));
        }
        
        // 시퀀스 저장
        activeSequences[item.targetImage] = sequence;
    }
}