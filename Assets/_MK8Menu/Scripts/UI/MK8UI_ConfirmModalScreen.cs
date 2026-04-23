using System.Text;
using DG.Tweening;
using MK8.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MK8.Menu.UI
{
    /// <summary>
    /// Confirm Modal: summarises all MK8PlayerSelection choices.
    /// A = fade to white → LoadScene("MK8Menu_Loading").
    /// B = back to Cup Select.
    /// Bước 10.
    /// </summary>
    public class MK8UI_ConfirmModalScreen : MK8UIScreen
    {
        [Header("Summary")]
        [SerializeField] private TextMeshProUGUI _summaryText;

        [Header("Scene Overlay (shared WhiteOverlay)")]
        [SerializeField] private CanvasGroup _sceneOverlay;

        [Header("Input")]
        [SerializeField] private MK8InputReader _inputReader;

        private bool  _acceptInput;
        private const string LoadingSceneName = "MK8Menu_Loading";

        // ── Overrides ─────────────────────────────────────────────────────────────

        public override void OnShow()
        {
            _acceptInput = false;

            if (_sceneOverlay != null)
            {
                _sceneOverlay.alpha          = 0f;
                _sceneOverlay.interactable   = false;
                _sceneOverlay.blocksRaycasts = false;
            }

            BuildSummary();
            DOVirtual.DelayedCall(0.3f, () => _acceptInput = true);
        }

        public override void OnHide()
        {
            _acceptInput = false;
            DOTween.Kill(gameObject);
            if (_sceneOverlay != null)
            {
                DOTween.Kill(_sceneOverlay);
                _sceneOverlay.alpha          = 0f;
                _sceneOverlay.blocksRaycasts = false;
            }
        }

        private void Update()
        {
            if (!_acceptInput || _inputReader == null) return;
            if (_inputReader.ConfirmPressedThisFrame) OnConfirm();
            if (_inputReader.BackPressedThisFrame)    OnBack();
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void BuildSummary()
        {
            if (_summaryText == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("<b>YOUR SELECTION</b>\n");

            string mode   = MK8PlayerSelection.GameMode.ToString();
            string speed  = SpeedClassName(MK8PlayerSelection.SpeedClass);
            string chr    = MK8PlayerSelection.Character  != null ? MK8PlayerSelection.Character.displayName  : "(none)";
            string body   = MK8PlayerSelection.KartBody   != null ? MK8PlayerSelection.KartBody.displayName   : "(none)";
            string wheels = MK8PlayerSelection.Wheels     != null ? MK8PlayerSelection.Wheels.displayName     : "(none)";
            string glider = MK8PlayerSelection.Glider     != null ? MK8PlayerSelection.Glider.displayName     : "(none)";
            string cup    = MK8PlayerSelection.Cup        != null ? MK8PlayerSelection.Cup.displayName        : "(none)";
            string track  = MK8PlayerSelection.Track      != null ? MK8PlayerSelection.Track.displayName      : "(none)";

            sb.AppendLine($"Mode:        {mode}");
            sb.AppendLine($"Speed:       {speed}");
            sb.AppendLine($"Character:   {chr}");
            sb.AppendLine($"Kart Body:   {body}");
            sb.AppendLine($"Wheels:      {wheels}");
            sb.AppendLine($"Glider:      {glider}");
            sb.AppendLine($"Cup:         {cup}");
            sb.AppendLine($"Track:       {track}");

            _summaryText.text = sb.ToString();
        }

        private static string SpeedClassName(MK8SpeedClass sc) => sc switch
        {
            MK8SpeedClass.CC50   => "50cc",
            MK8SpeedClass.CC100  => "100cc",
            MK8SpeedClass.CC150  => "150cc",
            MK8SpeedClass.Mirror => "Mirror",
            MK8SpeedClass.CC200  => "200cc",
            _                    => sc.ToString(),
        };

        private void OnConfirm()
        {
            _acceptInput = false;

            if (_sceneOverlay != null)
            {
                _sceneOverlay.blocksRaycasts = true;
                _sceneOverlay.DOFade(1f, 0.4f)
                    .SetDelay(0.05f)
                    .OnComplete(() => SceneManager.LoadScene(LoadingSceneName));
            }
            else
            {
                SceneManager.LoadScene(LoadingSceneName);
            }
        }

        private void OnBack()
        {
            _acceptInput = false;
            Back();
        }
    }
}
