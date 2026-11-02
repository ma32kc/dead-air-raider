using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadAir.Controls
{
    // Единственное место, где читается ввод игрока.
    // Кнопки задаются в DeadAirControls.inputactions (клавиатура+мышь и геймпад),
    // а остальные скрипты берут отсюда готовые значения и не знают, что именно нажато.
    [DisallowMultipleComponent]
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        private const string KeyboardScheme = "Keyboard&Mouse";

        private DeadAirControls controls;

        // Нажатия - событиями (сработать один раз), удержания и оси - свойствами (читать каждый кадр)
        public event Action JumpPressed;
        public event Action FirePressed;

        public Vector2 Move => controls.Player.Move.ReadValue<Vector2>();
        public Vector2 Look => controls.Player.Look.ReadValue<Vector2>();
        public bool SprintHeld => controls.Player.Sprint.IsPressed();
        public bool TapeHeld => controls.Player.Tape.IsPressed();

        // Для подсказок в HUD: показываем ту клавишу, что реально назначена
        public string TapeKeyName => controls.Player.Tape.GetBindingDisplayString(InputBinding.MaskByGroup(KeyboardScheme));

        private void Awake()
        {
            controls = new DeadAirControls();
        }

        private void OnEnable()
        {
            controls.Player.Jump.performed += OnJump;
            controls.Player.Fire.performed += OnFire;
            controls.Player.Enable();
        }

        private void OnDisable()
        {
            controls.Player.Jump.performed -= OnJump;
            controls.Player.Fire.performed -= OnFire;
            controls.Player.Disable();
        }

        private void OnDestroy()
        {
            controls.Dispose();
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            JumpPressed?.Invoke();
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            FirePressed?.Invoke();
        }
    }
}
