using System;
using UnityEngine;

namespace Game.CoreHandling
{
    public class Health : MonoBehaviour
    {
        #region Serialized Fields
        [Min(1.0f)]
        [SerializeField] private float _maxAmount = 100.0f;
        #endregion

        #region Properties
        public float MaxAmount { get => _maxAmount; }
        #endregion

        #region Private Fields
        private float _currentAmount = 0.0f;
        #endregion

        #region Events
        public event Action<float, float> OnHealthSet;
        public event Action<float, HealthDamageType> OnHealthDamage;
        public event Action<float> OnHealthRepaired;
        public event Action<HealthDamageType> OnDeath;
        #endregion

        #region Unity Methods
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _currentAmount = MaxAmount;
            OnHealthSet?.Invoke(_currentAmount, _maxAmount);
        }
        #endregion

        #region Class Functionality
        
        public void TakeDamage(float amount, HealthDamageType type = HealthDamageType.NormalShot)
        {
            _currentAmount -= amount;
            if(_currentAmount < 0.0f)
            {
                _currentAmount = 0.0f;
            }
            OnHealthSet?.Invoke(_currentAmount, _maxAmount);
            if(_currentAmount > 0.0f)
            {
                OnHealthDamage?.Invoke(amount, type);
            }
            else
            {
                OnDeath?.Invoke(type);
            }
        }

        public void RepairHealth(float amount)
        {
            _currentAmount += amount;
            if(_currentAmount > _maxAmount)
            {
                _currentAmount = _maxAmount;
            }

            OnHealthRepaired?.Invoke(amount);
            OnHealthSet?.Invoke(_currentAmount, _maxAmount);
        }

        #endregion

    }
}
