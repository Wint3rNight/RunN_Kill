using System;
using RPG.Attributes;
using UnityEngine;
using RPG.Movement;
using RPG.Combat;
using RPG.Core;
using UnityEngine.EventSystems;
using UnityEngine.AI;

namespace RPG.Control
{
    public class PlayerControl : MonoBehaviour
    {
        private Move _mover;
        private Health _health;
        [System.Serializable]
        struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        [SerializeField] CursorMapping[] cursorMappings = null;
        [SerializeField] float maxNavMeshProjectionDistance = 1f;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void Start()
        {
            _mover = GetComponent<Move>();
        }

        void Update()
        {
            if (InteractWithUI())
            {
                return;
            }

            if (_health.IsDead())
            {
                SetCursor(CursorType.None);
                return;
            }

            if (InteractWithComponent())
            {
                return;
            }

            if (InteractWithMovement())
            {
                return;
            }
            SetCursor(CursorType.None);
        }

        private bool InteractWithComponent()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            foreach (RaycastHit hit in hits)
            {
                IRaycastable[] raycastables = hit.transform.GetComponents<IRaycastable>();
                foreach (IRaycastable raycastable in raycastables)
                {
                    if (raycastable.HandleRaycast(this))
                    {
                        SetCursor(raycastable.GetCursorType());
                        return true;
                    }
                }
            }
            return false;
        }

        RaycastHit[] RaycastAllSorted()
        {

            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            float[] distnaces = new float[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                distnaces[i] = hits[i].distance;
            }
            Array.Sort(distnaces, hits);
            return hits;
        }

        private bool InteractWithUI()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                SetCursor(CursorType.UI);
                return true;
            }
            return false;
        }



        private void SetCursor(CursorType type)
        {
            CursorMapping mapping = GetCursorMapping(type);
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        private CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping mapping in cursorMappings)
            {
                if (mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0];
        }

        private bool InteractWithMovement()
        {
            Vector3 target;
            bool hasHit = RaycastNavMesh(out target);
            if (hasHit)
            {
                if (Input.GetMouseButton(0))
                {
                    _mover.StartMoveAction(target, 1f);
                }
                SetCursor(CursorType.Movement);
                return true;
            }

            return false;
        }

        private bool RaycastNavMesh(out Vector3 target)
        {
            target = new Vector3();
            RaycastHit hit;
            bool hasHit = Physics.Raycast(GetMouseRay(), out hit);
            if (!hasHit)
            {
                return false;
            }

            NavMeshHit navMeshHit;
            bool hasCastToNavMesh = NavMesh.SamplePosition(hit.point, out navMeshHit, maxNavMeshProjectionDistance, NavMesh.AllAreas);
            if (!hasCastToNavMesh)
            {
                return false;
            }
            target = navMeshHit.position;
            return true;
        }

        private static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }
}
