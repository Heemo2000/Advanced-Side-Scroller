using UnityEngine;

namespace Game.EnemyHandling
{
    public class ExplodingEnemyAnimator : MonoBehaviour
    {
        [SerializeField] private Color _colorA = Color.red;
        [SerializeField] private Color _colorB = Color.white;
        [SerializeField] private float _startFrequency = 12.0f;
        [SerializeField] private float _endFrequency = 1.5f;

        private Animator _animator;
        private SpriteRenderer _spriteRenderer;
        private ExplodingEnemy _explodingEnemy;
        private float _phase = 0.0f;
        
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _explodingEnemy = GetComponentInParent<ExplodingEnemy>();
            _explodingEnemy.OnExplosionTimerStarts += StartMakingRendererBlink;
            _explodingEnemy.OnBeforeExplode += MakeRendererBlink;
        }

        private void StartMakingRendererBlink()
        {
            _phase = 0.0f;
        }

        private void MakeRendererBlink(float remainingTime)
        {
            float t = remainingTime / _explodingEnemy.BeforeExplodeTime;
            float frequency = Mathf.Lerp(_endFrequency, _startFrequency, t);

            _phase += frequency * Time.deltaTime;

            float lerp = 0.5f * (1.0f + Mathf.Sin(_phase * Mathf.PI * 2.0f));
            _spriteRenderer.color = Color.Lerp(_colorA, _colorB, lerp);
        }

        private void OnDestroy()
        {
            _explodingEnemy.OnExplosionTimerStarts -= StartMakingRendererBlink;
            _explodingEnemy.OnBeforeExplode -= MakeRendererBlink;
        }
    }
}
