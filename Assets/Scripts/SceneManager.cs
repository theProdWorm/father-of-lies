using Entities;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    static Player PLAYER;
    public UnityEvent OnSceneLoaded;

    private Fading _fade;

    private void Awake()
    {
        _fade = FindFirstObjectByType<Fading>();
        if (!_fade) Debug.LogWarning("No fade found");
    }

    private void Start()
    {
        PLAYER = FindFirstObjectByType<Player>();
        if (PLAYER)
        {
            OnSceneLoaded.AddListener(PLAYER.SetDashing);
            PLAYER.OnDeath.AddListener(ReloadScene);
        }
    }

    public void ReloadScene(Entity Why)
    {
        Scene sceneLoaded = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        LoadScene(sceneLoaded.buildIndex);
    }

    public void LoadScene(int sceneIndex)
    {
        OnSceneLoaded.Invoke();
        if (!_fade)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
            return;
        }
        ProgressPersistence _progressPersistence = FindFirstObjectByType<ProgressPersistence>();
        if(_progressPersistence != null)
            StartCoroutine(AfterFade(_fade.FadeIn(_progressPersistence.JustDied), sceneIndex, false));
        else
            StartCoroutine(AfterFade(_fade.FadeIn(false), sceneIndex, false));

    }

    public void LoadScene(string sceneName)
    {
        OnSceneLoaded.Invoke();
        var temp = UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath(sceneName);
        LoadScene(temp);
    }

    public void LoadSceneAsync(int sceneIndex)
    {
        OnSceneLoaded.Invoke();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex);
    }

    public void LoadSceneAsync(string sceneName)
    {
        OnSceneLoaded.Invoke();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
    }

    public void LoadSceneAdditive(int sceneIndex)
    {
        OnSceneLoaded.Invoke();
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex, LoadSceneMode.Additive);
    }

    public void LoadSceneAdditive(string sceneName)
    {
        OnSceneLoaded.Invoke();
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public void LoadSceneAdditiveAsync(int sceneIndex)
    {
        OnSceneLoaded.Invoke();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
    }

    public void LoadSceneAdditiveAsync(string sceneName)
    {
        OnSceneLoaded.Invoke();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    public void UnloadSceneAsync(int sceneIndex)
    {
        OnSceneLoaded.Invoke();
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneIndex);
    }

    public void UnloadSceneAsync(string sceneName)
    {
        OnSceneLoaded.Invoke();
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
    }

    public void QuitGame() => Application.Quit();

    private IEnumerator AfterFade(IEnumerator func, int scene, bool quit)
    {
        if (_fade != null)
            yield return StartCoroutine(func);

        if (quit)
            Application.Quit();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
        
    }
}
