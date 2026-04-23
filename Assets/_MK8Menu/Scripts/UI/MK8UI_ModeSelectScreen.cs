using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MK8.Menu.UI
{
    /// <summary>
    /// Mode Select: Grand Prix / Time Trials / VS Race / Battle.
    /// Left panel = 4 mode items + arrow. Right panel = preview + tooltip.
    /// A = confirm (→ Speed Select), B = back (→ Main Menu).
    /// Section 5.4 — Bước 5.
    /// </summary>
    public class MK8UI_ModeSelectScreen : MK8UIScreen
    {
        // ── Mode Items ───────────────────────────────────────────────────────────
        [Header("Mode Items (0=GrandPrix … 3=Battle)")]
        [SerializeField] private MK8UI_ModeItem[] _modeItems;   // exactly 4

        // ── Arrow ────────────────────────────────────────────────────────────────
        [Header("Arrow Indicator")]
        [SerializeField] private MK8UI_ArrowIndicator _arrow;

        // ── Preview Panels ───────────────────────────────────────────────────────
        [Header("Preview Panels (one CanvasGroup per mode)")]
        [SerializeField] private CanvasGroup[] _previewPanels;

        // ── Tooltip ───────────────────────────────────────────────────────────────
        [Header("Tooltip Text (overlay on preview)")]
        [SerializeField] private TextMeshProUGUI _tooltipText;

        // ── Transition Overlay ────────────────────────────────────────────────────
        [Header("Scene Transition Overlay (shared WhiteOverlay)")]
        [SerializeField] private CanvasGroup _sceneOverlay;

        // ── Input / Navigation ────────────────────────────────────────────────────
        [Header("Input")]
        [SerializeField] private MK8InputReader _inputReader;

        [Header("Navigation")]
        /// <summary>SpeedSelectScreen — wired in Bước 6.</summary>
        [SerializeField] private MK8UIScreen _nextScreen;

        // ── Mode meta (must match array order) ────────────────────────────────────
        private static readonly string[] Tooltips =
        {
            "Go for gold in a 4-race cup!",
            "Race alone for new records!",
            "Race using custom rules!",
            "Pop your rivals' balloons!",
        };

        // ── State ─────────────────────────────────────────────────────────────────
        private int   _selectedIndex;
        private bool  _acceptInput;
        private float _navCooldown;
        private const float NavDelay = 0.15f;

        // ── MK8UIScreen overrides ─────────────────────────────────────────────────

        public override void OnShow()
        {
            _acceptInput   = false;
            _navCooldown   = 0f;
            _selectedIndex = 0;

            if (_sceneOverlay != null)
            {
                _sceneOverlay.alpha          = 0f;
                _sceneOverlay.interactable   = false;
                _sceneOverlay.blocksRaycasts = false;
            }

            for (int i = 0; i < _modeItems.Length; i++)
                _modeItems[i]?.SetSelected(i == 0);

            InitPreviews(0);
            UpdateTooltip(0);

            SnapArrowToSelected();
            _arrow?.Show(0f);
            _arrow?.StartGlow();

            DOVirtual.DelayedCall(0.3f, () => _acceptInput = true);
        }

        public override void OnHide()
        {
            _acceptInput = false;
            DOTween.Kill(gameObject);
            _arrow?.StopGlow();
        }

        private void Update()
        {
            if (!_acceptInput || _inputReader == null) return;

            _navCooldown -= Time.deltaTime;

            float navY = _inputReader.NavigateValue.y;
            if (_navCooldown <= 0f)
            {
                if      (navY >  0.5f) { Navigate(-1); _navCooldown = NavDelay; }
                else if (navY < -0.5f) { Navigate( 1); _navCooldown = NavDelay; }
            }

            if (_inputReader.ConfirmPressedThisFrame) OnConfirm();
            if (_inputReader.BackPressedThisFrame)    OnBack();
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void Navigate(int dir)
        {
            int count    = _modeItems.Length;
            if (count == 0) return;
            int newIndex = (_selectedIndex + dir + count) % count;
            if (newIndex == _selectedIndex) return;

            _modeItems[_selectedIndex]?.SetSelected(false);
            int prev       = _selectedIndex;
            _selectedIndex = newIndex;
            _modeItems[_selectedIndex]?.SetSelected(true);

            AnimateArrowToSelected();
            CrossfadePreview(prev, _selectedIndex);
            UpdateTooltip(_selectedIndex);
        }

        private void OnConfirm()
        {
            _acceptInput = false;

            var item = GetSelectedItem();
            item?.PlayKick();
            _arrow?.Hide();

            if (_sceneOverlay != null)
            {
                _sceneOverlay.blocksRaycasts = true;
                _sceneOverlay
                    .DOFade(1f, 0.35f)
                    .SetDelay(0.1f)
                    .OnComplete(() =>
                    {
                        SwitchToNext();
                        _sceneOverlay
                            .DOFade(0f, 0.35f)
                            .OnComplete(() => _sceneOverlay.blocksRaycasts = false);
                    });
            }
            else
            {
                SwitchToNext();
            }
        }

        private void OnBack()
        {
            _acceptInput = false;
            _arrow?.Hide(0.15f);
            // MK8UIScreen.Back() restores the previous screen (MainMenuScreen)
            Back();
        }

        private void SwitchToNext()
        {
            if (_nextScreen != null)
                Focus(_nextScreen);
            else
                Debug.Log("[MK8UI_ModeSelectScreen] _nextScreen not assigned yet (Bước 6).");
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private MK8UI_ModeItem GetSelectedItem()
        {
            if (_modeItems == null || _selectedIndex < 0 || _selectedIndex >= _modeItems.Length)
                return null;
            return _modeItems[_selectedIndex];
        }

        private void SnapArrowToSelected()
        {
            var item = GetSelectedItem();
            if (item == null || _arrow == null) return;
            var pos = item.transform.position;
            _arrow.transform.position = new Vector3(_arrow.transform.position.x, pos.y, 0f);
        }

        private void AnimateArrowToSelected()
        {
            var item = GetSelectedItem();
            if (item == null) return;
            _arrow?.MoveTo(item.transform.position);
        }

        private void InitPreviews(int activeIndex)
        {
            if (_previewPanels == null) return;
            for (int i = 0; i < _previewPanels.Length; i++)
            {
                if (_previewPanels[i] == null) continue;
                bool active = i == activeIndex;
                _previewPanels[i].gameObject.SetActive(active);
                _previewPanels[i].alpha = active ? 1f : 0f;
            }
        }

        private void CrossfadePreview(int from, int to)
        {
            if (_previewPanels == null) return;

            if (from >= 0 && from < _previewPanels.Length && _previewPanels[from] != null)
            {
                var panel = _previewPanels[from];
                panel.DOFade(0f, 0.2f).OnComplete(() => panel.gameObject.SetActive(false));
            }

            if (to >= 0 && to < _previewPanels.Length && _previewPanels[to] != null)
            {
                _previewPanels[to].gameObject.SetActive(true);
                _previewPanels[to].alpha = 0f;
                _previewPanels[to].DOFade(1f, 0.25f);
            }
        }

        private void UpdateTooltip(int index)
        {
            if (_tooltipText == null) return;
            if (index >= 0 && index < Tooltips.Length)
                _tooltipText.text = Tooltips[index];
        }
    }
}
