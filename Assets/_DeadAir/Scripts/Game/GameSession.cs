using DeadAir.Controls;
using DeadAir.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DeadAir.Game
{
    // Общие для сессии кнопки (карта действий Game): перезапуск после смерти,
    // включение отладки, захват и освобождение курсора.
    [DisallowMultipleComponent]
    public sealed class GameSession : MonoBehaviour
    {
        private const string KeyboardScheme = "Keyboard&Mouse";

        [SerializeField] private Health player;

        private DeadAirControls controls;

        public string RestartKeyName => controls.Game.Restart.GetBindingDisplayString(InputBinding.MaskByGroup(KeyboardScheme));

        private void Awake()
        {
            controls = new DeadAirControls();
        }

        private void OnEnable()
        {
            controls.Game.Restart.performed += OnRestart;
            controls.Game.ToggleDebug.performed += OnToggleDebug;
            controls.Game.ReleaseCursor.performed += OnReleaseCursor;
            controls.Game.CaptureCursor.performed += OnCaptureCursor;
            controls.Game.Enable();
        }

        private void OnDisable()
        {
            controls.Game.Restart.performed -= OnRestart;
            controls.Game.ToggleDebug.performed -= OnToggleDebug;
            controls.Game.ReleaseCursor.performed -= OnReleaseCursor;
            controls.Game.CaptureCursor.performed -= OnCaptureCursor;
            controls.Game.Disable();
        }

        private void OnDestroy()
        {
            controls.Dispose();
        }

        private void Start()
        {
            SetCursorLocked(true);
        }

        // Перезапуск только после смерти, чтобы случайно не сбросить забег
        private void OnRestart(InputAction.CallbackContext context)
        {
            if (player.IsDead)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnToggleDebug(InputAction.CallbackContext context)
        {
            DebugOverlay.Toggle();
        }

        private void OnReleaseCursor(InputAction.CallbackContext context)
        {
            SetCursorLocked(false);
        }

        private void OnCaptureCursor(InputAction.CallbackContext context)
        {
            SetCursorLocked(true);
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
