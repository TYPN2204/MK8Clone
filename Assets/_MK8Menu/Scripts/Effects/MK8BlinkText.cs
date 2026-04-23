using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MK8.Menu.Effects
{
    /// <summary>
    /// Reusable component that loops a TMP text colour between two values.
    /// Section 5.2: "Press Ⓐ to start" blinks black → #0066CC → black.
    /// </summary>
    public class MK8BlinkText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Color _colorA        = Color.black;
        [SerializeField] private Color _colorB        = new Color(0f, 0.4f, 0.8f, 1f); // #0066CC
        [SerializeField] private float _cycleDuration = 1.0f; // full black→blue→black

        private Tweener _tween;

        protected virtual void Reset()
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }

        // ── Public API ──────────────────────────────────────────────────────────

        public void StartBlink()
        {
            if (_text == null)
            {
                Debug.LogWarning("[MK8BlinkText] _text is null — assign TextMeshProUGUI in Inspector.");
                return;
            }

            StopBlink();
            _text.color = _colorA;
            // half-period per direction (InOutSine, Yoyo) → full cycle = _cycleDuration
            _tween = _text
                .DOColor(_colorB, _cycleDuration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void StopBlink()
        {
            _tween?.Kill();
            _tween = null;
            if (_text != null) _text.color = _colorA;
        }

        private void OnDisable() => StopBlink();
    }
}
