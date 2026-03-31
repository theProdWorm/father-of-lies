using UnityEngine;

namespace UI
{
    public class LookAtCamera : MonoBehaviour
    {
        private static Camera mainCamera;
        private void LateUpdate()
        {
            if(!mainCamera) mainCamera = Camera.main;
            gameObject.transform.LookAt(mainCamera?.transform);
        }
    }
}
