using GameManager;
using UnityEngine;

namespace Entities.Player
{
    public class PlayerController : MonoBehaviour
    {
        private bool _isFenrir;
        
        private void Start()
        {
            SceneManager.OnSceneLoaded.AddListener(() =>
            {
                StatsPersistence.IsFenrir = _isFenrir;
            });
        }
    }
}