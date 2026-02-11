using System;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Stats
{
    public class XPDisplay : MonoBehaviour
    {
        XP experience;
        
        private void Awake()
        {
            experience = GameObject.FindWithTag("Player").GetComponent<XP>();
        }

        private void Update()
        {
            GetComponent<Text>().text = String.Format("{0:0}",experience.GetXp()) ;
        }
    }

}   