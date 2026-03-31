using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup PauseCanvasGroup;
    [SerializeField] private CanvasGroup SettingsCanvasGroup;
    private static CanvasGroup _PauseCanvasGroup;
    private static CanvasGroup _SettingsCanvasGroup;
    private static PlayerInput _playerInput;
    public static bool Paused = false;

    public static GameObject PreviousSelected;

    private void Awake()
    {
        _SettingsCanvasGroup = SettingsCanvasGroup;
        SettingsMenu.INSTANCE = _SettingsCanvasGroup.GetComponent<SettingsMenu>();
        _SettingsCanvasGroup.gameObject.SetActive(false);
        if (!PauseCanvasGroup) return;
        _PauseCanvasGroup = PauseCanvasGroup;
        _PauseCanvasGroup.gameObject.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        UnpauseGame();
    }

    private void OnDisable()
    {
        if(Paused) UnpauseGame();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public static void MenuExitInput(InputAction.CallbackContext context)
    {
        if(context.performed) MenuExit();
    }

    public static void MenuExit()
    {
        if (_SettingsCanvasGroup.gameObject.activeSelf)
        {
            _SettingsCanvasGroup.gameObject.SetActive(false);
            if (Paused)
            {
                _PauseCanvasGroup.interactable = true;
                _PauseCanvasGroup.blocksRaycasts = true;
            }
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(PreviousSelected);
        }
        else if (_PauseCanvasGroup && _PauseCanvasGroup.gameObject.activeSelf)
        {
            UnpauseGame();
        }
    }

    public static void PauseGame()
    {
        if(Paused) return;
        if (!_playerInput) _playerInput = FindFirstObjectByType<PlayerInput>();
        Time.timeScale = 0;
        _playerInput.SwitchCurrentActionMap("UI");
        Paused = true;
        _PauseCanvasGroup?.gameObject.SetActive(true);
    }

    public static void OpenSettingsMenu()
    {
        if (!_playerInput) _playerInput = FindFirstObjectByType<PlayerInput>();
        if (Paused)
        {
            _PauseCanvasGroup.interactable = false;
            _PauseCanvasGroup.blocksRaycasts = false;
        }
        _SettingsCanvasGroup.gameObject.SetActive(true);
    }

    public static void UnpauseGame()
    {
        if (!Paused) return;
        if (!_playerInput) _playerInput = FindFirstObjectByType<PlayerInput>();
        Time.timeScale = 1;
        if(_playerInput)_playerInput.SwitchCurrentActionMap("Player");
        Paused = false;
        if(_PauseCanvasGroup)_PauseCanvasGroup.gameObject.SetActive(false);
    }
    
    public void ReturnToMainMenu() => UnityEngine.SceneManagement.SceneManager.LoadScene(0);
}
