using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MK8.Menu.UI
{
    /// <summary>
    /// Arrow indicator that tracks the selected mode item (Y only, X stays fixed).
    /// Has a glow-loop on its second image layer.
    /// Section 5.3 — Bước 4.
    /// </summary>
    public class MK8UI_ArrowIndicator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _arrowRect;    // the arrow RectTransform to animate
        [SerializeField] private Image         _glowImage;    // second image layer (glow)
        [SerializeField] private CanvasGroup   _canvasGroup;  // for show/hide fade

        // Section 5.3: "glow 0.3↔1 với SetLoops(-1, Yoyo), 1s/cycle"
        [Header("Glow")]
        [SerializeField] private float _glowMin      = 0.3f;
        [SerializeField] private float _glowMax      = 1.0f;
        [SerializeField] private float _glowDuration = 1.0f;  // half-cycle (Yoyo)

        private Tween _glowTween;

        private void Awake()
        {
            if (_arrowRect == null) _arrowRect = GetComponent<RectTransform>();
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Moves the arrow to align vertically with <paramref name="target"/>.
        /// X position is kept fixed (arrow stays on its own side of the list).
        /// </summary>
        public void MoveTo(Vector3 targetWorldPos, float duration = 0.25f)
        {
            if (_arrowRect == null) return;
            // Keep our X, adopt target's Y
            var dest = new Vector3(_arrowRect.position.x, targetWorldPos.y, _arrowRect.position.z);
            _arrowRect.DOMove(dest, duration).SetEase(Ease.OutBack);
        }

        public void StartGlow()
        {
            _glowTween?.Kill();
            if (_glowImage == null) return;
            _glowImage.color = new Color(_glowImage.color.r, _glowImage.color.g,
                                          _glowImage.color.b, _glowMin);
            _glowTween = _glowImage
                .DOFade(_glowMax, _glowDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void StopGlow()
        {
            _glowTween?.Kill();
            _glowTween = null;
        }

        public void Show(float duration = 0.15f)
        {
            if (_canvasGroup != null) _canvasGroup.DOFade(1f, duration);
        }

        public void Hide(float duration = 0.2f)
        {
            if (_canvasGroup != null) _canvasGroup.DOFade(0f, duration);
        }

        private void OnDisable() => StopGlow();
    }
}
