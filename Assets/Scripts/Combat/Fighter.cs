using System;
using System.Collections.Generic;
using ExternalScript;
using RPG.Attributes;
using RPG.Core;
using UnityEngine;
using RPG.Movement;
using RPG.Saving;
using RPG.Stats;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction, ISaveable, IModifierProvider
    {
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform rightHandTransform=null;
        [SerializeField] Transform leftHandTransform=null;
        [SerializeField] Weapon defaultWeapon=null;
        
        Health _target;
        float _timeSinceLastAttack = Mathf.Infinity;
        LazyValue<Weapon> currentWeapon;

        private void Awake()
        {
            currentWeapon=new LazyValue<Weapon>(SetupDefaultWeapon);
        }

        private Weapon SetupDefaultWeapon()
        {
            AttachWeapon(defaultWeapon);
            return defaultWeapon;
        }

        public void Start()
        {
            currentWeapon.ForceInit();
        }

        private void Update()
        {
            _timeSinceLastAttack += Time.deltaTime;
            if (_target == null)  return;
            if(_target.IsDead()) return;
            if (!IsInRange())
            {
                GetComponent<Move>().MoveTo(_target.transform.position, 1f);
            }
            else
            {
                GetComponent<Move>().Cancel();
                AttackBehaviour();
            }
        }

        public void EquipWeapon(Weapon weapon)
        {
            currentWeapon.value = weapon;
            AttachWeapon(weapon);
        }

        private void AttachWeapon(Weapon weapon)
        {
            Animator animator = GetComponent<Animator>();
            weapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        public Health GetTarget()
        {
            return _target;
        }
         
        private void AttackBehaviour()
        {
            transform.LookAt(_target.transform);
            if (_timeSinceLastAttack > timeBetweenAttacks)
            {
                //triggers the Hit() event
                TriggerAttack();
                _timeSinceLastAttack = 0;
            }
        }

        private void TriggerAttack()
        {
            GetComponent<Animator>().ResetTrigger("stopAttack");
            GetComponent<Animator>().SetTrigger("attack");
        }
        
        void Hit()
        {
            if (_target == null)
            {
                return;
            }
            
            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);
            if (currentWeapon.value.HasProjectile())
            {
                currentWeapon.value.LaunchProjectile(rightHandTransform, leftHandTransform, _target, gameObject,damage);
            }
            else
            {
                _target.TakeDamage(gameObject,damage);
            }
        }
        
        void Shoot()
        {
            Hit();
        }

        private bool IsInRange()
        {
            return Vector3.Distance(transform.position, _target.transform.position) < currentWeapon.value.GetRange();
        }

        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null)
            {
                return false;
            }
            Health targetToTest=combatTarget.GetComponent<Health>();
            return targetToTest!=null && !targetToTest.IsDead();
        }

        public void Attack(GameObject combatTarget)
        {
            GetComponent<ActionScheduler>().StartAction(this);
            _target =  combatTarget.GetComponent<Health>();
        }

        public void Cancel()
        {
            StopAttack();
            _target = null;
            GetComponent<Move>().Cancel();
        }

        private void StopAttack()
        {
            GetComponent<Animator>().ResetTrigger("attack");
            GetComponent<Animator>().SetTrigger("stopAttack");
        }
        
        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if(stat==Stat.Damage)
            {
                yield return currentWeapon.value.GetDamage();
            }
        }

        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if(stat==Stat.Damage)
            {
                yield return currentWeapon.value.GetPercentageBonus();
            }
        }

        public object CaptureState()
        {
            return currentWeapon.value.name;
        }

        public void RestoreState(object state)
        {
            string weaponName = (string)state;
            Weapon weapon = Resources.Load<Weapon>(weaponName);
            if (weapon != null) 
            {
                EquipWeapon(weapon);
            }
        }
    }
}