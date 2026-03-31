using UnityEngine;

namespace Helpers
{
    public class Instantiator : MonoBehaviour
    {
        public void Instantiate(GameObject prefab) => GameObject.Instantiate(prefab, transform.position, transform.rotation);
    }
}