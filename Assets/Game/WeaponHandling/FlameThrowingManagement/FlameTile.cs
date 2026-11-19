using UnityEngine;

namespace Game.WeaponHandling.FlameThrowingManagement
{
    public class FlameTile : MonoBehaviour
    {
        [SerializeField] private AnimationCurve _burnCurve;
        private Material _fireMaterial;
        private SpriteRenderer _renderer;
        private float _burnTime;
        private float _currentElapsedTime;
        private int _fireColorID = -1;

        public float BurnTime { get => _burnTime; }
        public float CurrentElapsedTime { get => _currentElapsedTime; }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _fireColorID = Shader.PropertyToID("_FireColor");
        }

        private void OnDestroy()
        {
            Destroy(_fireMaterial);
            _renderer.material = null;    
        }

        public void Setup(float burnTime, Material fireMaterial)
        {
            _burnTime = burnTime;
            if(_fireMaterial == null)
            {
                _fireMaterial = Material.Instantiate(fireMaterial);
                _renderer.material = _fireMaterial;
            }
            

            _currentElapsedTime = 0.0f;
            
            Color fireColor = _fireMaterial.GetColor(_fireColorID);
            fireColor.a = 1.0f;
            _fireMaterial.SetColor(_fireColorID, fireColor);
        }

        public void UpdateFlame()
        {
            _currentElapsedTime += Time.deltaTime;
            float normalized = _currentElapsedTime / _burnTime;

            Color fireColor = _fireMaterial.GetColor(_fireColorID);
            fireColor.a = 1.0f - _burnCurve.Evaluate(normalized);

            _fireMaterial.SetColor(_fireColorID, fireColor);
        }
    }
}
