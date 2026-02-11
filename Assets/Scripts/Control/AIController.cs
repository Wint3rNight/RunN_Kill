using System;
using ExternalScript;
using RPG.Attributes;
using UnityEngine;

using RPG.Combat;
using RPG.Core;
using RPG.Movement;

namespace RPG.Control
{
    public class AIController : MonoBehaviour
    {
        [SerializeField] float suspicionTime=3f;
        [SerializeField] float chaseDistance=5f;
        [SerializeField] PatrolPath patrolPath;
        [SerializeField] float waypointTolerance=1f;
        [SerializeField] float waypointDwellTime=3f;
        [Range(0,1)]
        [SerializeField] private float patrolSpeedFraction = 0.2f;
        
        Fighter _fighter;
        Health _health;
        GameObject _player;
        Move _move;
        
        LazyValue<Vector3> _guardPosi;
        float _timeSinceVision=Mathf.Infinity;
        float _timeSinceArrivedAtWaypoint = Mathf.Infinity;
        int _currentWaypointIndex=0;

        private void Awake()
        {
            _fighter=GetComponent<Fighter>();
            _health = GetComponent<Health>();
            _player = GameObject.FindWithTag("Player");
            _move=GetComponent<Move>();
            
            _guardPosi=new LazyValue<Vector3>(GetGuardPosition);
        }

        private Vector3 GetGuardPosition()
        {
            return transform.position;
        }

        private void Start()
        {
            _guardPosi.ForceInit();
        }
        public void Update()
        {
            if(_health.IsDead()) return;
            if (InAttackRangeOfPlayer() && _fighter.CanAttack(_player))
            {
                AttackBehaviour();
            }
            else if (_timeSinceVision<suspicionTime)
            {
                SusBehaviour();
            }
            else
            {
                PatrolBehaviour();
            }
            UpdateTimers();
        }

        private void UpdateTimers()
        {
            _timeSinceVision+=Time.deltaTime;
            _timeSinceArrivedAtWaypoint+=Time.deltaTime;
        }

        private void PatrolBehaviour()
        {
            Vector3 nextPosition = _guardPosi.value;
            if (patrolPath != null)
            {
                if (AtWaypoint())
                {
                    _timeSinceArrivedAtWaypoint = 0;
                    CycleWaypoint();
                }

                nextPosition = GetCurrentWaypoint();
            }

            if (_timeSinceArrivedAtWaypoint > waypointDwellTime)
            {
                _move.StartMoveAction(nextPosition, patrolSpeedFraction);
            }
        }
        private bool AtWaypoint()
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, GetCurrentWaypoint());
            return distanceToWaypoint < waypointTolerance;
        }

        private void CycleWaypoint()
        {
            _currentWaypointIndex = patrolPath.GetNextWaypoint(_currentWaypointIndex);
        }

        private Vector3 GetCurrentWaypoint()
        {
            return patrolPath.GetWaypoint(_currentWaypointIndex);
        }

        private void SusBehaviour()
        {
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }

        private void AttackBehaviour()
        {
            _timeSinceVision = 0;
            _fighter.Attack(_player);
        }

        private bool InAttackRangeOfPlayer()
        {
            float distanceToPlayer = Vector3.Distance(_player.transform.position, transform.position);
            return distanceToPlayer < chaseDistance;
        }
        
        //called by unity
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseDistance);
        }
    }
}