#region Headers
using UnityEngine;
using Utilities.IOC;
using Utilities.PauseHandling;
#endregion

namespace Game.WeaponHandling
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class Bullet : MonoBehaviour, IPausable
    {
        #region Serialized Fields
        [Header("Reflect Direction Settings:")]
        [Min(0.1f)]
        [SerializeField] private float _bulletReflectCheckDistance = 0.5f;
        [SerializeField] private LayerMask _bulletReflectCheckMask;
        [Min(2)]
        [SerializeField] private int _maxBulletReflectHits = 5;
        [Header("Damage Settings:")]
        [SerializeField] private LayerMask _damageLayerMask;
        [Min(0.1f)]
        [SerializeField] private float _damage = 10.0f;
        [Header("Destroy Settings:")]
        [SerializeField] private float _destroyTime = 2.0f;
        [Min(0.1f)]
        
        [SerializeField] private float _moveSpeed = 2.0f;
        
        #endregion

        #region Private Fields
        private Rigidbody2D _bulletRB;
        private BoxCollider2D _bulletCollider;
        private float _elapsedTime = 0.0f;
        private bool _isPaused = false;
        private int _currentBulletReflectHits = 0;
        private BulletPoolManager _bulletPoolManager;

        private Vector2 _currentPosition = Vector2.zero;

        private int _prefabID = -1;
        #endregion

        #region Properties
        public BulletPoolManager BulletPoolManager { get => _bulletPoolManager; set => _bulletPoolManager = value; }
        public int PrefabID { get => _prefabID; set => _prefabID = value; }
        #endregion


        #region Unity Methods
        private void Awake()
        {
            _bulletRB = GetComponent<Rigidbody2D>();
            _bulletCollider = GetComponent<BoxCollider2D>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _bulletRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _bulletRB.bodyType = RigidbodyType2D.Kinematic;
            _bulletCollider.isTrigger = true;

            GamePauseManager.Register(this);
        }

        private void OnEnable()
        {
            _elapsedTime = 0.0f;
            _currentPosition = transform.position;
            _currentBulletReflectHits = 0;
        }

        // Update is called once per frame
        private void Update()
        {
            if(_isPaused)
            {
                return;
            }
        
            if(_elapsedTime >= _destroyTime)
            {
                DestroyBullet();
                return;
            }

            _elapsedTime += Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (_isPaused)
            {
                return;
            }

            Vector2 rightDir = new Vector2(transform.right.x, transform.right.y);
            _currentPosition += rightDir * _moveSpeed * Time.fixedDeltaTime;
            _bulletRB.MovePosition(_currentPosition);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            int colliderLayerMask = 1 << other.gameObject.layer;

            if((colliderLayerMask & _damageLayerMask.value) != 0)
            {
                DestroyBullet();
            }
            else if((colliderLayerMask & _bulletReflectCheckMask.value) != 0)
            {
                Debug.Log("Detected reflection collider");
                if(_currentBulletReflectHits >= _maxBulletReflectHits)
                {
                    DestroyBullet();
                    return;
                }
                Vector2 rightDir = new Vector2(transform.right.x, transform.right.y);
                Vector2 checkPosition = _currentPosition;
                RaycastHit2D hit = Physics2D.Linecast(checkPosition,
                                                      checkPosition + rightDir * _bulletReflectCheckDistance,
                                                      _bulletReflectCheckMask.value);

                Vector2 reflectRightDir = Vector2.Reflect(rightDir, hit.normal);
                Debug.DrawLine(checkPosition, checkPosition + hit.normal * _bulletReflectCheckDistance, Color.red, 1.0f);
                transform.right = reflectRightDir;
                _currentBulletReflectHits++;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.right * _bulletReflectCheckDistance);
        }

        #endregion

        #region Class Functionality

        public void OnPause()
        {
            _isPaused = true;
        }

        public void OnResume()
        {
            _isPaused = false;
        }

        public void Initialize(Vector2 startPosition)
        {
            _currentPosition = startPosition;
        }

        private void DestroyBullet()
        {
            if (_bulletPoolManager != null)
            {
                _bulletPoolManager.ReturnToPool(_prefabID, this);
                return;
            }

            Destroy(gameObject);
        }

        #endregion
    }
}
