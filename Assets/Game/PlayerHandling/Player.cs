using UnityEngine;
using UnityEngine.InputSystem;

using Game.InputHandling;
using Game.WeaponHandling;

using Utilities.IOC;
using Utilities.PauseHandling;

namespace Game.PlayerHandling
{
    public class Player : MonoBehaviour, IPausable
    {
        [SerializeField] private GameInput _gameInput;
        
        private WeaponManager _weaponManager;
        private PlayerMovement _movement;
        private bool _jumpLonger = false;

        private bool _isPaused = false;
        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _weaponManager = GetComponent<WeaponManager>();
        }

        private void Start()
        {
            if(_gameInput == null)
            {
                _gameInput = FindFirstObjectByType<GameInput>();
            }

            _gameInput.OnJump += OnJump;
            _gameInput.OnShoot += _weaponManager.Use;

            ServiceLocator sceneServiceLocator = ServiceLocator.ForSceneOf(this);
            if (sceneServiceLocator != null)
            {
                GamePauseManager gamePauseManager = sceneServiceLocator.Get<GamePauseManager>();
                if (gamePauseManager != null)
                {
                    gamePauseManager.Register(this);
                }
                else
                {
                    GamePauseManager.RegisterStatically(this);
                }
            }
            else
            {
                GamePauseManager.RegisterStatically(this);
            }
        }

        private void Update()
        {
            if(_isPaused)
            {
                return;
            }

            float moveInputX = _gameInput.GetMoveInput();
            _movement.HandleInputX(moveInputX, false);

            bool jumpHeld = _gameInput.GetJumpHeldInput();
            _movement.SetJumpHeld(jumpHeld);

            _weaponManager.AimDirection = _gameInput.GetAimDirection();
        }

        private void OnDestroy()
        {
            if (_gameInput != null)
            {
                _gameInput.OnJump -= OnJump;
                _gameInput.OnShoot -= _weaponManager.Use;
            }
        }

        public void OnPause()
        {
            _isPaused = true;
        }

        public void OnResume()
        {
            _isPaused = false;
            
        }

        private void OnJump()
        {
            Debug.Log("Jump pressed");
            _movement.Jump();
        }

    }
}
