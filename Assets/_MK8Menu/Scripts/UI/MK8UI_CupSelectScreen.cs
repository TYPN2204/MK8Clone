using System;
using DG.Tweening;
using MK8.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MK8.Menu.UI
{
    /// <summary>
    /// Cup Select: 4×3 grid of cups. Right panel shows 4 track names for the selected cup.
    /// A = confirm (→ Confirm Modal, Bước 10). B = back (→ Kart Parts Select).
    /// Reads MK8Database if available; falls back to hard-coded MK8 cup/track names.
    /// Bước 9.
    /// </summary>
    public class MK8UI_CupSelectScreen : MK8UIScreen
    {
        private const int GridCols = 4;
        private const int GridRows = 3;

        [Header("Cup Grid (12 cells, row-major)")]
        [SerializeField] private Image[]           _cupIcons;    // 12 — colored placeholder
        [SerializeField] private TextMeshProUGUI[] _cupNames;    // 12
        [SerializeField] private GameObject[]      _cupFrames;   // 12 — selection ring

        [Header("Track Preview (right panel)")]
        [SerializeField] private TextMeshProUGUI   _selectedCupLabel;
        [SerializeField] private TextMeshProUGUI[] _trackLabels;         // 4

        [Header("Scene Overlay")]
        [SerializeField] private CanvasGroup _sceneOverlay;

        [Header("Input")]
        [SerializeField] private MK8InputReader _inputReader;

        [Header("Navigation")]
        [SerializeField] private MK8UIScreen _nextScreen;                // → ConfirmModal (Bước 10)

        // ── Placeholder data ──────────────────────────────────────────────────────
        private static readonly string[] CupNames =
        {
            "Mushroom Cup","Flower Cup","Star Cup","Special Cup",
            "Egg Cup","Triforce Cup","Crossing Cup","Bell Cup",
            "Lightning Cup","Leaf Cup","Acorn Cup","Rock Cup",
        };

        private static readonly Color[] CupColors =
        {
            new Color(0.9f,0.55f,0.2f), new Color(0.4f,0.85f,0.3f), new Color(0.9f,0.85f,0.2f), new Color(0.85f,0.2f,0.2f),
            new Color(0.3f,0.75f,0.9f), new Color(0.6f,0.3f,0.9f),  new Color(0.9f,0.5f,0.7f),  new Color(0.9f,0.7f,0.2f),
            new Color(0.35f,0.35f,0.9f),new Color(0.3f,0.75f,0.45f),new Color(0.8f,0.45f,0.3f), new Color(0.55f,0.55f,0.55f),
        };

        private static readonly string[][] CupTracks =
        {
            new[]{"Mario Kart Stadium","Water Park","Sweet Sweet Canyon","Thwomp Ruins"},
            new[]{"Mario Circuit","Toad Harbor","Twisted Mansion","Shy Guy Falls"},
            new[]{"Sunshine Airport","Dolphin Shoals","Electrodrome","Mount Wario"},
            new[]{"Cloudtop Cruise","Bone-Dry Dunes","Bowser's Castle","Rainbow Road"},
            new[]{"Yoshi Circuit","Excitebike Arena","Dragon Driftway","Mute City"},
            new[]{"Toad's Turnpike","Choco Mountain","Koopa Troopa Beach","Kalamari Desert"},
            new[]{"Wario Stadium","Sherbet Land","Royal Raceway","Bowser's Castle"},
            new[]{"Neo Bowser City","Ribbon Road","Super Bell Subway","Big Blue"},
            new[]{"Wario's Gold Mine","Coconut Mall","Koopa Cape","Maple Treeway"},
            new[]{"Kalimari Desert","Waluigi Pinball","DK Mountain","Daisy Cruiser"},
            new[]{"Acorn Cup 1","Acorn Cup 2","Acorn Cup 3","Acorn Cup 4"},
            new[]{"Rock Cup 1","Rock Cup 2","Rock Cup 3","Rock Cup 4"},
        };

        // ── State ─────────────────────────────────────────────────────────────────
        private int   _row, _col;
        private bool  _acceptInput;
        private float _navCooldown;
        private const float NavDelay = 0.13f;

        private int SelIdx => _row * GridCols + _col;

        // ── Overrides ─────────────────────────────────────────────────────────────

        public override void OnShow()
        {
            _acceptInput = false;
            _navCooldown = 0f;
            _row = _col = 0;

            if (_sceneOverlay != null)
            {
                _sceneOverlay.alpha          = 0f;
                _sceneOverlay.interactable   = false;
                _sceneOverlay.blocksRaycasts = false;
            }

            PopulateGrid();
            SetFrame(SelIdx, true);
            RefreshTrackPanel();
            DOVirtual.DelayedCall(0.3f, () => _acceptInput = true);
        }

        public override void OnHide()
        {
            _acceptInput = false;
            DOTween.Kill(gameObject);
        }

        private void Update()
        {
            if (!_acceptInput || _inputReader == null) return;
            _navCooldown -= Time.deltaTime;

            if (_navCooldown <= 0f)
            {
                float x = _inputReader.NavigateValue.x;
                float y = _inputReader.NavigateValue.y;
                if      (y >  0.5f) { Move(-1,  0); _navCooldown = NavDelay; }
                else if (y < -0.5f) { Move( 1,  0); _navCooldown = NavDelay; }
                else if (x >  0.5f) { Move( 0,  1); _navCooldown = NavDelay; }
                else if (x < -0.5f) { Move( 0, -1); _navCooldown = NavDelay; }
            }

            if (_inputReader.ConfirmPressedThisFrame) OnConfirm();
            if (_inputReader.BackPressedThisFrame)    OnBack();
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void PopulateGrid()
        {
            var db    = MK8Database.Instance;
            int total = GridCols * GridRows;
            for (int i = 0; i < total; i++)
            {
                string name  = i < CupNames.Length  ? CupNames[i]  : $"Cup {i + 1}";
                Color  color = i < CupColors.Length ? CupColors[i] : Color.gray;

                if (db?.cups?.Length > 0 && i < db.cups.Length && db.cups[i] != null)
                    name = db.cups[i].displayName;

                if (_cupNames  != null && i < _cupNames.Length  && _cupNames[i]  != null) _cupNames[i].text  = name;
                if (_cupIcons  != null && i < _cupIcons.Length  && _cupIcons[i]  != null) _cupIcons[i].color = color;
                SetFrame(i, false);
            }
        }

        private void Move(int dRow, int dCol)
        {
            SetFrame(SelIdx, false);
            _row = (_row + dRow + GridRows) % GridRows;
            _col = (_col + dCol + GridCols) % GridCols;
            SetFrame(SelIdx, true);
            RefreshTrackPanel();
        }

        private void SetFrame(int idx, bool on)
        {
            if (_cupFrames != null && idx < _cupFrames.Length && _cupFrames[idx] != null)
                _cupFrames[idx].SetActive(on);
        }

        private void RefreshTrackPanel()
        {
            int    idx    = SelIdx;
            var    db     = MK8Database.Instance;
            string cupNm  = idx < CupNames.Length  ? CupNames[idx]  : $"Cup {idx + 1}";
            var    tracks = idx < CupTracks.Length  ? CupTracks[idx] : new[] { "—", "—", "—", "—" };

            if (db?.cups?.Length > 0 && idx < db.cups.Length && db.cups[idx] != null)
            {
                var cup = db.cups[idx];
                cupNm = cup.displayName;
                if (cup.tracks?.Length > 0)
                    tracks = Array.ConvertAll(cup.tracks, t => t?.displayName ?? "—");
            }

            if (_selectedCupLabel != null) _selectedCupLabel.text = cupNm;
            for (int i = 0; i < (_trackLabels?.Length ?? 0); i++)
            {
                if (_trackLabels[i] == null) continue;
                _trackLabels[i].text = i < tracks.Length ? $"{i + 1}.  {tracks[i]}" : "";
            }
        }

        private void OnConfirm()
        {
            _acceptInput = false;
            var db = MK8Database.Instance;
            if (db?.cups?.Length > 0 && SelIdx < db.cups.Length && db.cups[SelIdx] != null)
            {
                MK8PlayerSelection.Cup = db.cups[SelIdx];
                if (db.cups[SelIdx].tracks?.Length > 0)
                    MK8PlayerSelection.Track = db.cups[SelIdx].tracks[0];
            }

            if (_sceneOverlay != null)
            {
                _sceneOverlay.blocksRaycasts = true;
                _sceneOverlay.DOFade(1f, 0.35f).SetDelay(0.1f).OnComplete(() =>
                {
                    SwitchToNext();
                    _sceneOverlay.DOFade(0f, 0.35f).OnComplete(
                        () => _sceneOverlay.blocksRaycasts = false);
                });
            }
            else SwitchToNext();
        }

        private void OnBack() { _acceptInput = false; Back(); }

        private void SwitchToNext()
        {
            if (_nextScreen != null) Focus(_nextScreen);
            else Debug.Log("[CupSelect] _nextScreen not assigned (Bước 10).");
        }
    }
}
