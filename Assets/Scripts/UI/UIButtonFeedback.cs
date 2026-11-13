using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BatlIO.UI
{
    [RequireComponent(typeof(Selectable))]
    public class UIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float hoverScale = 1.04f;
        [SerializeField] private float pressScale = 0.96f;
        [SerializeField] private float transitionSpeed = 12f;

        private Vector3 initialScale;
        private Vector3 targetScale;
        private Selectable selectable;

        private void Awake()
        {
            initialScale = transform.localScale;
            targetScale = initialScale;
            selectable = GetComponent<Selectable>();
        }

        private void OnEnable()
        {
            transform.localScale = initialScale;
            targetScale = initialScale;
        }

        private void Update()
        {
            if (selectable != null && !selectable.interactable)
            {
                targetScale = initialScale;
            }

            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * transitionSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (selectable != null && !selectable.interactable) return;
            targetScale = initialScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = initialScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (selectable != null && !selectable.interactable) return;
            targetScale = initialScale * pressScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (selectable != null && !selectable.interactable) return;
            targetScale = initialScale * hoverScale;
        }
    }
}
