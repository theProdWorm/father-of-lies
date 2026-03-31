using System;
using UnityEngine;
using System.Collections.Generic;

public class SeeThroughSync : MonoBehaviour
{
    private static GameObject PLAYER;

    private int PosID = Shader.PropertyToID("_Pos");
    private int PlayerPosID = Shader.PropertyToID("_Player_Position");
    private int SizeID = Shader.PropertyToID("_Size");

    [SerializeField]
    private float _radius = 3f;
    [SerializeField]
    private List<Material> _materials = new();
    [SerializeField]
    private LayerMask _mask;
    [SerializeField]
    private float _fadeSpeed = 1f;

    private Camera _camera;
    
    void Awake()
    {
        _camera = Camera.main;
        if (!PLAYER)
            PLAYER = GameObject.FindGameObjectWithTag("Player");
    }

    float size = 0;
    void Update()
    {
        Vector2 view = _camera.WorldToViewportPoint(transform.position);
        Vector3 dir = _camera.transform.position - transform.position;
        Ray ray = new Ray(transform.position, dir.normalized);

        if (Physics.Raycast(ray, dir.magnitude, _mask))
        {
            size += Time.deltaTime * _fadeSpeed;
        }
        else
        {
            size -= Time.deltaTime * _fadeSpeed;
        }
        size = Mathf.Clamp(size, 0, _radius);


        foreach (Material mat in _materials)
        {
            mat.SetFloat(SizeID, size);
            mat.SetVector(PosID, view);
            mat.SetVector(PlayerPosID, transform.position);
        }
    }

    private void OnDestroy()
    {
        foreach(Material mat in _materials)
        {
            mat.SetFloat(SizeID, 0);
            mat.SetVector(PosID, Vector2.zero);
            mat.SetVector(PlayerPosID, Vector3.zero);
        }
    }
}
