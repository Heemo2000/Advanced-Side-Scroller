using System;
using UnityEngine;

namespace Game.WeaponHandling.FlameThrowingManagement
{
    public class FlameTile : MonoBehaviour
    {
        private static string FireColorName = "_FireColor";
        [SerializeField] private AnimationCurve _burnCurve;
        private Material _fireMaterial;
        private SpriteRenderer _renderer;
        private float _burnTime;
        private float _currentElapsedTime;
        private float _maxFirePercent;
        private Action _onMaxFireReached;
        public float BurnTime { get => _burnTime; }
        public float CurrentElapsedTime { get => _currentElapsedTime; }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }

        private void OnDestroy()
        {
            Destroy(_fireMaterial);
            _renderer.material = null;    
        }

        public void Setup(float burnTime, float maxFirePercent, AnimationCurve burnCurve, Material fireMaterial, Action OnMaxFireReached = null)
        {
            _burnTime = burnTime;
            if(_fireMaterial == null)
            {
                _fireMaterial = Material.Instantiate(fireMaterial);
                _renderer.material = _fireMaterial;
            }
            _maxFirePercent = maxFirePercent;
            _burnCurve.SetKeys(burnCurve.keys);
            _onMaxFireReached = OnMaxFireReached;
            _currentElapsedTime = 0.0f;
            
            Color fireColor = _fireMaterial.GetColor(FireColorName);
            fireColor.a = 1.0f;
            _fireMaterial.SetColor(FireColorName, fireColor);
        }

        public void ModifyFire(float timeToReducePercent)
        {
            _currentElapsedTime = Mathf.Min(0.0f, _currentElapsedTime - _currentElapsedTime * timeToReducePercent);
            float currentFirePercent = _currentElapsedTime / _burnTime;
            if(currentFirePercent > _maxFirePercent)
            {
                return;
            }

            Color fireColor = _fireMaterial.GetColor(FireColorName);
            fireColor.a = _burnCurve.Evaluate(currentFirePercent);

            _fireMaterial.SetColor(FireColorName, fireColor);
        }

        public void UpdateFlame()
        {
            _currentElapsedTime += Time.deltaTime;
            float normalized = _currentElapsedTime / _burnTime;

            Color fireColor = _fireMaterial.GetColor(FireColorName);
            fireColor.a = _burnCurve.Evaluate(normalized);
            if(fireColor.a >= 1.0f)
            {
                _onMaxFireReached?.Invoke();
            }
            _fireMaterial.SetColor(FireColorName, fireColor);
        }
    }
}
