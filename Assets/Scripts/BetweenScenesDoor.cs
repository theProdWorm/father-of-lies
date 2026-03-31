using UnityEngine;

public class BetweenScenesDoor : MonoBehaviour
{
    static private SceneManager _sceneManager;
    [SerializeField]
    private int _sceneToLoad;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_sceneManager == null)
            {
                _sceneManager = FindFirstObjectByType<SceneManager>();
            }
            _sceneManager.LoadScene(_sceneToLoad);
        }
    }
}
