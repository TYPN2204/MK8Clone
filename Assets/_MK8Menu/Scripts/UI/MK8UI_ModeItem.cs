using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MK8.Menu.UI
{
    /// <summary>
    /// One entry in the Main Menu list. Handles selected/unselected visuals and kick animation.
    /// Section 5.3 — Bước 4.
    /// </summary>
    public class MK8UI_ModeItem : MonoBehaviour
    {
        // ── References ───────────────────────────────────────────────────────────
        [Header("Visuals")]
        [SerializeField] private Image           _bgNormal;    // white/transparent bg
        [SerializeField] private Image           _bgSelected;  // yellow highlight bg
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private CanvasGroup     _disabledOverlay; // semi-transparent dark overlay

        [Header("Config")]
        [SerializeField] private string _displayName = "Mode";
        [SerializeField] private bool   _isDisabled  = false;

        // ── Properties ───────────────────────────────────────────────────────────
        public bool          IsDisabled => _isDisabled;
        public RectTransform Rect       { get; private set; }
        /// <summary>Root CanvasGroup used by cascade animations (slide + fade).</summary>
        public CanvasGroup   Fader      { get; private set; }

        private void Awake()
        {
            Rect  = GetComponent<RectTransform>();
            Fader = GetComponent<CanvasGroup>();
            if (_label != null) _label.text = _displayName;
            ApplyDisabledVisual();
        }

        // ── Public API ───────────────────────────────────────────────────────────

        public void SetSelected(bool selected)
        {
            if (_bgNormal   != null) _bgNormal.gameObject.SetActive(!selected);
            if (_bgSelected != null) _bgSelected.gameObject.SetActive(selected);
        }

        /// <summary>Bounce kick on confirm — Section 5.3 "dokick".</summary>
        public void PlayKick()
        {
            // DOPunchScale: punch upward then settle
            transform.DOPunchScale(Vector3.one * 0.18f, 0.35f, vibrato: 5, elasticity: 0.5f);
        }

        // ── Private ──────────────────────────────────────────────────────────────

        private void ApplyDisabledVisual()
        {
            if (!_isDisabled) return;
            if (_label != null) _label.color = new Color(0.55f, 0.55f, 0.55f, 1f);
            if (_disabledOverlay != null) _disabledOverlay.alpha = 0.5f;
        }
    }
}
