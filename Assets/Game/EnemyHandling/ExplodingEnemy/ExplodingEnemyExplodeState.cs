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
                    _enemy.InvokeBeforeExplodeEvent();
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
        }

        private void Explode()
        {
            if(Physics.OverlapSphereNonAlloc(_enemy.transform.position, _enemy.ExplodeRange, _detectedColliders, 1 << _enemy.Target.gameObject.layer) > 0)
            {
                Health health = _detectedColliders[0].transform.GetComponent<Health>();
                health.TakeDamage(_enemy.ExplodeDamage, HealthDamageType.Explode);
            }
        }

        #endregion
    }
}
