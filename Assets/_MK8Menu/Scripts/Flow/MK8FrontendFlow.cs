using DG.Tweening;
using MK8.Menu.UI;
using UnityEngine;

namespace MK8.Menu
{
    /// <summary>
    /// Top-level coordinator for the MK8Menu_Frontend scene.
    /// Manages scene-open fade-in and exposes navigation methods to each UIScreen.
    ///
    /// Screens are registered here as SerializeFields and wired in Inspector / SceneBuilder.
    /// Section 6.2 of mk8_clone_prompt.md.
    /// </summary>
    public class MK8FrontendFlow : MonoBehaviour
    {
        // ── Screens (one added per step) ─────────────────────────────────────────
        [Header("Screens")]
        [SerializeField] private MK8UI_TitleScreen      _title;       // Bước 3
        [SerializeField] private MK8UI_MainMenuScreen   _mainMenu;    // Bước 4
        [SerializeField] private MK8UI_ModeSelectScreen _modeSelect;  // Bước 5
        // Bước 6: [SerializeField] private MK8UI_SpeedSelectScreen _speedSelect;
        // ...

        // ── Shared canvas overlay (white flash between screens) ──────────────────
        [Header("Scene Overlay")]
        [SerializeField] private CanvasGroup _whiteOverlay;

        [Header("Timing")]
        [SerializeField] private float _openFadeDuration = 0.8f;

        // ── Unity callbacks ──────────────────────────────────────────────────────

        private void Start()
        {
            MK8UIScreen.ClearStack();

            if (_whiteOverlay != null)
            {
                _whiteOverlay.alpha          = 1f;
                _whiteOverlay.interactable   = false;
                _whiteOverlay.blocksRaycasts = false;
                _whiteOverlay.DOFade(0f, _openFadeDuration).SetEase(Ease.InOutSine);
            }

            if (_title != null)
                MK8UIScreen.Focus(_title);
            else
                Debug.LogError("[MK8FrontendFlow] _title is not assigned.");
        }

        // ── Public navigation API ─────────────────────────────────────────────────

        public void GoToMainMenu()
        {
            if (_mainMenu != null)
                MK8UIScreen.Focus(_mainMenu);
            else
                Debug.LogWarning("[MK8FrontendFlow] _mainMenu not assigned.");
        }

        public void GoToModeSelect()
        {
            if (_modeSelect != null)
                MK8UIScreen.Focus(_modeSelect);
            else
                Debug.LogWarning("[MK8FrontendFlow] _modeSelect not assigned.");
        }

        public void GoToSpeedSelect()
        {
            // TODO Bước 6
            Debug.Log("[MK8FrontendFlow] GoToSpeedSelect — Bước 6.");
        }
    }
}
