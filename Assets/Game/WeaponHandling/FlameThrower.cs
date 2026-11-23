using UnityEngine;
using Game.SoundHandling;
using Game.WeaponHandling.FlameThrowingManagement;
using Game.CoreHandling;

namespace Game.WeaponHandling
{
    public class FlameThrower : Weapon
    {
        [Header("Particle Systems Settings:")]
        [SerializeField] private ParticleSystem _fireParticleSystem;
        [SerializeField] private ParticleSystem _smokeParticleSystem;
        [SerializeField] private ParticleSystem _sparklesParticleSystem;

        [Header("Reloading Settings:")]
        [SerializeField] private MultipleSoundPlayer _reloadingSound;

        [Header("Ammo Settings:")]
        [Min(10.0f)]
        [SerializeField] private float _totalFuelAmount = 100.0f;
        [Min(0.1f)]
        [SerializeField] private float _useSpeed = 1.0f;

        [Header("Fire and Visual Settings:")]
        [SerializeField] private float _minFirePower = 0.1f;
        [SerializeField] private float _maxFirePower = 1.0f;
        [Min(0.1f)]
        [SerializeField] private float _fireBuildupSpeed = 10.0f;
        [Min(0.01f)]
        [SerializeField] private float _minFireSpeedMultiplier = 2.0f;
        [Min(0.01f)]
        [SerializeField] private float _maxFireSpeedMultiplier = 10.0f;
        [SerializeField] private Material _fireUnlitMaterial;
        [Min(0.1f)]
        [SerializeField] private float _minFireColorIntensityMultiplier = 0.2f;
        [Min(0.1f)]
        [SerializeField] private float _maxFireColorIntensityMultiplier = 1.0f;

        [Min(20f)]
        [SerializeField] private float _minRateOverTimeMultiplier = 40.0f;
        [Min(30f)]
        [SerializeField] private float _maxRateOverTimeMultiplier = 80.0f;

        [Header("Actual Mechanic Settings")]
        [Min(2)]
        [SerializeField] private int _eachAngleSegments = 4;
        [SerializeField] private float _maxCheckRadius = 2.0f;
        [SerializeField] private int _maxTilesAffected = 1;
        [SerializeField] private LayerMask _checkLayerMask;
        [SerializeField] private Transform _firePoint;
        [Min(0.1f)]
        [SerializeField] private float _damage = 10.0f;

        private ParticleSystem.MainModule _firePSMain;
        private ParticleSystem.ShapeModule _firePSShape;
        private ParticleSystem.EmissionModule _firePSEmission;

        private ParticleSystem.MainModule _smokePSMain;
        private ParticleSystem.MainModule _sparklePSMain;
        private float _currentFuelAmount = 0.0f;
        private bool _shouldUse = false;
        private int _emissionColorID = -1;
        private Color _originalEmissionColor = Color.white;
        private float _intensityMultiplier = 0.0f;

        private RaycastHit2D _hit;
        private float _eachAngle = 0.0f;
        private float _currentAngle = 0.0f;
        private float _maxCheckDistance = 0.0f;

        private void Start()
        {
            _firePSMain = _fireParticleSystem.main;
            _firePSEmission = _fireParticleSystem.emission;
            _firePSShape = _fireParticleSystem.shape;

            _firePSMain.startLifetimeMultiplier = _minFirePower;
            _firePSMain.startSpeedMultiplier = _minFirePower * _minFireSpeedMultiplier;

            _smokePSMain = _smokeParticleSystem.main;
            _smokePSMain.startLifetimeMultiplier = _minFirePower;
            _smokePSMain.startSpeedMultiplier = _minFirePower * _minFireSpeedMultiplier;

            _sparklePSMain = _sparklesParticleSystem.main;
            _sparklePSMain.startLifetimeMultiplier = _minFirePower;
            _sparklePSMain.startSpeedMultiplier = _minFirePower * _minFireSpeedMultiplier;

            _emissionColorID = Shader.PropertyToID("_EmissionColor");

            _fireUnlitMaterial.EnableKeyword("_EMISSION");

            _originalEmissionColor = _fireUnlitMaterial.GetColor(_emissionColorID);

            _fireUnlitMaterial.SetColor(_emissionColorID, 
                                        _originalEmissionColor * 
                                        _minFireColorIntensityMultiplier);

            
            _currentFuelAmount = _totalFuelAmount;
            _reloadingSound.OnAfterAllAudiosPlayed += ActualReload;
        }


