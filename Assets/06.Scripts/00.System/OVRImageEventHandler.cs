using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VR.UI
{
    public class OVRImageEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private MultiImageHoverManager manager;
        private Image targetImage;
        private bool isPointerOver = false;

        public void Initialize(MultiImageHoverManager manager)
        {
            this.manager = manager;
            this.targetImage = GetComponent<Image>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isPointerOver) return;
            isPointerOver = true;

            // OVR 포인터 이벤트 처리
            if (manager != null && targetImage != null && 
                manager.imageToConfigMap.ContainsKey(targetImage))
            {
                manager.ApplyHoverEffect(manager.GetHoverableImageForTarget(targetImage));
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isPointerOver) return;
            isPointerOver = false;

            // OVR 포인터 이벤트 처리
            if (manager != null && targetImage != null && 
                manager.imageToConfigMap.ContainsKey(targetImage))
            {
                manager.RemoveHoverEffect(manager.GetHoverableImageForTarget(targetImage));
            }
        }
    }
} 