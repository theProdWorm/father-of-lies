using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RotateTowards", story: "[Agent] Rotates towards [Target]", category: "Action/Transform", id: "be8ea33b9bf0b7a38bb61efc96fdb308")]
public class RotateTowardsAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed;
    [SerializeReference] public BlackboardVariable<bool> Disabled;
    

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Disabled.Value) return Status.Running;
        var transform = Agent.Value.transform;
        var rotation = Quaternion.LookRotation(Target.Value.transform.position - transform.position, Vector3.up);
        var lerpRot = Quaternion.Lerp(transform.rotation,rotation , Time.deltaTime * RotationSpeed.Value);
        var rot = lerpRot.eulerAngles;
        transform.eulerAngles = new Vector3(0, rot.y, 0);
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

