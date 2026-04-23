using DG.Tweening;
using UnityEngine;

namespace MK8.Menu
{
    /// <summary>
    /// Minimal placeholder that receives the white overlay coming from MK8Menu_Boot
    /// and fades it out to reveal the Frontend scene content.
    ///
    /// NOTE: This script is a placeholder for Bước 2.
    ///       Step 3 will replace it with the full MK8FrontendFlow + MK8UI_TitleScreen.
    /// </summary>
    public class MK8FrontendInitializer : MonoBehaviour
    {
        // ── References (assign in Inspector) ────────────────────────────────────
        [Header("References")]
        [SerializeField] private CanvasGroup _whiteOverlay;   // starts at alpha = 1

        // ── Timing ──────────────────────────────────────────────────────────────
        [Header("Timing (seconds)")]
        [SerializeField] private float _fadeOutDuration = 0.8f;

        // ── Unity callbacks ─────────────────────────────────────────────────────

        private void Start()
        {
            if (_whiteOverlay == null)
            {
                Debug.LogError(
                    "[MK8FrontendInitializer] _whiteOverlay is not assigned. " +
                    "Drag the 'WhiteOverlay' CanvasGroup into the Inspector.");
                return;
            }

            // White overlay must start fully opaque (set in scene / Boot does this via DOFade 1)
            _whiteOverlay.alpha          = 1f;
            _whiteOverlay.interactable   = false;
            _whiteOverlay.blocksRaycasts = false;

            // Fade out to reveal the scene
            _whiteOverlay.DOFade(0f, _fadeOutDuration).SetEase(Ease.InOutSine);
        }
    }
}
