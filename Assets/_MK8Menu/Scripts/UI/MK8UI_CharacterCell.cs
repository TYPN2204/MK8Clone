using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MK8.Menu.UI
{
    /// <summary>
    /// One cell in the Character Select 6×5 grid.
    /// Populated at runtime by MK8UI_CharacterSelectScreen.OnShow().
    /// Bước 7.
    /// </summary>
    public class MK8UI_CharacterCell : MonoBehaviour
    {
        [SerializeField] private Image           _portrait;       // colored placeholder rect
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private Image           _selectionFrame; // gold border, toggled
        [SerializeField] private GameObject      _lockOverlay;    // dim + lock text

        public bool          IsLocked { get; private set; }
        public RectTransform Rect     { get; private set; }

        private void Awake() => Rect = GetComponent<RectTransform>();

        // ── Data binding ──────────────────────────────────────────────────────────

        /// <summary>Fills cell from ScriptableObject if available.</summary>
        public void SetData(MK8.Shared.MK8CharacterData data)
        {
            if (data == null) return;
            IsLocked = data.isLocked;
            if (_nameLabel != null) _nameLabel.text = data.displayName;
            if (_portrait  != null && data.portrait != null)
            {
                _portrait.sprite = data.portrait;
                _portrait.color  = data.isLocked ? new Color(0.4f, 0.4f, 0.4f) : Color.white;
            }
            if (_lockOverlay != null) _lockOverlay.SetActive(data.isLocked);
        }

        /// <summary>Fills cell with placeholder data (no ScriptableObject needed).</summary>
        public void SetPlaceholder(string charName, bool locked, Color portraitColor)
        {
            IsLocked = locked;
            if (_nameLabel != null)
                _nameLabel.text = locked ? "???" : charName;
            if (_portrait != null)
                _portrait.color = locked
                    ? new Color(portraitColor.r * 0.35f, portraitColor.g * 0.35f, portraitColor.b * 0.35f)
                    : portraitColor;
            if (_lockOverlay != null) _lockOverlay.SetActive(locked);
        }

        public void SetSelected(bool selected)
        {
            if (_selectionFrame != null) _selectionFrame.gameObject.SetActive(selected);
        }
    }
}
