using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using Game.CoreHandling;
using Utilities.PauseHandling;
using DG.Tweening;

namespace Game.PlayerHandling
{
    public class PlayerDamageFeel : MonoBehaviour
    {
        [Min(0.1f)]
        [SerializeField] private float _normalDamageShakeEffectTime = 0.2f;
        [Min(0.1f)]
        [SerializeField] private float _totalExplosionShakeEffectTime = 1.0f;
        [SerializeField] private float _deltaExplosionShakeEffectTime = 0.2f;
        [Min(0.1f)]
        [SerializeField] private float _explosionForce = 10.0f;
        [SerializeField] private Material _blurMaterial;
        [Min(0.1f)]
        [SerializeField] private float _maxBlurAmount = 3.0f;
        [Min(0.1f)]
        [SerializeField] private float _returnToClearScreenTime = 0.2f;
        private Health _health;
        private CinemachineImpulseSource _impulseSource;
        private Coroutine _explodeCoroutine;
        private int _blurID = 0;
        private void Awake()
        {
            _health = GetComponent<Health>();
            _impulseSource = GetComponent<CinemachineImpulseSource>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _blurID = Shader.PropertyToID("_Blur");
            _blurMaterial.SetFloat(_blurID, 0.0f);
            _health.OnHealthDamage += OnHealthDamage;
        }

        private void OnDestroy()
        {
            _health.OnHealthDamage -= OnHealthDamage;
        }

        private void OnHealthDamage(float damage, Vector2 hitPosition, Vector2 hitDirection, HealthDamageType damageType)
        {
            switch(damageType)
            {
                case HealthDamageType.NormalShot:
                    _impulseSource.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump;
                    _impulseSource.ImpulseDefinition.ImpulseDuration = _normalDamageShakeEffectTime;    
                    _impulseSource.GenerateImpulseAt(hitPosition, hitDirection);
                    break;
                
                case HealthDamageType.HeavyShot:
                    break;
                case HealthDamageType.Fire:
                    break;

                case HealthDamageType.Explode:
                    if(_explodeCoroutine == null)
                    {
                        _explodeCoroutine = StartCoroutine(ShowExplodeBehaviour());
                    }
                    break;
            }
        }

        private IEnumerator ShowExplodeBehaviour()
        {
            /*float currentTime = 0.0f;
            _impulseSource.ImpulseDefinition.ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion;
            while (currentTime <= _totalExplosionShakeEffectTime)
            {
                _impulseSource.ImpulseDefinition.ImpulseDuration = _deltaExplosionShakeEffectTime;
                _impulseSource.GenerateImpulse(Random.insideUnitCircle * _explosionForce);
                currentTime += Time.deltaTime;
                yield return null;
            }*/
            Sequence sequence = DOTween.Sequence();
            sequence.Join(DOTween.To(OnExplosion, 1.0f, 0.0f, _totalExplosionShakeEffectTime));
            sequence.Append(DOTween.To(AfterExplosion, 0.0f, 1.0f, _returnToClearScreenTime));
            sequence.SetUpdate(false);
            yield return sequence.WaitForCompletion();
            _explodeCoroutine = null;
        }

        private void OnExplosion(float value)
        {
            _impulseSource.ImpulseDefinition.ImpulseDuration = Mathf.Max(value * _deltaExplosionShakeEffectTime, 0.02f);
            _impulseSource.GenerateImpulse(Random.insideUnitCircle * _explosionForce);

            _blurMaterial.SetFloat(_blurID, Mathf.Lerp(0.0f, _maxBlurAmount, 1.0f - value));
        }

        private void AfterExplosion(float value)
        {
            _blurMaterial.SetFloat(_blurID, Mathf.Lerp(_maxBlurAmount, 0.0f, value));
        }
    }

}
