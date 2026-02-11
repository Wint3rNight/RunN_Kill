using System;
using System.Collections.Generic;
using RPG.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace RPG.Saving
{
    [ExecuteAlways]
    public class SavableEntities : MonoBehaviour
    {
        [SerializeField] string uniqueIdentifier = "";
        static Dictionary<string, SavableEntities> _globalLookup = new Dictionary<string, SavableEntities>();
        
        public string GetUniqueIdentifier()
        {
            return uniqueIdentifier;
        }

        public object CaptureState()
        {
            Dictionary<string, object> state = new Dictionary<string, object>();
            foreach (ISaveable savable in GetComponents<ISaveable>())
            {
                state[savable.GetType().ToString()] = savable.CaptureState();
            }
            return state;
        }

        public void RestoreState(object state)
        {
            Dictionary<string, object> stateDict = (Dictionary<string, object>)state;
            foreach (ISaveable savable in GetComponents<ISaveable>())
            {
                string typeString = savable.GetType().ToString();
                if (stateDict.ContainsKey(typeString))
                {
                    savable.RestoreState(stateDict[typeString]);
                }
            }   
        }
        
#if UNITY_EDITOR
        public void Update()
        {
            if (Application.IsPlaying(gameObject)) return;
            if(string.IsNullOrEmpty(gameObject.scene.path))return;
            
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty property = serializedObject.FindProperty("uniqueIdentifier");
            
            if(string.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))
            {
                property.stringValue = System.Guid.NewGuid().ToString();
                serializedObject.ApplyModifiedProperties();
            }
            _globalLookup[property.stringValue] = this;
        }
#endif
        private bool IsUnique(string candidate)
        {
            if (!_globalLookup.ContainsKey(candidate))
            {
                return true;
            }

            if (_globalLookup[candidate] == this)
            {
                return true;
            }

            if (_globalLookup[candidate] == null)
            {
                _globalLookup.Remove(candidate);
                return true;
            }

            if (_globalLookup[candidate].GetUniqueIdentifier()!= candidate)
            {
                _globalLookup.Remove(candidate);
                return true;
            }
            return false;
        }
    }
}