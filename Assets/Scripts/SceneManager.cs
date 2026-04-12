using Entities;
using System.Collections;
using Entities.Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    private static PlayerEntity PLAYER;
    public static readonly UnityEvent OnSceneExit = new();

    private Fading _fade;

    private void Awake()
    {
        _fade = FindFirstObjectByType<Fading>();
        if (!_fade) Debug.LogWarning("No fade found");
    }

    private void Start()
    {
        PLAYER = PlayerEntity.INSTANCE;
        if (!PLAYER)
            return;
        
        OnSceneExit.AddListener(PLAYER.SetDashing);
        PLAYER.OnDeath.AddListener(ReloadScene);
    }

    public void ReloadScene(Entity Why)
    {
        Scene sceneLoaded = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        LoadScene(sceneLoaded.buildIndex);
    }

    public void LoadScene(int sceneIndex)
    {
        int currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        if (currentScene != sceneIndex)
            PlayerSpawner.ResetProgress();
        
        OnSceneExit.Invoke();
        if (!_fade)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
            return;
        }
        
        StartCoroutine(AfterFade(_fade.FadeIn(PlayerSpawner.DIED), sceneIndex, false));
    }

    public void LoadScene(string sceneName)
    {
        OnSceneExit.Invoke();
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
        LoadScene(buildIndex);
    }

    public void LoadSceneAsync(int sceneIndex)
    {
        OnSceneExit.Invoke();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex);
    }

    public void QuitGame() => Application.Quit();

    private IEnumerator AfterFade(IEnumerator func, int scene, bool quit)
    {
        if (_fade)
            yield return StartCoroutine(func);

        if (quit)
            QuitGame();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
}
