using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
    public class SingletonByName : MonoBehaviour
    {
        private static readonly Dictionary<string, SingletonByName> _instances = new();
        
        private void Awake()
        {
            var instances = FindObjectsByType<SingletonByName>(FindObjectsSortMode.None);
            foreach (var instance in instances)
            {
                if (_instances.TryAdd(instance.name, instance))
                {
                    DontDestroyOnLoad(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
}
