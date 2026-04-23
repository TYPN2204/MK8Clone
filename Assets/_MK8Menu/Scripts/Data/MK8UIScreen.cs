using System.Collections.Generic;
using UnityEngine;

namespace MK8.Menu.UI
{
    public class MK8UIScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        private static readonly Stack<MK8UIScreen> Stack = new();

        public static MK8UIScreen Active => Stack.Count > 0 ? Stack.Peek() : null;

        protected virtual void Reset()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public static void Focus(MK8UIScreen screen)
        {
            if (screen == null) return;

            if (Active != null)
            {
                Active.HideInternal();
            }

            Stack.Push(screen);
            screen.ShowInternal();
        }

        public static void Back()
        {
            if (Stack.Count <= 1) return;

            var current = Stack.Pop();
            current.HideInternal();

            var previous = Stack.Peek();
            previous.ShowInternal();
        }

        public static void BackTo(MK8UIScreen target)
        {
            if (target == null || Stack.Count == 0) return;

            while (Stack.Count > 1 && Stack.Peek() != target)
            {
                var popped = Stack.Pop();
                popped.HideInternal();
            }

            if (Stack.Peek() == target)
            {
                target.ShowInternal();
            }
        }

        public static void ClearStack()
        {
            while (Stack.Count > 0)
            {
                var screen = Stack.Pop();
                screen.HideInternal();
            }
        }

        private void ShowInternal()
        {
            if (_canvasGroup == null)
            {
                Debug.LogError($"MK8UIScreen ({name}): CanvasGroup is missing.");
                return;
            }

            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            OnShow();
        }

        private void HideInternal()
        {
            if (_canvasGroup == null) return;

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            OnHide();
            gameObject.SetActive(false);
        }

        public virtual void OnShow() { }
        public virtual void OnHide() { }
    }
}