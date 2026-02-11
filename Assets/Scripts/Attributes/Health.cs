using System;
using ExternalScript;
using RPG.Core;
using RPG.Saving;
using RPG.Stats;
using UnityEngine;

namespace RPG.Attributes
{ 
    public class Health: MonoBehaviour, ISaveable
    {
        [SerializeField] float regenerationPercentage = 70;
        
        LazyValue<float> _healthPoints;
        
        bool _isDead = false;

        private void Awake()
        {
            _healthPoints = new LazyValue<float>(GetInitialHealth);
        }

        private float GetInitialHealth()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }
        private void Start()
        {
            _healthPoints.ForceInit();
        }

        private void OnEnable()
        {
            GetComponent<BaseStats>().OnLevelUp += RegenerateHealth;
        }

        private void OnDisable()
        {
            GetComponent<BaseStats>().OnLevelUp -= RegenerateHealth;
        }
        
        public bool IsDead()
        {
            return _isDead;
        }
        
        public void TakeDamage(GameObject instigator, float damage)
        {
            print(gameObject.name + " takes " + damage + " damage");
            
            _healthPoints.value = Mathf.Max(_healthPoints.value - damage, 0);
            if (_healthPoints.value == 0)
            {
                Die();
                AwardXP(instigator);
            }
        }
        
        public float GetHealthPoints()
        {
            return _healthPoints.value;
        }

        public float GetMaxHealth()
        {
            return GetComponent<BaseStats>().GetStat(Stat.Health);
        }
        
        public float GetPercent()
        {
            return 100*(_healthPoints.value / GetComponent<BaseStats>().GetStat(Stat.Health));
        }

        private void Die()
        {
            if (_isDead)  return;
            _isDead = true;
            GetComponent<Animator>().SetTrigger("die");
            GetComponent<ActionScheduler>().CancelCurrentAction();
            GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
            GetComponent<CapsuleCollider>().isTrigger = true;
        }

        private void AwardXP(GameObject instigator)
        {
            XP xp = instigator.GetComponent<XP>();
            if(xp==null) return;
            
            xp.GainXp(GetComponent<BaseStats>().GetStat(Stat.ExperienceReward));
        }
        
        private void RegenerateHealth()
        {
            float regenHealthPoint = GetComponent<BaseStats>().GetStat(Stat.Health)*(regenerationPercentage/100);
            _healthPoints.value = Mathf.Max(_healthPoints.value, regenHealthPoint);
        }

        public object CaptureState()
        {
            return _healthPoints.value;
        }

        public void RestoreState(object state)
        {
            _healthPoints.value = (float)state;
            
            if (_healthPoints.value <= 0)
            {
                Die();
            }
        }
    }
}