        private void FixedUpdate()
        {
            if(_shouldUse)
            {
                CheckForCatchable();
            }
            else
            {
                _maxCheckDistance = _firePSMain.startLifetime.constantMin * _firePSMain.startSpeed.constant;
            }
        }
        private void LateUpdate()
        {

            if(_shouldUse)
            {
                _intensityMultiplier = Mathf.Lerp(_intensityMultiplier, _maxFireColorIntensityMultiplier, _fireBuildupSpeed * Time.deltaTime);
                _fireUnlitMaterial.SetColor(_emissionColorID, _originalEmissionColor * _intensityMultiplier);

                
                _firePSMain.startLifetimeMultiplier = Mathf.Lerp(
                                                                 _firePSMain.startLifetimeMultiplier, 
                                                                 _maxFirePower, 
                                                                 _fireBuildupSpeed * Time.deltaTime);

                _firePSEmission.rateOverTimeMultiplier = Mathf.Lerp(_firePSEmission.rateOverTimeMultiplier, _maxRateOverTimeMultiplier, _fireBuildupSpeed * Time.deltaTime);

                _firePSMain.startSpeedMultiplier = Mathf.Lerp(
                                                                 _firePSMain.startSpeedMultiplier,
                                                                 _maxFirePower * _maxFireSpeedMultiplier,
                                                                 _fireBuildupSpeed * Time.deltaTime);

                _smokePSMain.startLifetimeMultiplier = Mathf.Lerp(
                                                                  _smokePSMain.startLifetimeMultiplier,
                                                                  _maxFirePower,
                                                                  _fireBuildupSpeed * Time.deltaTime);

                _smokePSMain.startSpeedMultiplier = Mathf.Lerp(
                                                                 _smokePSMain.startSpeedMultiplier,
                                                                 _maxFirePower * _maxFireSpeedMultiplier,
                                                                 _fireBuildupSpeed * Time.deltaTime);

                _sparklePSMain.startLifetimeMultiplier = Mathf.Lerp(
                                                                    _sparklePSMain.startLifetimeMultiplier,
                                                                    _maxFirePower,
                                                                    _fireBuildupSpeed * Time.deltaTime);

                _sparklePSMain.startSpeedMultiplier = Mathf.Lerp(
                                                                 _sparklePSMain.startSpeedMultiplier,
                                                                 _maxFirePower * _maxFireSpeedMultiplier,
                                                                 _fireBuildupSpeed * Time.deltaTime);
            }
            else
            {
                _intensityMultiplier = Mathf.Lerp(_intensityMultiplier, _minFireColorIntensityMultiplier, _fireBuildupSpeed * Time.deltaTime);
                _fireUnlitMaterial.SetColor(_emissionColorID, _originalEmissionColor * _intensityMultiplier);

                _firePSMain.startLifetimeMultiplier = Mathf.Lerp(
                                                                 _firePSMain.startLifetimeMultiplier,
                                                                 _minFirePower,
                                                                 _fireBuildupSpeed * Time.deltaTime);

                _firePSEmission.rateOverTimeMultiplier = Mathf.Lerp(_firePSEmission.rateOverTimeMultiplier, _minRateOverTimeMultiplier, _fireBuildupSpeed * Time.deltaTime);

                _firePSMain.startSpeedMultiplier = Mathf.Lerp(
                                                                 _firePSMain.startSpeedMultiplier,
                                                                 _minFirePower * _minFireSpeedMultiplier,
                                                                 _fireBuildupSpeed * Time.deltaTime);

                _smokePSMain.startLifetimeMultiplier = Mathf.Lerp(
                                                                  _smokePSMain.startLifetimeMultiplier,
                                                                  _minFirePower,
                                                                  _fireBuildupSpeed * Time.deltaTime);

                _smokePSMain.startSpeedMultiplier = Mathf.Lerp(
                                                                 _smokePSMain.startSpeedMultiplier,
                                                                 _minFirePower * _minFireSpeedMultiplier,
                                                                 _fireBuildupSpeed * Time.deltaTime);

                _sparklePSMain.startLifetimeMultiplier = Mathf.Lerp(
                                                                    _sparklePSMain.startLifetimeMultiplier,
                                                                    _minFirePower,
                                                                    _fireBuildupSpeed * Time.deltaTime);

                _sparklePSMain.startSpeedMultiplier = Mathf.Lerp(
                                                                 _sparklePSMain.startSpeedMultiplier,
                                                                 _minFirePower * _minFireSpeedMultiplier,
                                                                 _fireBuildupSpeed * Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            _fireUnlitMaterial.SetColor(_emissionColorID, _originalEmissionColor);
            _reloadingSound.OnAfterAllAudiosPlayed -= ActualReload;
        }

        private void OnDrawGizmosSelected()
        {
            float currentAngle = 0.0f;
            for(int i = 0; i < _eachAngleSegments; i++)
            {
                Quaternion angleRotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
                Gizmos.DrawLine(_firePoint.position, _firePoint.position + angleRotation * _firePoint.right * _maxCheckDistance);
                currentAngle += _eachAngle;
            }
        }

        public override void Reload()
        {
            _reloadingSound.Play();
        }

        public override void SingleUse()
        {
            _shouldUse = true;
        }

        public override void LeaveTrigger()
        {
            base.LeaveTrigger();
            _shouldUse = false;
        }

        private void ActualReload()
        {
            _currentFuelAmount = _totalFuelAmount;
        }

        private void CheckForCatchable()
        {
            _currentAngle = 0.0f;
            _eachAngle = _firePSShape.angle / (float)_eachAngleSegments;
            _maxCheckDistance = _firePSMain.startLifetime.constantMin * _firePSMain.startSpeed.constant;

            for(int i = 0; i < _eachAngleSegments; i++)
            {
                Quaternion angleRotation = Quaternion.AngleAxis(_currentAngle, Vector3.forward);
                _hit = Physics2D.CircleCast(_firePoint.position, _maxCheckRadius, angleRotation * _firePoint.right, _maxCheckDistance, _checkLayerMask.value);
                if (_hit.collider != null)
                {
                    //Debug.DrawLine(_hit.point, _hit.point + _hit.normal, Color.red);
                    if(_hit.transform.TryGetComponent<Health>(out Health health))
                    {
                        health.TakeDamage(_damage, _hit.point, _hit.normal, HealthDamageType.Fire);
                    }
                    
                    if (_hit.transform.TryGetComponent<IFlameCatchable>(out var catchable))
                    {
                        catchable.SpawnFire(_firePoint.position + angleRotation * _firePoint.right * _maxCheckDistance, _hit.point, _hit.normal, _maxTilesAffected);
                    }
                }
                _currentAngle += _eachAngle;
            }
            
        }
    }
}
