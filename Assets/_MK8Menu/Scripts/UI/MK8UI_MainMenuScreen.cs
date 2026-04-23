using DG.Tweening;
using UnityEngine;

namespace MK8.Menu.UI
{
    /// <summary>
    /// Main Menu: 6 mode items on a blue left panel, character scene on right, bottom bar.
    /// Input: Up/Down to navigate, A to confirm (Single Player only active now),
    ///        B to go back (no previous screen from here so ignored).
    /// Section 5.3 — Bước 4.
    /// </summary>
    public class MK8UI_MainMenuScreen : MK8UIScreen
    {
        // ── Mode Items ───────────────────────────────────────────────────────────
        [Header("Mode Items (0=SinglePlayer … 5=Shop)")]
        [SerializeField] private MK8UI_ModeItem[] _modeItems;   // exactly 6

        // ── Arrow Indicators ─────────────────────────────────────────────────────
        [Header("Arrow Indicators")]
        [SerializeField] private MK8UI_ArrowIndicator _arrowLeft;
        [SerializeField] private MK8UI_ArrowIndicator _arrowRight;

        // ── Character Scenes (one CanvasGroup per mode) ──────────────────────────
        [Header("Character Scenes (one per mode, 0–5)")]
        [SerializeField] private CanvasGroup[] _charScenes;

        // ── Transition overlay ────────────────────────────────────────────────────
        [Header("Scene Transition Overlay")]
        [SerializeField] private CanvasGroup _sceneOverlay;  // shared white overlay on canvas

        // ── Input ─────────────────────────────────────────────────────────────────
        [Header("Input")]
        [SerializeField] private MK8InputReader _inputReader;

        // ── Navigation ────────────────────────────────────────────────────────────
        [Header("Navigation")]
        /// <summary>ModeSelectScreen — wired in Bước 5.</summary>
        [SerializeField] private MK8UIScreen _nextScreen;

        // ── State ─────────────────────────────────────────────────────────────────
        private int   _selectedIndex;
        private bool  _acceptInput;
        private float _navCooldown;
        private const float NavDelay = 0.15f; // seconds between nav ticks

        // ── MK8UIScreen overrides ─────────────────────────────────────────────────

        public override void OnShow()
        {
            _acceptInput   = false;
            _navCooldown   = 0f;
            _selectedIndex = 0;

            // Ensure overlay is transparent
            if (_sceneOverlay != null)
            {
                _sceneOverlay.alpha          = 0f;
                _sceneOverlay.interactable   = false;
                _sceneOverlay.blocksRaycasts = false;
            }

            // Set all mode items to unselected, then select index 0
            for (int i = 0; i < _modeItems.Length; i++)
                _modeItems[i]?.SetSelected(i == 0);

            // Show char scene 0, hide others
            InitCharScenes(0);

            // Place arrows instantly at item 0, then start glow
            SnapArrowsToSelected();
            _arrowLeft?.Show(0f);
            _arrowRight?.Show(0f);
            _arrowLeft?.StartGlow();
            _arrowRight?.StartGlow();

            // Accept input after a short delay (let OnShow fade-in complete)
            DOVirtual.DelayedCall(0.3f, () => _acceptInput = true);
        }

        public override void OnHide()
        {
            _acceptInput = false;
            DOTween.Kill(gameObject);
            _arrowLeft?.StopGlow();
            _arrowRight?.StopGlow();
        }

        private void Update()
        {
            if (!_acceptInput || _inputReader == null) return;

            _navCooldown -= Time.deltaTime;

            // Navigate Up / Down
            float navY = _inputReader.NavigateValue.y;
            if (_navCooldown <= 0f)
            {
                if (navY >  0.5f) { Navigate(-1); _navCooldown = NavDelay; }
                else if (navY < -0.5f) { Navigate( 1); _navCooldown = NavDelay; }
            }

            if (_inputReader.ConfirmPressedThisFrame) OnConfirm();
            // Back from main menu → no-op (nowhere to go back to)
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void Navigate(int direction)
        {
            int count    = _modeItems.Length;
            if (count == 0) return;

            int newIndex = (_selectedIndex + direction + count) % count;
            if (newIndex == _selectedIndex) return;

            _modeItems[_selectedIndex]?.SetSelected(false);
            int prev       = _selectedIndex;
            _selectedIndex = newIndex;
            _modeItems[_selectedIndex]?.SetSelected(true);

            // Animate arrows to new position (Y only, Section 5.3 ease OutBack 0.25s)
            AnimateArrowsToSelected();
            // Crossfade character scene
            CrossfadeCharScene(prev, _selectedIndex);
        }

        private void OnConfirm()
        {
            var item = GetSelectedItem();
            if (item == null || item.IsDisabled) return;

            _acceptInput = false;

            // 1. Arrows hide (fade)
            _arrowLeft?.Hide();
            _arrowRight?.Hide();

            // 2. Kick animation
            item.PlayKick();

            // 3. White overlay fade-in → switch screen → fade-out
            if (_sceneOverlay != null)
            {
                _sceneOverlay.blocksRaycasts = true;
                _sceneOverlay
                    .DOFade(1f, 0.35f)
                    .SetDelay(0.1f)   // let kick animate first
                    .OnComplete(() =>
                    {
                        SwitchToNextScreen();
                        _sceneOverlay
                            .DOFade(0f, 0.35f)
                            .OnComplete(() => _sceneOverlay.blocksRaycasts = false);
                    });
            }
            else
            {
                SwitchToNextScreen();
            }
        }

        private void SwitchToNextScreen()
        {
            if (_nextScreen != null)
                Focus(_nextScreen);
            else
                Debug.Log("[MK8UI_MainMenuScreen] _nextScreen not assigned yet (Bước 5).");
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private MK8UI_ModeItem GetSelectedItem()
        {
            if (_modeItems == null || _selectedIndex < 0 || _selectedIndex >= _modeItems.Length)
                return null;
            return _modeItems[_selectedIndex];
        }

        private void SnapArrowsToSelected()
        {
            var item = GetSelectedItem();
            if (item == null) return;
            var pos = item.transform.position;
            if (_arrowLeft  != null) _arrowLeft.transform.position  =
                new Vector3(_arrowLeft.transform.position.x,  pos.y, 0f);
            if (_arrowRight != null) _arrowRight.transform.position =
                new Vector3(_arrowRight.transform.position.x, pos.y, 0f);
        }

        private void AnimateArrowsToSelected()
        {
            var item = GetSelectedItem();
            if (item == null) return;
            _arrowLeft?.MoveTo(item.transform.position);
            _arrowRight?.MoveTo(item.transform.position);
        }

        private void InitCharScenes(int activeIndex)
        {
            if (_charScenes == null) return;
            for (int i = 0; i < _charScenes.Length; i++)
            {
                if (_charScenes[i] == null) continue;
                bool active = i == activeIndex;
                _charScenes[i].gameObject.SetActive(active);
                _charScenes[i].alpha = active ? 1f : 0f;
            }
        }

        private void CrossfadeCharScene(int from, int to)
        {
            if (_charScenes == null) return;

            if (from >= 0 && from < _charScenes.Length && _charScenes[from] != null)
            {
                var fromScene = _charScenes[from]; // capture for lambda
                _charScenes[from]
                    .DOFade(0f, 0.2f)
                    .OnComplete(() => fromScene.gameObject.SetActive(false));
            }

            if (to >= 0 && to < _charScenes.Length && _charScenes[to] != null)
            {
                _charScenes[to].gameObject.SetActive(true);
                _charScenes[to].alpha = 0f;
                _charScenes[to].DOFade(1f, 0.25f);
            }
        }
    }
}
