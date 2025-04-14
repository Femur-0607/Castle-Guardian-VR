using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace VR.UI
{
    public class MultiImageHoverManager : MonoBehaviour
    {
        #region 필드변수

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
        public Dictionary<Image, HoverableImage> imageToConfigMap = new Dictionary<Image, HoverableImage>();

        #endregion

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

                    // 이미지와 설정 매핑
                    imageToConfigMap[item.targetImage] = item;

                    // 이미지에 OVR 이벤트 핸들러 추가
                    AddOVREventHandlers(item.targetImage.gameObject);
                }
            }
        }

        private void AddOVREventHandlers(GameObject targetObject)
        {
            // 기존 이벤트 핸들러가 있는지 확인하고 추가
            OVRImageEventHandler eventHandler = targetObject.GetComponent<OVRImageEventHandler>();
            if (eventHandler == null)
            {
                eventHandler = targetObject.AddComponent<OVRImageEventHandler>();
                eventHandler.Initialize(this);
            }
        }

        public HoverableImage GetHoverableImageForTarget(Image targetImage)
        {
            if (imageToConfigMap.ContainsKey(targetImage))
                return imageToConfigMap[targetImage];
            return null;
        }

        #region 시각 효과 관련 함수

        public void ApplyHoverEffect(HoverableImage item)
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

        public void RemoveHoverEffect(HoverableImage item)
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
        
        // 모든 호버 효과 초기화 메서드
        public void ResetAllHoverEffects()
        {
            // 모든 활성 시퀀스 중지
            foreach (var sequence in activeSequences.Values)
            {
                if (sequence != null)
                    sequence.Kill();
            }
            
            // 딕셔너리 초기화
            activeSequences.Clear();
            
            // 모든 이미지를 원래 상태로 복원
            foreach (var item in hoverableImages)
            {
                if (item.targetImage != null)
                {
                    item.targetImage.transform.localScale = item.originalScale;
                    item.targetImage.color = item.originalColor;
                    item.targetImage.transform.rotation = item.originalRotation;
                }
            }
        }
        
        #endregion
    }
} 