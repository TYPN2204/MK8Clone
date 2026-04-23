using DG.Tweening;
using MK8.Menu.UI;
using UnityEngine;

namespace MK8.Menu
{
    /// <summary>
    /// Top-level coordinator for the MK8Menu_Frontend scene.
    /// Manages scene-open fade-in and exposes navigation methods to each UIScreen.
    /// Section 6.2 of mk8_clone_prompt.md.
    /// </summary>
    public class MK8FrontendFlow : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private MK8UI_TitleScreen           _title;
        [SerializeField] private MK8UI_MainMenuScreen        _mainMenu;
        [SerializeField] private MK8UI_ModeSelectScreen      _modeSelect;
        [SerializeField] private MK8UI_SpeedSelectScreen     _speedSelect;
        [SerializeField] private MK8UI_CharacterSelectScreen _charSelect;
        [SerializeField] private MK8UI_KartPartsSelectScreen _kartPartsSelect;
        [SerializeField] private MK8UI_CupSelectScreen       _cupSelect;
        [SerializeField] private MK8UI_ConfirmModalScreen    _confirmModal;

        [Header("Scene Overlay")]
        [SerializeField] private CanvasGroup _whiteOverlay;

        [Header("Timing")]
        [SerializeField] private float _openFadeDuration = 0.8f;

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

            if (_title != null) MK8UIScreen.Focus(_title);
            else Debug.LogError("[MK8FrontendFlow] _title not assigned.");
        }

        // ── Public navigation API ─────────────────────────────────────────────────

        public void GoToMainMenu()
        {
            if (_mainMenu != null) MK8UIScreen.Focus(_mainMenu);
            else Debug.LogWarning("[MK8FrontendFlow] _mainMenu not assigned.");
        }

        public void GoToModeSelect()
        {
            if (_modeSelect != null) MK8UIScreen.Focus(_modeSelect);
            else Debug.LogWarning("[MK8FrontendFlow] _modeSelect not assigned.");
        }

        public void GoToSpeedSelect()
        {
            if (_speedSelect != null) MK8UIScreen.Focus(_speedSelect);
            else Debug.LogWarning("[MK8FrontendFlow] _speedSelect not assigned.");
        }

        public void GoToCharacterSelect()
        {
            if (_charSelect != null) MK8UIScreen.Focus(_charSelect);
            else Debug.LogWarning("[MK8FrontendFlow] _charSelect not assigned.");
        }

        public void GoToKartPartsSelect()
        {
            if (_kartPartsSelect != null) MK8UIScreen.Focus(_kartPartsSelect);
            else Debug.LogWarning("[MK8FrontendFlow] _kartPartsSelect not assigned.");
        }

        public void GoToCupSelect()
        {
            if (_cupSelect != null) MK8UIScreen.Focus(_cupSelect);
            else Debug.LogWarning("[MK8FrontendFlow] _cupSelect not assigned.");
        }

        public void GoToConfirmModal()
        {
            if (_confirmModal != null) MK8UIScreen.Focus(_confirmModal);
            else Debug.LogWarning("[MK8FrontendFlow] _confirmModal not assigned.");
        }
    }
}
