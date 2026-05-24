using UnityEngine;
using Game.InputHandling;
using UnityEngine.InputSystem;
using System;

namespace Game.PlayerHandling
{
    public class Player : MonoBehaviour
    {
        private GameControls _gameControls;
        private PlayerMovement _movement;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _gameControls = new GameControls();
            _gameControls.Enable();
            _gameControls.GameActionMap.Jump.started += OnJump;
        }

        private void Update()
        {
            float moveInputX = _gameControls.GameActionMap.Movement.ReadValue<Vector2>().x;
            _movement.HandleInputX(moveInputX, false);
        }

        private void OnDestroy()
        {
            _gameControls.Disable();
            _gameControls.GameActionMap.Jump.started -= OnJump;
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            Debug.Log("Jump pressed");
            _movement.Jump();
        }
    }
}
