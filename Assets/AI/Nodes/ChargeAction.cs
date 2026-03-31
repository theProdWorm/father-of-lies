using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Charge", story: "[Agent] charges in the direction of [Target]", category: "Action/Navigation", id: "ce309e5c9599135bdf8f42632e397d22")]
public class ChargeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> ChargeDistance;
    [SerializeReference] public BlackboardVariable<float> ChargeSpeed;
    [SerializeReference] public BlackboardVariable<bool> IsCharging;
    
    protected BehaviorGraphCollisionEvents m_CollisionEvents { get; private set; }

    [CreateProperty]
    protected bool m_HasBeenProcessed;
    
    private NavMeshAgent _navMeshAgent;
    private bool _wait;
    private bool _validCollision;
    private bool _hitTarget;

    private float _chargeSpeed;

    protected override Status OnStart()
    {
        _navMeshAgent = Agent.Value.GetComponentInChildren<NavMeshAgent>();
        if (!_navMeshAgent) return Status.Failure;
        _navMeshAgent.ResetPath();
        _wait = false;
        _validCollision = false;
        _hitTarget = false;
        m_HasBeenProcessed = false;

        _chargeSpeed = ChargeSpeed.Value;
        
        if (m_CollisionEvents == null)
        {
            m_CollisionEvents = Agent.Value.GetOrAddComponent<BehaviorGraphCollisionEvents>();
        }

        m_CollisionEvents.OnCollisionEnterEvent += OnCollisionEnter;
        IsCharging.Value = true;
        return Status.Running;
    }

    private void OnCollisionEnter(GameObject other)
    {
        if (other == null)
        {
            return;
        }
        _validCollision = true;
        if (other.gameObject == Target.Value)
        {
            _hitTarget = true;
        }
    }

    private Vector3 direction;
    private Vector3 targetPosition;
    private float time;
    protected override Status OnUpdate()
    {
        if (_validCollision)
        {
            IsCharging.Value = false;
            if (_hitTarget) return Status.Success;
            else return Status.Failure;
        }
        if (!_wait)
        {
            direction = Target.Value.transform.position - Agent.Value.transform.position;
            targetPosition = Agent.Value.transform.position +
                             (direction.normalized * ChargeDistance);
            var distance = Vector3.Distance(Agent.Value.transform.position, targetPosition);
            time = distance / _chargeSpeed;
            _wait = true;
        }
        if (time > 0f)
        {
            time -= Time.deltaTime;
            _navMeshAgent.Move(direction.normalized * (Time.deltaTime * _chargeSpeed));
        }
        else
        {
            IsCharging.Value = false;
            return Status.Failure;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
        _navMeshAgent = null;
        m_CollisionEvents.OnCollisionEnterEvent -= OnCollisionEnter;
    }
}

