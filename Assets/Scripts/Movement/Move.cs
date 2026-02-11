using RPG.Attributes;
using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Saving;

namespace RPG. Movement
{
    
    public class Move : MonoBehaviour, IAction, ISaveable
    {
        [SerializeField] float maxSpeed = 6f;
        
        private NavMeshAgent _agent;
        private Animator _animator;
        private Health _health;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _health = GetComponent<Health>();
        }
        
        void Update()
        {
            _agent.enabled = !_health.IsDead();
            UpdateAnimator();
        }

        public void StartMoveAction(Vector3 destination, float speedFraction)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            MoveTo(destination, speedFraction);
        }
        public void MoveTo(Vector3 destination, float speedFraction)
        {
            _agent.destination = destination;
            _agent.speed = maxSpeed * Mathf.Clamp01(speedFraction);
            _agent.isStopped = false;
        }

        public void Cancel()
        {
            _agent.isStopped = true;
        }
        private void UpdateAnimator()
        {
            Vector3 velocity=_agent.velocity;
            Vector3 localVelocity=transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;
            GetComponent<Animator>().SetFloat("forwardSpeed",speed);
        }

        public object CaptureState()
        {
            return new SerializableVector3(transform.position);
        }
        public void RestoreState(object state)
        {
            SerializableVector3 position = (SerializableVector3)state;
            _agent.enabled = false;
            transform.position = position.ToVector();
            _agent.enabled = true;
            GetComponent<ActionScheduler>().CancelCurrentAction();
        }
    }

}