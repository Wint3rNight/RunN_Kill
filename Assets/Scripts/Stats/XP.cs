using System;
using UnityEngine;
using RPG.Saving;
 
namespace RPG.Stats
{
    public class XP : MonoBehaviour, ISaveable
    {
        [SerializeField] float experiencePoints = 0f;
        
        public event Action onExperienceGained;

        public void GainXp(float experience)
        {
            experiencePoints += experience;
            onExperienceGained();
        }
        
        public float GetXp()
        {
            return experiencePoints;
        }
        
        public object CaptureState()
        {
            return experiencePoints;
        }
        
        public void RestoreState(object state)
        {
            experiencePoints = (float)state;
        }
    }
}