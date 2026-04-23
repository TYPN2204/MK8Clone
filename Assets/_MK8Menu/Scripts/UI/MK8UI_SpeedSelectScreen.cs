using DG.Tweening;
using MK8.Shared;
using TMPro;
using UnityEngine;

namespace MK8.Menu.UI
{
    /// <summary>
    /// Speed Select: 50cc / 100cc / 150cc / Mirror / 200cc.
    /// Left panel = 5 speed items with cascade-IN entry from the right.
    /// Right panel = preview image per speed class.
    /// A = confirm (→ Character Select, Bước 7). B = back (→ Mode Select).
    /// Section 5.5 — Bước 6.
    /// </summary>
    public class MK8UI_SpeedSelectScreen : MK8UIScreen
    {
        // ── Speed Items ──────────────────────────────────────────────────────────
        [Header("Speed Items (0=50cc … 4=200cc)")]
        [SerializeField] private MK8UI_ModeItem[] _speedItems;   // exactly 5

        // ── Arrow ────────────────────────────────────────────────────────────────
        [Header("Arrow Indicator")]
        [SerializeField] private MK8UI_ArrowIndicator _arrow;

        // ── Preview Panels ───────────────────────────────────────────────────────
        [Header("Preview Panels (one CanvasGroup per speed class)")]
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
        /// <summary>CharacterSelectScreen — wired in Bước 7.</summary>
        [SerializeField] private MK8UIScreen _nextScreen;

        // ── Speed meta ────────────────────────────────────────────────────────────
        private static readonly string[] Tooltips =
        {
            "A slower, friendlier race.",
            "Pick up the pace!",
            "Full speed ahead!",
            "Mirror world — all tracks flipped!",
            "The fastest class. Good luck!",
        };

        // ── State ─────────────────────────────────────────────────────────────────
        private int       _selectedIndex;
        private bool      _acceptInput;
        private float     _navCooldown;
        private Vector2[] _itemRestPositions;   // cached once on first OnShow
        private const float NavDelay = 0.15f;

        // Cascade entry constants
        private const float EntrySlideOffset = 800f;   // start X offset (right of screen)
        private const float EntrySlideDur    = 0.30f;
        private const float EntryFadeDur     = 0.25f;
        private const float EntryStagger     = 0.06f;

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

            // ── Cache item rest positions on first visit ──────────────────────────
            if (_itemRestPositions == null && _speedItems != null)
            {
                _itemRestPositions = new Vector2[_speedItems.Length];
                for (int i = 0; i < _speedItems.Length; i++)
                    if (_speedItems[i]?.Rect != null)
                        _itemRestPositions[i] = _speedItems[i].Rect.anchoredPosition;
            }

            // ── Position all items off-screen right, alpha 0 ──────────────────────
            for (int i = 0; i < (_speedItems?.Length ?? 0); i++)
            {
                var mi = _speedItems[i];
                if (mi == null) continue;
                var rest = _itemRestPositions != null ? _itemRestPositions[i] : mi.Rect.anchoredPosition;
                if (mi.Rect != null)
                    mi.Rect.anchoredPosition = new Vector2(rest.x + EntrySlideOffset, rest.y);
                if (mi.Fader != null)
                    mi.Fader.alpha = 0f;
                mi.SetSelected(i == 0);
            }

            InitPreviews(0);
            UpdateTooltip(0);

            // ── Cascade items in from right ───────────────────────────────────────
            for (int i = 0; i < (_speedItems?.Length ?? 0); i++)
            {
                var mi = _speedItems[i];
                if (mi?.Rect == null) continue;

                float   delay = i * EntryStagger;
                Vector2 rest  = _itemRestPositions != null
                    ? _itemRestPositions[i]
                    : mi.Rect.anchoredPosition;

                mi.Rect.DOAnchorPos(rest, EntrySlideDur)
                    .SetDelay(delay)
                    .SetEase(Ease.OutCubic);

                if (mi.Fader != null)
                    mi.Fader.DOFade(1f, EntryFadeDur).SetDelay(delay);
            }

            // Show arrow after last item arrives
            float arrowDelay = (_speedItems?.Length ?? 0) * EntryStagger + EntrySlideDur * 0.5f;
            DOVirtual.DelayedCall(arrowDelay, () =>
            {
                SnapArrowToSelected();
                _arrow?.Show(0f);
                _arrow?.StartGlow();
            });

            // Accept input after the full cascade has settled
            float totalDuration = ((_speedItems?.Length ?? 0) - 1) * EntryStagger + EntrySlideDur + 0.1f;
            DOVirtual.DelayedCall(totalDuration, () => _acceptInput = true);
        }

        public override void OnHide()
        {
            _acceptInput = false;
            DOTween.Kill(gameObject);
            _arrow?.StopGlow();
            // Kill all running item tweens (Rect + Fader + gameObject)
            if (_speedItems != null)
                foreach (var mi in _speedItems)
                {
                    if (mi == null) continue;
                    if (mi.Rect  != null) DOTween.Kill(mi.Rect);
                    if (mi.Fader != null) DOTween.Kill(mi.Fader);
                    DOTween.Kill(mi.gameObject);
                }
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
            int count = _speedItems?.Length ?? 0;
            if (count == 0) return;

            int newIndex = (_selectedIndex + dir + count) % count;
            if (newIndex == _selectedIndex) return;

            _speedItems[_selectedIndex]?.SetSelected(false);
            int prev       = _selectedIndex;
            _selectedIndex = newIndex;
            _speedItems[_selectedIndex]?.SetSelected(true);

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

            // Persist the player's speed class choice
            MK8PlayerSelection.SpeedClass = (MK8SpeedClass)_selectedIndex;

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
            Back();
        }

        private void SwitchToNext()
        {
            if (_nextScreen != null)
                Focus(_nextScreen);
            else
                Debug.Log("[MK8UI_SpeedSelectScreen] _nextScreen not assigned yet (Bước 7).");
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private MK8UI_ModeItem GetSelectedItem()
        {
            if (_speedItems == null || _selectedIndex < 0 || _selectedIndex >= _speedItems.Length)
                return null;
            return _speedItems[_selectedIndex];
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
