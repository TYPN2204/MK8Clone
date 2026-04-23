using DG.Tweening;
using MK8.Shared;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MK8.Menu.UI
{
    /// <summary>
    /// Character Select: 6×5 portrait grid.
    /// Reads MK8Database.Instance.characters if available; falls back to hard-coded names.
    /// Up/Down/Left/Right = navigate grid. A = confirm (→ Kart Parts). B = back (→ Speed Select).
    /// Bước 7.
    /// </summary>
    public class MK8UI_CharacterSelectScreen : MK8UIScreen
    {
        private const int Cols = 6;
        private const int Rows = 5;

        [Header("Character Cells (30, row-major: [row*6+col])")]
        [SerializeField] private MK8UI_CharacterCell[] _cells;      // 30

        [Header("Right Preview")]
        [SerializeField] private Image           _previewPortrait;  // placeholder colored rect
        [SerializeField] private TextMeshProUGUI _previewName;

        [Header("Scene Overlay")]
        [SerializeField] private CanvasGroup _sceneOverlay;

        [Header("Input")]
        [SerializeField] private MK8InputReader _inputReader;

        [Header("Navigation")]
        [SerializeField] private MK8UIScreen _nextScreen;           // → KartPartsSelect

        // ── Placeholder roster ────────────────────────────────────────────────────
        private static readonly string[] PlaceholderNames =
        {
            "Mario",     "Luigi",       "Peach",       "Daisy",        "Yoshi",     "Toad",
            "Koopa",     "Shy Guy",     "Lakitu",      "Toadette",     "Rosalina",  "Baby Mario",
            "Baby Luigi","Baby Peach",  "Baby Daisy",  "Baby Rosalina","Birdo",     "Diddy Kong",
            "Bowser",    "Donkey Kong", "Waluigi",     "Wario",        "Bowser Jr", "Lemmy",
            "Larry",     "Morton",      "Wendy",       "Iggy",         "Roy",       "Ludwig",
        };

        // last 12 characters are locked in placeholder mode
        private static readonly bool[] PlaceholderLocked =
        {
            false,false,false,false,false,false,
            false,false,false,false,false,false,
            false,false,false,false,true, true,
            false,false,false,false,true, true,
            true, true, true, true, true, true,
        };

        private static readonly Color[] RowColors =
        {
            new Color(0.9f, 0.5f, 0.5f),
            new Color(0.5f, 0.85f, 0.5f),
            new Color(0.5f, 0.5f, 0.9f),
            new Color(0.9f, 0.85f, 0.4f),
            new Color(0.65f,0.65f,0.65f),
        };

        // ── State ─────────────────────────────────────────────────────────────────
        private int   _row, _col;
        private bool  _acceptInput;
        private float _navCooldown;
        private const float NavDelay = 0.13f;

        private int Index => _row * Cols + _col;

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

            PopulateCells();
            SelectCell(0, 0, snap: true);
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

        private void PopulateCells()
        {
            var db = MK8Database.Instance;
            for (int i = 0; i < (_cells?.Length ?? 0); i++)
            {
                var cell = _cells[i];
                if (cell == null) continue;
                int row  = i / Cols;

                if (db?.characters?.Length > 0 && i < db.characters.Length && db.characters[i] != null)
                    cell.SetData(db.characters[i]);
                else
                    cell.SetPlaceholder(
                        i < PlaceholderNames.Length ? PlaceholderNames[i] : $"Char {i + 1}",
                        i < PlaceholderLocked.Length && PlaceholderLocked[i],
                        RowColors[row < RowColors.Length ? row : RowColors.Length - 1]);

                cell.SetSelected(false);
            }
        }

        private void Move(int dRow, int dCol)
        {
            if (_cells != null && Index < _cells.Length)
                _cells[Index]?.SetSelected(false);

            _row = (_row + dRow + Rows) % Rows;
            _col = (_col + dCol + Cols) % Cols;
            SelectCell(_row, _col);
        }

        private void SelectCell(int row, int col, bool snap = false)
        {
            _row = row; _col = col;
            if (_cells != null && Index < _cells.Length)
                _cells[Index]?.SetSelected(true);
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            var db   = MK8Database.Instance;
            int idx  = Index;
            string nm = idx < PlaceholderNames.Length ? PlaceholderNames[idx] : $"Char {idx + 1}";
            if (db?.characters?.Length > 0 && idx < db.characters.Length && db.characters[idx] != null)
                nm = db.characters[idx].displayName;
            if (_previewName != null) _previewName.text = nm;
        }

        private void OnConfirm()
        {
            var cell = (_cells != null && Index < _cells.Length) ? _cells[Index] : null;
            if (cell != null && cell.IsLocked) return;

            _acceptInput = false;
            var db = MK8Database.Instance;
            if (db?.characters?.Length > 0 && Index < db.characters.Length)
                MK8PlayerSelection.Character = db.characters[Index];

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
            else Debug.Log("[CharacterSelect] _nextScreen not assigned (Bước 8).");
        }
    }
}
