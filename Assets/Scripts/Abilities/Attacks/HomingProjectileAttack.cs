using Abilities.Attacks;
using UnityEngine;

public class HomingProjectileAttack : ProjectileAttack
{
    public Transform target;
    public float turnRate;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        
        var rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        var lerpRot = Quaternion.Lerp(transform.rotation,rotation , Time.fixedDeltaTime * turnRate);
        var rot = lerpRot.eulerAngles;
        transform.eulerAngles = new Vector3(0, rot.y, 0);
        
        _rigidbody.linearVelocity = transform.forward * _speed;
    }
}
