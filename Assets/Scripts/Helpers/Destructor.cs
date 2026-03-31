using UnityEngine;

namespace Helpers
{
    public class Destructor : MonoBehaviour
    {
        public void DestroyObject(GameObject obj) => Destroy(obj);
        public void DestroyComponent(Component component) => Destroy(component);
    }
}