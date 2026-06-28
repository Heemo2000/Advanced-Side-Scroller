using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.IOC;
using Utilities.PauseHandling;

namespace Game.InputHandling
{
    public class GameInput : MonoBehaviour, IPausable
    {
        private GameControls _gameControls;
        private bool _isPaused = false;
        private GamePauseManager _gamePauseManager;

        public event Action OnJump;
        

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _gameControls = new GameControls();
            _gameControls.Enable();
            _gameControls.GameActionMap.Jump.started += OnJumpPressed;
            _gameControls.GameActionMap.TogglePause.started += OnTogglePausePressed;
        }

        

        private void OnDestroy()
        {
            _gameControls.Disable();
            _gameControls.GameActionMap.Jump.started -= OnJumpPressed;
            _gameControls.GameActionMap.TogglePause.started -= OnTogglePausePressed;
        }

        public void OnPause()
        {
            _isPaused = true;
            _gameControls.GameActionMap.Movement.Disable();
            _gameControls.GameActionMap.Jump.Disable();
        }

        public void OnResume()
        {
            _isPaused = false;
            _gameControls.GameActionMap.Movement.Enable();
            _gameControls.GameActionMap.Jump.Enable();
        }

        public float GetMoveInput()
        {
            if(_isPaused)
            {
                return 0.0f;
            }
            float moveInputX = _gameControls.GameActionMap.Movement.ReadValue<Vector2>().x;
            return moveInputX;
        }

        public bool GetJumpHeldInput()
        {
            if(_isPaused)
            {
                return false;
            }

            bool jumpHeld = _gameControls.GameActionMap.Jump.IsPressed();
            return jumpHeld;
        }

        private void OnJumpPressed(InputAction.CallbackContext context)
        {
            OnJump?.Invoke();
        }

        private void OnTogglePausePressed(InputAction.CallbackContext context)
        {
            if(_gamePauseManager == null)
            {
                _gamePauseManager = ServiceLocator.ForSceneOf(this).Get<GamePauseManager>();
                _gamePauseManager.Register(this);
            }

            _gamePauseManager.Toggle();
        }
    }
}
