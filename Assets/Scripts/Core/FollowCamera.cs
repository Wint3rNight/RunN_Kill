using UnityEngine;

namespace RPG.Core
{
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;

        void LateUpdate()
        {
            if (target == null) return;
            transform.position = target.position;
            transform.LookAt(target);
        }

    }
}