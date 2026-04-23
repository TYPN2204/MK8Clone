using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MK8.Menu
{
    /// <summary>
    /// Boot sequence: black screen → logo fade-in → hold → white overlay fade-in → load Frontend.
    /// Section 5.1 of mk8_clone_prompt.md — Bước 2.
    /// </summary>
    public class MK8BootSequence : MonoBehaviour
    {
        // ── References (assign in Inspector) ────────────────────────────────────
        [Header("Canvas Groups")]
        [SerializeField] private CanvasGroup _logoGroup;      // CanvasGroup on LogoContainer GO
        [SerializeField] private CanvasGroup _whiteOverlay;   // CanvasGroup on WhiteOverlay GO

        // ── Timing ──────────────────────────────────────────────────────────────
        [Header("Timing (seconds)")]
        [SerializeField] private float _logoFadeInDuration  = 1.0f;  // alpha 0 → 1
        [SerializeField] private float _logoHoldDuration    = 2.5f;  // logo stays on screen
        [SerializeField] private float _whiteOutDuration    = 0.8f;  // white overlay 0 → 1

        private const string FrontendSceneName = "MK8Menu_Frontend";

        // ── Unity callbacks ─────────────────────────────────────────────────────

        private void Awake()
        {
            // Ensure starting state (alpha = 0 for both)
            if (_logoGroup    != null) _logoGroup.alpha    = 0f;
            if (_whiteOverlay != null) _whiteOverlay.alpha = 0f;
        }

        private IEnumerator Start()
        {
#if UNITY_EDITOR
            // Validate references early so errors surface immediately in Play mode
            if (_logoGroup == null || _whiteOverlay == null)
            {
                Debug.LogError(
                    "[MK8BootSequence] One or more SerializeField references are null.\n" +
                    "  • _logoGroup    → drag 'LogoContainer' CanvasGroup here\n" +
                    "  • _whiteOverlay → drag 'WhiteOverlay'  CanvasGroup here");
                yield break;
            }
#endif
            yield return PlaySequence();
        }

        // ── Private sequence ────────────────────────────────────────────────────

        private IEnumerator PlaySequence()
        {
            // 1. Fade logo in
            yield return _logoGroup
                .DOFade(1f, _logoFadeInDuration)
                .SetEase(Ease.InOutSine)
                .WaitForCompletion();

            // 2. Hold so player can see the logo
            yield return new WaitForSeconds(_logoHoldDuration);

            // 3. Fade white overlay in — covers entire screen
            _whiteOverlay.blocksRaycasts = true;
            yield return _whiteOverlay
                .DOFade(1f, _whiteOutDuration)
                .SetEase(Ease.InOutSine)
                .WaitForCompletion();

            // 4. Load Frontend scene (MK8Menu_Frontend must be in Build Settings index 1)
            SceneManager.LoadScene(FrontendSceneName);
        }
    }
}
