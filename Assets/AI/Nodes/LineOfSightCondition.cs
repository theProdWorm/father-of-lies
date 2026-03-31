using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using Unity.Mathematics;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "LineOfSight", story: "[Target] [Is] in the line (width: [Width]) of sight of [Agent] using [Layers]", category: "Conditions", id: "29751955a1c3ed83ae8c2f4f05a5d3d6")]
public partial class LineOfSightCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<bool> Is;
    [SerializeReference] public BlackboardVariable<List<string>> Layers;
    [SerializeReference] public BlackboardVariable<float> Width;
    
    private Transform agentTransform;
    private Transform targetTransform;
    private LayerMask blockingMask;

    public override bool IsTrue()
    {
        if (Is.Value)
        {
            if (Width.Value > 0) return CheckWideLineOfSight();
            return CheckLineOfSight();
        }
        else
        {
            if (Width.Value > 0) return !CheckWideLineOfSight();
            return !CheckLineOfSight();
        }
    }

    public override void OnStart()
    {
        agentTransform = Agent.Value.transform;
        targetTransform = Target.Value.transform;
        foreach (var layer in Layers.Value.Select(LayerMask.NameToLayer))
        {
            blockingMask.value = blockingMask.value | 1 << layer;
        }
    }
    
    private bool CheckLineOfSight()
    {
        Ray ray = new Ray(agentTransform.position, targetTransform.position - agentTransform.position);
        
        if (Physics.Raycast(ray, out RaycastHit hit,
                math.distance(agentTransform.position, targetTransform.position) + .5f, blockingMask))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool CheckWideLineOfSight()
    {
        Vector3 origin1 = agentTransform.position;
        origin1.x -= Width.Value / 2;
        Vector3 origin2 = targetTransform.position;
        origin2.x += Width.Value / 2;
        Ray ray1 = new Ray(origin1, targetTransform.position - agentTransform.position);
        Ray ray2 = new Ray(origin1, targetTransform.position - agentTransform.position);
        
        if (Physics.Raycast(ray1, out RaycastHit _, math.distance(agentTransform.position, targetTransform.position) + .5f, blockingMask)
            && Physics.Raycast(ray2, out RaycastHit _, math.distance(agentTransform.position, targetTransform.position) + .5f, blockingMask))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public override void OnEnd()
    {
    }
}
