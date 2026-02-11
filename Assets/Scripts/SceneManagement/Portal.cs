using System.Collections;
using RPG.Saving;
using RPG.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

namespace SceneManagement
{
    public class Portal : MonoBehaviour
    {
        enum DestinationIdentifier
        {
            A,B,C,D,E,F
        }
        [SerializeField] int sceneToLoad= -1;
        [SerializeField] Transform spawnPoint;
        [SerializeField] float fadeInTime = 2f;
        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeWaitTime = 0.5f;
        
        bool _isTransitioning=false;
        [SerializeField] DestinationIdentifier destination;
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !_isTransitioning)
            {
                StartCoroutine(Transition());
            }
        }

        private IEnumerator Transition()
        {
            if (sceneToLoad < 0)
            {
                Debug.LogError($"Scene {sceneToLoad} not found");
                yield break;   
            }
            _isTransitioning = true;
            
            DontDestroyOnLoad(gameObject);
            
            Fader fader = FindAnyObjectByType<Fader>();
            
            SavingWrapper savingWrapper = FindAnyObjectByType<SavingWrapper>();
            yield return fader.FadeOut(fadeOutTime);
            savingWrapper.Save();
            
            yield return SceneManager.LoadSceneAsync(sceneToLoad);
            
            savingWrapper.Load();
            
            Portal otherPortal = GetOtherPortal();
            UpdatePlayer(otherPortal);
            
            savingWrapper.Save();
            
            yield return new WaitForSeconds(fadeWaitTime);
            yield return fader.FadeIn(fadeInTime);
            
            Destroy(gameObject);
        }
        private void UpdatePlayer(Portal otherPortal)
        {
            if(otherPortal == null) return;
            GameObject player = GameObject.FindWithTag("Player");
            
            player.GetComponent<NavMeshAgent>().enabled = false;
            
            player.GetComponent<NavMeshAgent>().Warp(otherPortal.spawnPoint.position);
            player.transform.rotation=otherPortal.spawnPoint.rotation;
            
            player.GetComponent<NavMeshAgent>().enabled = true;
        }

        private Portal GetOtherPortal()
        {
            foreach (Portal portal in Object.FindObjectsByType<Portal>(FindObjectsSortMode.None))
            {
                if (portal == this) continue;
                if (portal.destination != this.destination) continue;
                
                return portal;
            }

            return null;
        }
    }
}