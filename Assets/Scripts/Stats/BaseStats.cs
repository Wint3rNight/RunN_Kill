using System;
using ExternalScript;
using UnityEngine;

namespace RPG.Stats{
    public class BaseStats : MonoBehaviour
    {
        [Range(1, 99)]
        [SerializeField] int startLevel = 1;
        [SerializeField] CharacterClass characterClass;
        [SerializeField] Progression progression = null;
        [SerializeField] GameObject levelUpParticleEffect = null;
        [SerializeField] bool shouldUseModifiers = false;
        
        public event Action OnLevelUp;

        private LazyValue<int> _currentLevel;
        
        XP _experience;

        private void Awake()
        {
            _experience = GetComponent<XP>();
            _currentLevel=new LazyValue<int>(CalculateLevel);
        }

        private void Start()
        {
            _currentLevel.ForceInit();
        }

        private void OnEnable()
        {
            if(_experience != null)
            {
                _experience.onExperienceGained += UpdateLevel;
            }
        }

        private void OnDisable()
        {
            if (_experience != null)
            {
                _experience.onExperienceGained -= UpdateLevel;
            }
        }

        private void UpdateLevel()
        {
            int newLevel = CalculateLevel();
            if(newLevel > _currentLevel.value)
            {
                _currentLevel.value = newLevel;
                LevelUpEffect();
                OnLevelUp();
            }
        }

        private void LevelUpEffect()
        {
            Instantiate(levelUpParticleEffect, transform);
        }

        public float GetStat(Stat stat)
        {
            return (GetBaseStat(stat)+GetAdditiveModifier(stat))*(1+GetPercentageModifier(stat)/100);
        }
        
        private float GetBaseStat(Stat stat)
        {
            return progression.GetStat(stat, characterClass, GetLevel());
        }

        public int GetLevel()
        {
            if (_currentLevel.value < 1)
            {
                _currentLevel.value = CalculateLevel();
            }
            return _currentLevel.value;
        }
        
        private float GetAdditiveModifier(Stat stat)
        {
            if (!shouldUseModifiers)
            {
                return 0;
            }
            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetAdditiveModifiers(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        private float GetPercentageModifier(Stat stat)
        {
            if (!shouldUseModifiers)
            {
                return 0;
            }
            float total = 0;
            foreach (IModifierProvider provider in GetComponents<IModifierProvider>())
            {
                foreach (float modifier in provider.GetPercentageModifiers(stat))
                {
                    total += modifier;
                }
            }
            return total;
        }

        
        private int CalculateLevel()
        {
            XP experience = GetComponent<XP>();
            if (experience == null)
            {
                return startLevel;
            }
            
            float currentXp = experience.GetXp();
            int penultimateLevel = progression.GetLevels(Stat.ExperienceToLevelUp, characterClass);
            for (int level = 1; level <= penultimateLevel; level++)
            {
                float xpToLevelUp = progression.GetStat(Stat.ExperienceToLevelUp, characterClass, level);
                if (xpToLevelUp > currentXp)
                {
                    return level;
                }
            }
            return penultimateLevel+1;
        }
    }
}