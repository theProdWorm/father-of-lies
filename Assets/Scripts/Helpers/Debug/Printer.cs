using UnityEngine;

namespace Helpers
{
    public class Printer : MonoBehaviour
    {
        public void Print(string message) => Debug.Log(message);
        public void Print(int message)    => Debug.Log(message);
    }
}
