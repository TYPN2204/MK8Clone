using DG.Tweening;
using UnityEngine;

namespace MK8.Menu.Effects
{
    /// <summary>
    /// Expanding-circle ripple that plays at an anchored position on the Canvas.
    /// Section 5.2: "Spawn ripple visual tại vị trí Mario (sprite expanding circle, alpha fade)."
    ///
    /// Usage: call Play(anchoredPos) — the GO activates, animates, then deactivates itself.
    /// Replace the child Image with a circle sprite for final art.
    /// </summary>
    public class MK8WaterRippleEffect : MonoBehaviour
    {
        [SerializeField] private RectTransform _rippleRect;   // the circle image RectTransform
        [SerializeField] private CanvasGroup   _rippleGroup;  // for alpha fade-out

        [Header("Animation")]
        [SerializeField] private float _scaleTo   = 3.0f;    // end scale multiplier
        [SerializeField] private float _duration  = 0.6f;    // seconds

        private void Awake()
        {
            gameObject.SetActive(false); // starts hidden
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <param name="anchoredPosition">Canvas-space anchored position (same space as caller).</param>
        public void Play(Vector2 anchoredPosition)
        {
            if (_rippleRect == null || _rippleGroup == null)
            {
                Debug.LogWarning("[MK8WaterRippleEffect] Missing references — assign in Inspector.");
                return;
            }

            DOTween.Kill(_rippleRect);
            DOTween.Kill(_rippleGroup);

            _rippleRect.anchoredPosition = anchoredPosition;
            _rippleRect.localScale       = Vector3.one * 0.1f;
            _rippleGroup.alpha           = 1f;
            gameObject.SetActive(true);

            // Expand + fade simultaneously
            _rippleRect.DOScale(_scaleTo, _duration).SetEase(Ease.OutCubic);
            _rippleGroup
                .DOFade(0f, _duration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }
}
