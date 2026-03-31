using UnityEngine;

public class Gateway : MonoBehaviour
{
    [SerializeField]
    private Collider _collider;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider.enabled = false;
    }

    public void Open()
    {
        _collider.enabled = false;
        _animator.SetTrigger("Open");
    }

    public void Close()
    {
        _collider.enabled = true;
        _animator.SetTrigger("Close");
    }

    private void OnValidate()
    {
        if (_collider == null)
            Debug.LogError("Collider is not assigned in the inspector for " + gameObject.name);
    }
}