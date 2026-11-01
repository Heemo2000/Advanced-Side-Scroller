using Game.CoreHandling;
using UnityEngine;
using UnityHFSM;
namespace Game.EnemyHandling
{
    public class NonExplodingEnemySuspiciousState : StateBase
    {
        private readonly NonExplodingEnemy _enemy;
        private float _currentTime = 0.0f;
        private Vector2 _suspiciousCheckPosition = Vector2.zero;
        private bool _shouldCheck = false;
        public NonExplodingEnemySuspiciousState(NonExplodingEnemy enemy) : base(false, false)
        {
            _enemy = enemy;
        }

        public override void OnEnter()
        {
            _enemy.Health.OnHealthDamage += ResetSuspiciousTimer;
            

            _currentTime = 0.0f;
            _shouldCheck = false;

            _enemy.Mover.OnReachingDestination += OnReachingDestination;

            Vector2 randomDirection = Quaternion.AngleAxis(Random.Range(0.0f, 180.0f), Vector3.forward) * Vector3.right;
            _suspiciousCheckPosition = _enemy.HitPosition + randomDirection * Random.Range(_enemy.MinSuspicionCheckDistance, _enemy.MaxSuspicionCheckDistance);
            _enemy.Mover.SetDestination(_suspiciousCheckPosition);
        }

        public override void OnLogic()
        {
            if(!_shouldCheck)
            {
                return;
            }
            if(_currentTime < _enemy.SuspicionCheckTime)
            {
                _currentTime += Time.deltaTime;
                return;
            }

            _currentTime = 0.0f;
            _enemy.ShouldBecomeSuspicious = false;
        }

        public override void OnExit()
        {
            _enemy.Health.OnHealthDamage -= ResetSuspiciousTimer;
            _enemy.Mover.OnReachingDestination -= OnReachingDestination;

            _enemy.ShouldBecomeSuspicious = false;
        }

        private void ResetSuspiciousTimer(float amount, Vector2 hitPosition, Vector2 hitDirection, HealthDamageType damageType)
        {
            _currentTime = 0.0f;
            _enemy.ShouldBecomeSuspicious = true;
            _enemy.HitPosition = hitPosition;
        }

        private void OnReachingDestination()
        {
            _shouldCheck = true;
        }
    }
}
