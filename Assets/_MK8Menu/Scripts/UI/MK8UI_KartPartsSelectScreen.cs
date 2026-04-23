using DG.Tweening;
using MK8.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MK8.Menu.UI
{
    /// <summary>
    /// Kart Parts Select: Body / Wheels / Glider — 3 vertical columns.
    /// Left/Right = switch column. Up/Down = scroll item. A = confirm. B = back.
    /// Reads MK8Database if available, else uses built-in placeholder lists.
    /// Bước 8.
    /// </summary>
    public class MK8UI_KartPartsSelectScreen : MK8UIScreen
    {
        [Header("Column Panels (0=Body, 1=Wheels, 2=Glider)")]
        [SerializeField] private CanvasGroup[]     _columnPanels;      // 3 — alpha 1=active 0.45=dim
        [SerializeField] private TextMeshProUGUI[] _itemNames;         // 3
        [SerializeField] private Image[]           _itemIcons;         // 3 — placeholder color rect
        [SerializeField] private TextMeshProUGUI[] _itemCountLabels;   // 3 — "2 / 5"

        [Header("Scene Overlay")]
        [SerializeField] private CanvasGroup _sceneOverlay;

        [Header("Input")]
        [SerializeField] private MK8InputReader _inputReader;

        [Header("Navigation")]
        [SerializeField] private MK8UIScreen _nextScreen;              // → CupSelect

        // ── Fallback catalogue ────────────────────────────────────────────────────
        private static readonly string[][] Fallback =
        {
            new[] { "Standard Kart", "Pipe Frame",   "Mach 8",       "Steel Driver", "Cat Cruiser"  },
            new[] { "Standard",      "Monster",       "Roller",       "Slim",         "Slick"        },
            new[] { "Super Glider",  "Cloud Glider",  "Wario Wing",   "Waddle Wing",  "Peach Parasol"},
        };

        private static readonly Color[] ColumnTint =
        {
            new Color(0.25f, 0.55f, 1f),    // Body   — blue
            new Color(0.25f, 0.80f, 0.35f), // Wheels — green
            new Color(0.95f, 0.60f, 0.10f), // Glider — orange
        };

        // ── State ─────────────────────────────────────────────────────────────────
        private int   _activeCol;
        private int[] _idx = { 0, 0, 0 };
        private bool  _acceptInput;
        private float _navCooldown;
        private const float NavDelay = 0.13f;

        private int Count(int col)
        {
            var db = MK8Database.Instance;
            return col switch
            {
                0 => db?.kartBodies?.Length > 0 ? db.kartBodies.Length : Fallback[0].Length,
                1 => db?.wheels?.Length     > 0 ? db.wheels.Length     : Fallback[1].Length,
                2 => db?.gliders?.Length    > 0 ? db.gliders.Length    : Fallback[2].Length,
                _ => 1,
            };
        }

        private string ItemName(int col, int i)
        {
            var db = MK8Database.Instance;
            int c  = Count(col);
            int si = i % c;
            return col switch
            {
                0 => db?.kartBodies?.Length > 0 ? db.kartBodies[si].displayName : Fallback[0][si],
                1 => db?.wheels?.Length     > 0 ? db.wheels    [si].displayName : Fallback[1][si],
                2 => db?.gliders?.Length    > 0 ? db.gliders   [si].displayName : Fallback[2][si],
                _ => "—",
            };
        }

        // ── Overrides ─────────────────────────────────────────────────────────────

        public override void OnShow()
        {
            _acceptInput = false;
            _navCooldown = 0f;
            _activeCol   = 0;
            _idx         = new[] { 0, 0, 0 };

            if (_sceneOverlay != null)
            {
                _sceneOverlay.alpha          = 0f;
                _sceneOverlay.interactable   = false;
                _sceneOverlay.blocksRaycasts = false;
            }

            RefreshAll();
            HighlightCol(_activeCol);
            DOVirtual.DelayedCall(0.3f, () => _acceptInput = true);
        }

        public override void OnHide()
        {
            _acceptInput = false;
            DOTween.Kill(gameObject);
            // Kill column-panel CanvasGroup tweens (DOFade targets the CG, not the GO)
            if (_columnPanels != null)
                foreach (var cg in _columnPanels)
                    if (cg != null) DOTween.Kill(cg);
        }

        private void Update()
        {
            if (!_acceptInput || _inputReader == null) return;
            _navCooldown -= Time.deltaTime;

            if (_navCooldown <= 0f)
            {
                float x = _inputReader.NavigateValue.x;
                float y = _inputReader.NavigateValue.y;
                if      (x >  0.5f) { ShiftCol( 1); _navCooldown = NavDelay; }
                else if (x < -0.5f) { ShiftCol(-1); _navCooldown = NavDelay; }
                else if (y >  0.5f) { Scroll(-1);   _navCooldown = NavDelay; }
                else if (y < -0.5f) { Scroll( 1);   _navCooldown = NavDelay; }
            }

            if (_inputReader.ConfirmPressedThisFrame) OnConfirm();
            if (_inputReader.BackPressedThisFrame)    OnBack();
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private void ShiftCol(int dir)
        {
            _activeCol = (_activeCol + dir + 3) % 3;
            HighlightCol(_activeCol);
        }

        private void Scroll(int dir)
        {
            _idx[_activeCol] = (_idx[_activeCol] + dir + Count(_activeCol)) % Count(_activeCol);
            RefreshCol(_activeCol);
        }

        private void HighlightCol(int active)
        {
            if (_columnPanels == null) return;
            for (int i = 0; i < _columnPanels.Length; i++)
                if (_columnPanels[i] != null)
                    _columnPanels[i].DOFade(i == active ? 1f : 0.45f, 0.15f);
        }

        private void RefreshAll() { for (int c = 0; c < 3; c++) RefreshCol(c); }

        private void RefreshCol(int col)
        {
            int    cnt  = Count(col);
            int    i    = _idx[col];
            string name = ItemName(col, i);

            if (_itemNames       != null && col < _itemNames.Length       && _itemNames[col]       != null)
                _itemNames[col].text       = name;
            if (_itemCountLabels != null && col < _itemCountLabels.Length && _itemCountLabels[col] != null)
                _itemCountLabels[col].text = $"{i + 1} / {cnt}";
            if (_itemIcons       != null && col < _itemIcons.Length       && _itemIcons[col]       != null)
                _itemIcons[col].color      = ColumnTint[col]; // placeholder
        }

        private void OnConfirm()
        {
            _acceptInput = false;
            var db = MK8Database.Instance;
            if (db != null)
            {
                if (db.kartBodies?.Length > 0)
                    MK8PlayerSelection.KartBody = db.kartBodies[_idx[0] % db.kartBodies.Length];
                if (db.wheels?.Length     > 0)
                    MK8PlayerSelection.Wheels   = db.wheels    [_idx[1] % db.wheels.Length    ];
                if (db.gliders?.Length    > 0)
                    MK8PlayerSelection.Glider   = db.gliders   [_idx[2] % db.gliders.Length   ];
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
            else Debug.Log("[KartPartsSelect] _nextScreen not assigned (Bước 9).");
        }
    }
}
