using System.Linq;
using UnityEngine;

namespace Effects
{
    public class IceBlock : MonoBehaviour
    {
        public void Shatter()
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.DetachChildren();
            Destroy(gameObject);
        }
    }
}
