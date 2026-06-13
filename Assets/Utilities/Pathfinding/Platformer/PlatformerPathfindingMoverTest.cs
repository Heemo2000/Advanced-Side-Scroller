using UnityEngine;
using UnityEngine.InputSystem;

namespace Utilities.Pathfinding.Platformer.Test
{
    public class PlatformerPathfindingMoverTest : MonoBehaviour
    {
        [SerializeField] private PlatformerPathfindingMover _mover;

        private Camera _mainCamera;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _mainCamera = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            if(Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Vector2 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(Mouse.current.position.value);
                Debug.Log("Pressed mouse LMB at " + mouseWorldPosition);
                //_mover.ForcefullyStop();
                _mover.SetDestination(mouseWorldPosition);
                
                
            }
        }
    }
}
