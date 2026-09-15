using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using Utilities.IOC;
using Utilities.PauseHandling;

namespace Game.InputHandling
{
    public class GameInput : MonoBehaviour, IPausable
    {
        [SerializeField] private OnScreenStick _aimJoystick;
        private GameControls _gameControls;
        private bool _isPaused = false;
        private GamePauseManager _gamePauseManager;

        public event Action OnJump;
        public event Action OnShoot;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _gameControls = new GameControls();
            _gameControls.Enable();
            
            _gameControls.PC.Jump.started += OnJumpPressed;
            _gameControls.PC.TogglePause.started += OnTogglePausePressed;
            _gameControls.PC.Shoot.performed += OnShootPerformed;

            _gameControls.Mobile.Jump.started += OnJumpPressed;
            _gameControls.Mobile.TogglePause.started += OnTogglePausePressed;
            _gameControls.Mobile.Shoot.performed += OnShootPerformed;
        }

        

        private void OnDestroy()
        {
            _gameControls.Disable();
            _gameControls.PC.Jump.started -= OnJumpPressed;
            _gameControls.PC.TogglePause.started -= OnTogglePausePressed;
            _gameControls.PC.Shoot.performed -= OnShootPerformed;

            _gameControls.Mobile.Jump.started -= OnJumpPressed;
            _gameControls.Mobile.TogglePause.started -= OnTogglePausePressed;
            _gameControls.Mobile.Shoot.performed -= OnShootPerformed;
        }

        public void OnPause()
        {
            _isPaused = true;
            _gameControls.PC.Movement.Disable();
            _gameControls.PC.Jump.Disable();

            _gameControls.Mobile.Movement.Disable();
            _gameControls.Mobile.Jump.Disable();
        }

        public void OnResume()
        {
            _isPaused = false;
            _gameControls.PC.Movement.Enable();
            _gameControls.PC.Jump.Enable();

            _gameControls.Mobile.Movement.Enable();
            _gameControls.Mobile.Jump.Enable();
        }

        public Vector2 GetAimDirection()
        {
            if(!PlatformDetector.IsMobile())
            {
                return (_gameControls.PC.AimPosition.ReadValue<Vector2>() - new Vector2(Screen.width/2.0f, Screen.height/2.0f)).normalized;
            }

            return _gameControls.Mobile.AimInput.ReadValue<Vector2>().normalized;
        }

        public float GetMoveInput()
        {
            if(_isPaused)
            {
                return 0.0f;
            }
            float moveInputX = !PlatformDetector.IsMobile() ? 
                                _gameControls.PC.Movement.ReadValue<Vector2>().x :
                                _gameControls.Mobile.Movement.ReadValue<Vector2>().x;
            return moveInputX;
        }

        public bool GetJumpHeldInput()
        {
            if(_isPaused)
            {
                return false;
            }

            bool jumpHeld = !PlatformDetector.IsMobile() ? 
                             _gameControls.PC.Jump.IsPressed() :
                             _gameControls.Mobile.Jump.IsPressed();
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

        private void OnShootPerformed(InputAction.CallbackContext context)
        {
            OnShoot?.Invoke();
        }
    }
}
