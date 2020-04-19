﻿using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.CrowdSimulation.Scripts.UI
{
    public enum RayCastState
    {
        Normal,
        Building,
        SquereSelection
    }

    [RequireComponent(typeof(PlayerInput))]
    public class RayCast : MonoBehaviour
    {
        public static Vector3 CurrentMonoPoint;
        
        private static bool MonoPointUpdate { get => State == RayCastState.Building; }
        private static RayCastState State { get; set; } = RayCastState.Normal;

        public void OnSecondaryActionWithModifier(InputValue inputValue)
        {
            var value = inputValue.Get<float>();
            if (value == 1)
            {
                if (State == RayCastState.SquereSelection) return;
                State = RayCastState.SquereSelection;
                EntitySelection.OnSelectionBegin();
            }
            else
            {
                if (State != RayCastState.SquereSelection) return;
                State = RayCastState.Normal;
                EntitySelection.OnSelectionEnd();
            }
        }

        public void OnSecondaryAction()
        {
            switch (State)
            {
                case RayCastState.Normal:
                    var entity = GetRayCastEntity();
                    EntitySelection.SelectEntity(entity);
                    break;
                case RayCastState.Building:
                    BuildingBuilder.OnSecondaryAction();
                    break;
            }
        }

        public void OnPrimaryAction()
        {
            if (IsPointerOverUIObject()) return;
            
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out UnityEngine.RaycastHit hit, 500f))
            {
                return;
            }
            var goalPoint = hit.point;
            switch (State)
            {
                case RayCastState.Normal:
                    EntitySelection.SelectedSetGoalPoint(goalPoint);
                    break;
                case RayCastState.Building:
                    BuildingBuilder.OnPrimaryAction();
                    break;
            }            
        }

        public void OnCursorMove(InputValue value)
        {
            switch (State)
            {
                case RayCastState.SquereSelection:
                    EntitySelection.SetSelectionBoxFromPosition(value.Get<Vector2>());
                    break;
                case RayCastState.Building:
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (!Physics.Raycast(ray, out UnityEngine.RaycastHit hit, 500f))
                    {
                        return;
                    }
                    BuildingBuilder.OnMouseMove(hit.point);
                    break;
            }
        }
        
        public static void SetToBuilding()
        {
            State = RayCastState.Building;
        }

        public static void ResetState()
        {
            State = RayCastState.Normal;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        private static bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = Mouse.current.position.ReadValue();
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            results = results.Where((result) => !result.gameObject.tag.Equals("Ignore")).ToList();
            return results.Count > 0 || (GUIUtility.hotControl != 0);
        }

        private Entity GetRayCastEntity()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance = 1000f;
            var from = ray.origin;
            var to = ray.origin + ray.direction * distance;

            BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>();
            var collisionWOrld = buildPhysicsWorld.PhysicsWorld.CollisionWorld;

            RaycastInput input = new RaycastInput()
            {
                Start = from,
                End = to,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << 1,
                    GroupIndex = 0,
                }
            };

            Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
            if (collisionWOrld.CastRay(input, out hit))
            {
                Entity hitEntity = buildPhysicsWorld.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
                return hitEntity;
            }
            else
            {
                return Entity.Null;
            }
        }
    }
}
