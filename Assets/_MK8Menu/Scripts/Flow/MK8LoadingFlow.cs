using System.Text;
using DG.Tweening;
using MK8.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MK8.Menu
{
    /// <summary>
    /// Top-level controller for the MK8Menu_Loading scene.
    /// On Start: fades white overlay out, populates selection summary, then waits for A.
    /// A = stub ContinueToRaceScene() — logs and returns to Boot until a race scene exists.
    /// Bước 11.
    /// </summary>
    public class MK8LoadingFlow : MonoBehaviour
    {
        [Header("Overlay")]
        [SerializeField] private CanvasGroup _fadeOverlay;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI _statusLabel;
        [SerializeField] private TextMeshProUGUI _selectionLabel;

        [Header("Input")]
        [SerializeField] private MK8InputReader _inputReader;

        [Header("Timing")]
        [SerializeField] private float _fadeInDuration = 0.8f;

        private bool _acceptInput;

        // ── Unity ─────────────────────────────────────────────────────────────────

        private void Start()
        {
            _acceptInput = false;

            RefreshSelectionLabel();

            if (_statusLabel != null)
                _statusLabel.text = "Loading…";

            if (_fadeOverlay != null)
            {
                _fadeOverlay.alpha          = 1f;
                _fadeOverlay.interactable   = false;
                _fadeOverlay.blocksRaycasts = false;
                _fadeOverlay.DOFade(0f, _fadeInDuration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(OnFadeInComplete);
            }
            else
            {
                OnFadeInComplete();
            }
        }

        private void Update()
        {
            if (!_acceptInput || _inputReader == null) return;
            if (_inputReader.ConfirmPressedThisFrame) ContinueToRaceScene();
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void OnFadeInComplete()
        {
            if (_statusLabel != null)
                _statusLabel.text = "Press  A  to start the race!";
            _acceptInput = true;
        }

        private void RefreshSelectionLabel()
        {
            if (_selectionLabel == null) return;

            var sb = new StringBuilder();

            string chr    = MK8PlayerSelection.Character  != null ? MK8PlayerSelection.Character.displayName  : "—";
            string body   = MK8PlayerSelection.KartBody   != null ? MK8PlayerSelection.KartBody.displayName   : "—";
            string wheels = MK8PlayerSelection.Wheels     != null ? MK8PlayerSelection.Wheels.displayName     : "—";
            string glider = MK8PlayerSelection.Glider     != null ? MK8PlayerSelection.Glider.displayName     : "—";
            string cup    = MK8PlayerSelection.Cup        != null ? MK8PlayerSelection.Cup.displayName        : "—";
            string track  = MK8PlayerSelection.Track      != null ? MK8PlayerSelection.Track.displayName      : "—";
            string speed  = SpeedClassName(MK8PlayerSelection.SpeedClass);

            sb.AppendLine($"Character :  {chr}");
            sb.AppendLine($"Kart Body :  {body}");
            sb.AppendLine($"Wheels    :  {wheels}");
            sb.AppendLine($"Glider    :  {glider}");
            sb.AppendLine($"Cup       :  {cup}");
            sb.AppendLine($"Track     :  {track}");
            sb.AppendLine($"Speed     :  {speed}");

            _selectionLabel.text = sb.ToString();
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

        /// <summary>
        /// Stub — loads race scene when one is available.
        /// Until then, returns to Boot (index 0) to demonstrate the full menu loop.
        /// </summary>
        private void ContinueToRaceScene()
        {
            _acceptInput = false;
            Debug.Log("[MK8LoadingFlow] ContinueToRaceScene — stub. Returning to Boot scene.");
            SceneManager.LoadScene(0); // MK8Menu_Boot
        }
    }
}
