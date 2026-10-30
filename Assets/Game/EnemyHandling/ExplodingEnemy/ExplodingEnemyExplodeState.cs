using Game.CoreHandling;
using Game.EnemyHandling;
using System;
using UnityEngine;
using UnityHFSM;

namespace Game
{
    public class ExplodingEnemyExplodeState : StateBase
    {
        #region Private Fields
        private readonly ExplodingEnemy _enemy;
        private Collider[] _detectedColliders;
        private bool _startExplodeTimer = false;
        private float _remainingTime = 0.0f;
        #endregion

        #region Class Functionalities
        public ExplodingEnemyExplodeState(ExplodingEnemy enemy) : base(false, false)
        {
            _enemy = enemy;
        }

        public override void Init()
        {
            _detectedColliders = new Collider[1];
            _startExplodeTimer = false;
        }

        public override void OnEnter()
        {
            Vector2 targetPosition = _enemy.Target.position;
            Vector2 position = _enemy.transform.position;
            Vector2 direction = (targetPosition - position).normalized;

            _enemy.Mover.SetDestination(targetPosition - direction * _enemy.ExplodeDistance * (1.0f - _enemy.MaxClosestDistancePercent));
            _enemy.Mover.OnReachingDestination += OnReachingDestination;
            _enemy.OnExplode += Explode;
        }

        

        public override void OnLogic()
        {
            if (_startExplodeTimer)
            {
                if(_remainingTime > 0.0f)
                {
                    _remainingTime -= Time.deltaTime;
                    _enemy.InvokeBeforeExplodeEvent(_remainingTime);
                }
                else
                {
                    _enemy.InvokeExplodeEvent();
                    _startExplodeTimer = false;
                }
            }
        }

        public override void OnExit()
        {
            _enemy.Mover.OnReachingDestination -= OnReachingDestination;
            _enemy.OnExplode -= Explode;
        }

        private void OnReachingDestination()
        {
            _startExplodeTimer = true;
            _remainingTime = _enemy.BeforeExplodeTime;
            _enemy.InvokeExplosionTimerStartsEvent();
        }

        private void Explode()
        {
            if(Vector3.SqrMagnitude(_enemy.Target.position - _enemy.transform.position) <= _enemy.ExplodeRange * _enemy.ExplodeRange)
            {
                if(_enemy.Target.TryGetComponent<Health>(out Health health))
                {
                    health.TakeDamage(_enemy.ExplodeDamage, _enemy.transform.position, Vector2.zero, HealthDamageType.Explode);
                }   
            }

            _enemy.Health.TakeDamage(_enemy.Health.MaxAmount, _enemy.Health.transform.position, Vector2.zero, HealthDamageType.Explode);
        }

        #endregion
    }
}
