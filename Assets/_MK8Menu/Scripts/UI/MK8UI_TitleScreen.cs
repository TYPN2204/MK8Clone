using DG.Tweening;
using MK8.Menu.Effects;
using UnityEngine;

namespace MK8.Menu.UI
{
    /// <summary>
    /// Title screen — white bg, Mario sprite right, logo bottom-left, blinking "Press A".
    /// On confirm: ripple + Mario shrink/slide → logo fade → blue diagonal panel → next screen.
    ///
    /// Section 5.2 of mk8_clone_prompt.md — Bước 3.
    /// </summary>
    public class MK8UI_TitleScreen : MK8UIScreen
    {
        // ── UI Elements ─────────────────────────────────────────────────────────
        [Header("Title Elements")]
        [SerializeField] private RectTransform _marioKartRect;  // placeholder image, right-center
        [SerializeField] private CanvasGroup   _logoGroup;       // MARIOKART8 logo + "Ver. 4.1"
        [SerializeField] private CanvasGroup   _pressAGroup;     // "Press Ⓐ to start" container
        [SerializeField] private MK8BlinkText  _blinkText;       // blink component on press-A text

        [Header("Diagonal Panel (Blue Transition)")]
        [SerializeField] private RectTransform _diagonalPanel;
        // Hidden: off-screen top-left.  Visible: covers left portion of screen.
        [SerializeField] private Vector2 _panelHiddenPos  = new Vector2(-2200f, 600f);
        [SerializeField] private Vector2 _panelVisiblePos = new Vector2(-50f, 0f);

        [Header("Ripple Effect")]
        [SerializeField] private MK8WaterRippleEffect _rippleEffect;

        [Header("Input")]
        [SerializeField] private MK8InputReader _inputReader;

        [Header("Navigation")]
        /// <summary>Assigned in Bước 4 (MainMenuScreen). Leave empty for now.</summary>
        [SerializeField] private MK8UIScreen _nextScreen;

        // ── Internal state ───────────────────────────────────────────────────────
        private bool     _acceptInput;
        private Sequence _showSequence;
        private Vector2  _marioOriginalPos;

        // ── MK8UIScreen overrides ────────────────────────────────────────────────

        public override void OnShow()
        {
            _acceptInput = false;
            _showSequence?.Kill();

            // Cache Mario original anchored position so the screen can be re-entered later
            if (_marioKartRect != null)
            {
                _marioOriginalPos              = _marioKartRect.anchoredPosition;
                _marioKartRect.localScale      = Vector3.one;
            }

            // Reset Press-A group: invisible
            if (_pressAGroup != null)
            {
                _pressAGroup.alpha          = 0f;
                _pressAGroup.interactable   = false;
                _pressAGroup.blocksRaycasts = false;
            }

            // Reset logo group: fully visible
            if (_logoGroup != null) _logoGroup.alpha = 1f;

            // Reset diagonal panel off-screen
            if (_diagonalPanel != null)
                _diagonalPanel.anchoredPosition = _panelHiddenPos;

            // After 0.5 s → show "Press A" text + enable blink + accept input
            // Section 5.2: "Sau ~0.5s: text 'Press Ⓐ to start' xuất hiện"
            _showSequence = DOTween.Sequence()
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    if (_pressAGroup != null)
                    {
                        _pressAGroup.DOFade(1f, 0.3f);
                        _pressAGroup.interactable   = true;
                        _pressAGroup.blocksRaycasts = true;
                    }
                    _blinkText?.StartBlink();
                    _acceptInput = true;
                });
        }

        public override void OnHide()
        {
            _acceptInput = false;
            _showSequence?.Kill();
            DOTween.Kill(gameObject);
            _blinkText?.StopBlink();
        }

        private void Update()
        {
            if (!_acceptInput || _inputReader == null) return;
            if (_inputReader.ConfirmPressedThisFrame) OnConfirm();
        }

        // ── Confirm → transition ─────────────────────────────────────────────────

        private void OnConfirm()
        {
            _acceptInput = false;
            _blinkText?.StopBlink();

            // 1. Water ripple at Mario's canvas position (Section 5.2)
            if (_rippleEffect != null && _marioKartRect != null)
                _rippleEffect.Play(_marioKartRect.anchoredPosition);

            PlayTransitionOut();
        }

        private void PlayTransitionOut()
        {
            // Section 5.2 transition sequence:
            // Mario scale+slide → logo/pressA fade → diagonal panel → focus next screen

            var seq = DOTween.Sequence().SetTarget(gameObject);

            // 2. Mario: scale 1→0.75 + translate right (Parallel — both at t=0)
            if (_marioKartRect != null)
            {
                seq.Join(_marioKartRect
                    .DOScale(0.75f, 0.4f)
                    .SetEase(Ease.OutQuad));

                seq.Join(_marioKartRect
                    .DOAnchorPosX(_marioOriginalPos.x + 300f, 0.4f)
                    .SetEase(Ease.OutQuad));
            }

            // 3. Logo + "Press A" fade out (parallel with Mario)
            if (_logoGroup   != null) seq.Join(_logoGroup.DOFade(0f,   0.3f));
            if (_pressAGroup != null) seq.Join(_pressAGroup.DOFade(0f, 0.3f));

            // 4. Small gap then diagonal blue panel slides in from top-left
            //    Section 5.2: "dải màu xanh slide từ góc trên-trái chéo xuống"
            if (_diagonalPanel != null)
            {
                seq.AppendInterval(0.05f);
                seq.Append(_diagonalPanel
                    .DOAnchorPos(_panelVisiblePos, 0.45f)
                    .SetEase(Ease.OutQuart));
            }

            // 5. Focus next screen (MainMenuScreen — wired in Bước 4)
            seq.OnComplete(() =>
            {
                if (_nextScreen != null)
                {
                    Focus(_nextScreen);
                }
                else
                {
                    Debug.Log("[MK8UI_TitleScreen] Transition done. " +
                              "_nextScreen not yet assigned — assign MainMenuScreen in Bước 4.");
                }
            });
        }
    }
}
