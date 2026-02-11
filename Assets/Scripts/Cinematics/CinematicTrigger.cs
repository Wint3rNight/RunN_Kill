using UnityEngine;
using UnityEngine.Playables;
namespace RPG.Cinematics
{
    public class NewMonoBehaviourScript : MonoBehaviour
    {
        bool alreadyTriggered = false;
        private void OnTriggerEnter(Collider other)
        {
            if (!alreadyTriggered && other.gameObject.tag=="Player")
            {
                alreadyTriggered = true;
                GetComponent<PlayableDirector>().Play();
            }
        }
    }

}
