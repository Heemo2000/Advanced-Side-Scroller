using UnityEngine;
using Unity.Cinemachine;

namespace Game.WeaponHandling
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class WeaponScreenShake : MonoBehaviour
    {
        public enum ShootType
        {
            None = -1,
            Single,
            Continous
        }

        [SerializeField] private ShootType _shootType = ShootType.None;

        private CinemachineImpulseSource _impulseSource;
        private Weapon _weapon;
        private void Awake()
        {
            _impulseSource = GetComponent<CinemachineImpulseSource>();
            _weapon = GetComponent<Weapon>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            switch (_shootType)
            {
                case ShootType.Single:
                    _weapon.OnSingleUse += DoScreenShake;
                    break;
                
                case ShootType.Continous:
                    _weapon.OnContinousUse += DoScreenShake;
                    break;
            }
        }

        private void DoScreenShake()
        {
            _impulseSource.GenerateImpulse();
        }
    }
}
