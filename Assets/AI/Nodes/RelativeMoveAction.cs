using System;
using UnityEngine;
using Unity.Properties;
using UnityEngine.AI;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Relative Move", story: "[Agent] moves relative to [Target]", category: "Action/Navigation",
        id: "e6034d269de7d693e4e6c36aee4e104f")]
    public partial class RelativeMoveAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<float> StepDistance = new BlackboardVariable<float>(2.0f);
        [SerializeReference] public BlackboardVariable<float> DirectionAngle = new BlackboardVariable<float>(0f);
        [SerializeReference] public BlackboardVariable<bool> SucceedOnDistance = new BlackboardVariable<bool>(false);
        [SerializeReference] public BlackboardVariable<float> RefreshDistance = new BlackboardVariable<float>(-1f);
        private NavMeshAgent _navMeshAgent;

        protected override Status OnStart()
        {
            _navMeshAgent = Agent.Value.GetComponentInChildren<NavMeshAgent>();
            if (!_navMeshAgent || !_navMeshAgent.isOnNavMesh) return Status.Failure;
            if (RefreshDistance.Value < 0)
            {
                RefreshDistance.Value = _navMeshAgent.stoppingDistance;
            }
            return Status.Running;
        }

        private bool wait;

        protected override Status OnUpdate()
        {
            if (!wait)
            {
                var direction = Target.Value.transform.position - Agent.Value.transform.position;
                direction = Quaternion.Euler(0, DirectionAngle, 0) * direction;
                _navMeshAgent.SetDestination(Agent.Value.transform.position +
                                             (direction.normalized * StepDistance.Value));
                wait = true;
            }

            if (wait && _navMeshAgent.remainingDistance <= RefreshDistance.Value)
            {
                wait = false;
                if (SucceedOnDistance.Value) return Status.Success;
            }

            return Status.Running;
        }

        protected override void OnEnd()
        {
            _navMeshAgent = null;
        }
    }
}