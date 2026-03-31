using System;
using UnityEngine;
using Unity.Properties;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "SimplifiedTargetNav",
        description: "Simplified version of the default navigation node, requires a NavMeshAgent",
        story: "[Agent] moves to [Target]", category: "Action/Navigation", id: "c624932b20ef21ca8a52baebb4dc1c73")]
    public partial class SimplifiedTargetNavAction : Action
    {
        public enum TargetPositionMode
        {
            ClosestPointOnAnyCollider, // Use the closest point on any collider, including child objects
            ClosestPointOnTargetCollider, // Use the closest point on the target's own collider only
            ExactTargetPosition // Use the exact position of the target, ignoring colliders
        }

        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.2f);

        [SerializeReference]
        public BlackboardVariable<string> AnimatorSpeedParam = new BlackboardVariable<string>("SpeedMagnitude");


        [FormerlySerializedAs("m_TargetPositionMode")]
        [Tooltip("Defines how the target position is determined for navigation:" +
                 "\n- ClosestPointOnAnyCollider: Use the closest point on any collider, including child objects" +
                 "\n- ClosestPointOnTargetCollider: Use the closest point on the target's own collider only" +
                 "\n- ExactTargetPosition: Use the exact position of the target, ignoring colliders. Default if no collider is found.")]
        [SerializeReference]
        public BlackboardVariable<TargetPositionMode> targetPositionMode =
            new(TargetPositionMode.ClosestPointOnAnyCollider);

        private NavMeshAgent _navMeshAgent;
        private Animator _animator;
        private Vector3 _lastTargetPosition;
        private Vector3 _colliderAdjustedTargetPosition;
        private float _colliderOffset;

        protected override Status OnStart()
        {
            if (!Agent.Value || !Target.Value)
            {
                return Status.Failure;
            }

            return Initialize();
        }

        protected override Status OnUpdate()
        {
            if (!Agent.Value || !Target.Value)
            {
                return Status.Failure;
            }

            // Check if the target position has changed.
            bool boolUpdateTargetPosition =
                !Mathf.Approximately(_lastTargetPosition.x, Target.Value.transform.position.x)
                || !Mathf.Approximately(_lastTargetPosition.y, Target.Value.transform.position.y)
                || !Mathf.Approximately(_lastTargetPosition.z, Target.Value.transform.position.z);

            if (boolUpdateTargetPosition)
            {
                _lastTargetPosition = Target.Value.transform.position;
                _colliderAdjustedTargetPosition = GetPositionColliderAdjusted();
            }

            float distance = GetDistanceXZ();
            bool destinationReached = distance <= (DistanceThreshold + _colliderOffset);

            if (destinationReached && !_navMeshAgent.pathPending)
            {
                return Status.Success;
            }
            else if (boolUpdateTargetPosition) // navmesh-based destination update (if needed)
            {
                _navMeshAgent.SetDestination(_colliderAdjustedTargetPosition);
            }

            UpdateAnimatorSpeed();

            return Status.Running;
        }

        protected override void OnEnd()
        {
            UpdateAnimatorSpeed(0f);

            if (_navMeshAgent)
            {
                if (_navMeshAgent.isOnNavMesh)
                {
                    _navMeshAgent.ResetPath();
                }
            }

            _navMeshAgent = null;
            _animator = null;
        }

        protected override void OnDeserialize()
        {
            // If using a navigation mesh, we need to reset default value before Initialize.
            _navMeshAgent = Agent.Value.GetComponentInChildren<NavMeshAgent>();
            if (_navMeshAgent)
            {
                _navMeshAgent.Warp(Agent.Value.transform.position);
            }

            Initialize();
        }

        private Status Initialize()
        {
            _lastTargetPosition = Target.Value.transform.position;
            _colliderAdjustedTargetPosition = GetPositionColliderAdjusted();

            // Add the extents of the colliders to the stopping distance.
            _colliderOffset = 0.0f;
            Collider agentCollider = Agent.Value.GetComponentInChildren<Collider>();
            if (agentCollider)
            {
                Vector3 colliderExtents = agentCollider.bounds.extents;
                _colliderOffset += Mathf.Max(colliderExtents.x, colliderExtents.z);
            }

            if (GetDistanceXZ() <= (DistanceThreshold + _colliderOffset))
            {
                return Status.Success;
            }

            // If using a navigation mesh, set target position for navigation mesh agent.
            _navMeshAgent = Agent.Value.GetComponentInChildren<NavMeshAgent>();
            if (_navMeshAgent)
            {
                if (_navMeshAgent.isOnNavMesh)
                {
                    _navMeshAgent.ResetPath();
                }
                _navMeshAgent.SetDestination(_colliderAdjustedTargetPosition);
            }
            else return Status.Failure;

            _animator = Agent.Value.GetComponentInChildren<Animator>();
            UpdateAnimatorSpeed(0f);

            return Status.Running;
        }


        private Vector3 GetPositionColliderAdjusted()
        {
            switch (targetPositionMode.Value)
            {
                case TargetPositionMode.ClosestPointOnAnyCollider:
                    Collider anyCollider = Target.Value.GetComponentInChildren<Collider>(includeInactive: false);
                    if (!anyCollider || anyCollider.enabled == false)
                        break;
                    return anyCollider.ClosestPoint(Agent.Value.transform.position);
                case TargetPositionMode.ClosestPointOnTargetCollider:
                    Collider targetCollider = Target.Value.GetComponent<Collider>();
                    if (!targetCollider || targetCollider.enabled == false)
                        break;
                    return targetCollider.ClosestPoint(Agent.Value.transform.position);
            }

            // Default to target position.
            return Target.Value.transform.position;
        }

        private float GetDistanceXZ()
        {
            Vector3 agentPosition = new Vector3(Agent.Value.transform.position.x, _colliderAdjustedTargetPosition.y,
                Agent.Value.transform.position.z);
            return Vector3.Distance(agentPosition, _colliderAdjustedTargetPosition);
        }

        private void UpdateAnimatorSpeed(float explicitSpeed = -1)
        {
            //NavigationUtility.UpdateAnimatorSpeed(m_Animator, AnimatorSpeedParam, m_NavMeshAgent, m_CurrentSpeed, explicitSpeed: explicitSpeed);
        }
    }
}