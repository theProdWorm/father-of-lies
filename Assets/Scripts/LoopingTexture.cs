using System.Collections.Generic;
using UnityEngine;

public class LoopingTexture : MonoBehaviour
{
    private enum Direction
    {
        Left,
        Right
    };
    
    [SerializeField] private GameObject _prefab;
    
    [SerializeField] private float _speed;
    [SerializeField] private Direction _direction;

    [SerializeField] private int _width;

    private List<GameObject> _textureObjects;

    private void Start()
    {
        _textureObjects = new List<GameObject>();

        for (int i = 0; i < 2; i++)
        {
            Vector3 pos = transform.position + _width * i * (_direction == Direction.Left ? Vector3.left : Vector3.right);
            
            var obj = Instantiate(_prefab, pos, Quaternion.identity, transform);
            _textureObjects.Add(obj);
        }
    }
    
    private void Update()
    {
        for (int i = 0; i < _textureObjects.Count; i++)
        {
            float moveAmount = _speed * Time.deltaTime;
            Vector3 dir = _direction == Direction.Left ? Vector3.left : Vector3.right;
            
            Vector3 moveVector = dir * moveAmount;
            
            _textureObjects[i].transform.Translate(moveVector);

            if ((_direction == Direction.Left && _textureObjects[i].transform.position.x < -_width) ||
                (_direction == Direction.Right && _textureObjects[i].transform.position.x > _width))
            {
                Vector3 jump = _width * 2 * (_direction == Direction.Left ? Vector3.right : Vector3.left);

                _textureObjects[i].transform.position += jump;
            }
        }
    }
}
