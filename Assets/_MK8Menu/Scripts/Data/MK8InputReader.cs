using UnityEngine;
using UnityEngine.InputSystem;

namespace MK8.Menu
{
    public class MK8InputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _actions;

        private InputAction _navigate;
        private InputAction _confirm;
        private InputAction _back;

        public Vector2 NavigateValue { get; private set; }
        public bool ConfirmPressedThisFrame { get; private set; }
        public bool BackPressedThisFrame { get; private set; }

        private void Awake()
        {
            if (_actions == null)
            {
                Debug.LogError("MK8InputReader: Missing InputActionAsset reference.");
                return;
            }

            var map = _actions.FindActionMap("UI", true);
            _navigate = map.FindAction("Navigate", true);
            _confirm = map.FindAction("Confirm", true);
            _back = map.FindAction("Back", true);
        }

        private void OnEnable()
        {
            _navigate?.Enable();
            _confirm?.Enable();
            _back?.Enable();
        }

        private void OnDisable()
        {
            _navigate?.Disable();
            _confirm?.Disable();
            _back?.Disable();
        }

        private void Update()
        {
            ConfirmPressedThisFrame = false;
            BackPressedThisFrame = false;

            if (_navigate != null) NavigateValue = _navigate.ReadValue<Vector2>();
            if (_confirm != null && _confirm.WasPressedThisFrame()) ConfirmPressedThisFrame = true;
            if (_back != null && _back.WasPressedThisFrame()) BackPressedThisFrame = true;
        }
    }
}