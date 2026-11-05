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

        [SerializeField] private Canvas _screenBlurCanvas;
        [SerializeField] private Material _blurMaterial;
        
        [Min(0.1f)]
        [SerializeField] private float _minBlurScaleAmount = 0.5f;
        [Min(0.1f)]
        [SerializeField] private float _maxBlurScaleAmount = 2.0f;
        
        [Min(0.1f)]
        [SerializeField] private float _returnToClearScreenTime = 0.2f;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float _speedPercentDuringExplosion = 0.4f;
        private Health _health;
        private CinemachineImpulseSource _impulseSource;
        private PlayerMovement _movement;
        private Coroutine _explodeCoroutine;
        private int _scaleID = 0;
        private void Awake()
        {
            _health = GetComponent<Health>();
            _impulseSource = GetComponent<CinemachineImpulseSource>();
            _movement = GetComponent<PlayerMovement>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _scaleID = Shader.PropertyToID("_Scale");
            _blurMaterial.SetFloat(_scaleID, _minBlurScaleAmount);
            DisableBlurCanvas();
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
            Sequence sequence = DOTween.Sequence();
            sequence.AppendCallback(EnableBlurCanvas);
            sequence.Join(DOTween.To(OnExplosion, 1.0f, 0.0f, _totalExplosionShakeEffectTime));
            sequence.Append(DOTween.To(AfterExplosion, 0.0f, 1.0f, _returnToClearScreenTime));
            sequence.AppendCallback(SetSpeedPercentBackTo1);
            sequence.AppendCallback(DisableBlurCanvas);

            sequence.SetUpdate(false);
            yield return sequence.WaitForCompletion();
            _explodeCoroutine = null;
        }

        private void EnableBlurCanvas()
        {
            _screenBlurCanvas.gameObject.SetActive(true);
        }

        private void DisableBlurCanvas()
        {
            _screenBlurCanvas.gameObject.SetActive(false);
        }

        private void OnExplosion(float value)
        {
            _movement.SpeedPercent = _speedPercentDuringExplosion;
            _impulseSource.ImpulseDefinition.ImpulseDuration = Mathf.Max(value * _deltaExplosionShakeEffectTime, 0.02f);
            _impulseSource.GenerateImpulse(Random.insideUnitCircle * _explosionForce);

            _blurMaterial.SetFloat(_scaleID, Mathf.Lerp(_minBlurScaleAmount, _maxBlurScaleAmount, 1.0f - value));
        }

        private void AfterExplosion(float value)
        {
            _blurMaterial.SetFloat(_scaleID, Mathf.Lerp(_maxBlurScaleAmount, _minBlurScaleAmount, value));
        }

        private void SetSpeedPercentBackTo1()
        {
            _movement.SpeedPercent = 1.0f;
        }
    }

}
