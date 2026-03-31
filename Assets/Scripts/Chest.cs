using System;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    private static readonly int Open = Animator.StringToHash("Open");

    public bool Highlighted { get; set; }
    
    public Vector3 Position { get; private set; }

    private Animator _animator;
    private ParticleSystem _particleSystem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();
        _particleSystem = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        Position = transform.position;
    }

    public void PlayParticle()
    {
        _particleSystem.Play();
    }

    public void Interacted()
    {
        _animator.SetTrigger(Open);
        gameObject.tag = "Untagged";
    }

    private void OnDrawGizmos()
    {
        if (Highlighted)
        {
            var pos = transform.position;
           Gizmos.DrawLine(pos, new Vector3(pos.x,5,pos.z)); 
        }
            
    }
}
