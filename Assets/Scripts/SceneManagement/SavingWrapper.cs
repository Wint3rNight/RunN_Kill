using System;
using System.Collections;
using System.Collections.Generic;
using RPG.Saving;
using UnityEngine;

namespace  RPG.SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        const string DefaultSaveFile = "save";
        
        [SerializeField] float fadeInTime=0.2f;

        private void Awake()
        {
            StartCoroutine(LoadLastScene());
        }

        private IEnumerator LoadLastScene()
        {
            yield return GetComponent<SavingSystem>().LoadLastScene(DefaultSaveFile);
            Fader fader = FindAnyObjectByType<Fader>();            
            fader.FadeOutImmediate();
            yield return fader.FadeIn(fadeInTime);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Save();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                Load();
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Delete();
            }
        }

        public void Load()
        {
            GetComponent<SavingSystem>().Load(DefaultSaveFile);
        }

        public void Save()
        {
            GetComponent<SavingSystem>().Save(DefaultSaveFile);
        }

        public void Delete()
        {
            GetComponent<SavingSystem>().Delete(DefaultSaveFile);
        }
    }
}