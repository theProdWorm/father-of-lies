using System;
using System.Collections;
using Entities;
using Entities.Player;
using Gameplay;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class FollowCamera : MonoBehaviour
{
    private enum FollowBehaviour
    {
        Ahead, Behind
    }
    
    [SerializeField] private Rigidbody _target;
    
    [Header("Position")]
    [SerializeField] private bool _lerpPosition = true;
    [SerializeField] private float _positionLerpSpeed = .1f;
    [SerializeField] private float _followOffset = 1f;
    [SerializeField] private FollowBehaviour _followBehaviour = FollowBehaviour.Ahead;
    
    [Header("Rotation")]
    [SerializeField] private float _controllerRotationSpeed = 20f;
    [SerializeField] private float _mouseRotationSpeed = 50f;
    [Tooltip("Minimum rotation value on the x-axis (pitch).")]
    [SerializeField] private float _minPitch = 30f;
    [Tooltip("Maximum rotation value on the x-axis (pitch).")]
    [SerializeField] private float _maxPitch = 80f;
    
    [Header("Screen Shake")]
    [SerializeField] private float _shakeIntensity = 1f;
    
    private float   _upwardOffset;
    private float   _backwardOffset;
    
    private Vector3 _rotationEuler;

    private Vector2 _rotateInput;
    
    private Coroutine _shakeCoroutine;
    private Vector3 _shakeOffset;

    private Coroutine _forcedLookAtCoroutine;
    private Transform _forcedTarget;

    private void Start()
    {
        _rotationEuler = transform.rotation.eulerAngles;
        
        var toTarget = transform.position - _target.position;
        var projection = Vector3.Project(toTarget, transform.forward);

        _upwardOffset = (toTarget - projection).magnitude;
        _backwardOffset = projection.magnitude;
        
        SettingsMenu.INSTANCE.ScreenShakeSlider.onValueChanged.AddListener(SetScreenShakeMultiplier);
        SettingsMenu.INSTANCE.ScreenShakeSlider.value = PlayerPrefs.HasKey("ShakeIntensity") ? PlayerPrefs.GetFloat("ShakeIntensity") : 1f;
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat("ShakeIntensity", _shakeIntensity);
    }

    private void Update()
    {
        if (MenuManager.Paused || _forcedLookAtCoroutine != null) 
            return;
        
        bool holdingRightClick = Input.GetMouseButton(1);
        Cursor.visible = !holdingRightClick;
        Cursor.lockState = holdingRightClick ? CursorLockMode.Locked : CursorLockMode.None;
        
        transform.position = _target.position;
        
        float yawDelta = _rotateInput.x * Time.deltaTime;
        yawDelta *= holdingRightClick ? _mouseRotationSpeed : _controllerRotationSpeed;
        float yaw = (_rotationEuler.y + yawDelta) % 360;
        
        float pitchDelta = -_rotateInput.y * Time.deltaTime;
        pitchDelta *= holdingRightClick ? _mouseRotationSpeed : _controllerRotationSpeed;
        float pitch = Mathf.Clamp(_rotationEuler.x + pitchDelta, _minPitch, _maxPitch);
        
        _rotationEuler = new Vector3(pitch, yaw, 0);
        
        transform.rotation = Quaternion.Euler(_rotationEuler);
        
        transform.position -= transform.forward * _backwardOffset;
        transform.position += transform.up * _upwardOffset;
        transform.position += _shakeOffset;
        
        if (holdingRightClick)
            _rotateInput = Vector2.zero;
    }
    
    public void Shake(ScreenShakeEvent shakeEvent)
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = null;
            _shakeOffset = Vector3.zero;
        }
        
        _shakeCoroutine = StartCoroutine(ShakeCoroutine(shakeEvent));
    }

    private IEnumerator ShakeCoroutine(ScreenShakeEvent shakeEvent)
    {
        float elapsedTime = 0;

        while (elapsedTime < shakeEvent.Duration)
        {
            float t = Mathf.Clamp01(elapsedTime / shakeEvent.Duration);
            float intensity = _shakeIntensity * shakeEvent.IntensityMultiplier * shakeEvent.IntensityCurve.Evaluate(t);

            Vector3 shakeDir = Random.insideUnitSphere;
            Vector3 projection = Vector3.Project(shakeDir, transform.forward);

            _shakeOffset = intensity * (shakeDir - projection);
            
            yield return null;
            
            elapsedTime += Time.deltaTime;
        }
    }

    private IEnumerator ForceLookAtCoroutine(ForceLookEvent forceLookEvent)
    {
        var startRotation = transform.rotation;
        var toTarget = _forcedTarget.position - transform.position;
        var targetRotation = Quaternion.LookRotation(toTarget);
        
        float elapsedTime = 0;
        while (elapsedTime < forceLookEvent.ForcedLookAtLerpTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / forceLookEvent.ForcedLookAtLerpTime);
            t = forceLookEvent.ForcedLookAtCurve.Evaluate(t);
            
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            
            yield return null;
        }

        yield return new WaitForSeconds(forceLookEvent.ForcedLookAtHoldTime);
        
        elapsedTime = 0;
        while (elapsedTime < forceLookEvent.LookBackLerpTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / forceLookEvent.LookBackLerpTime);
            t = forceLookEvent.LookBackCurve.Evaluate(t);
            
            transform.rotation = Quaternion.Slerp(targetRotation, startRotation, t);
            
            yield return null;
        }
        
        // transform.rotation = targetRotation;
        _forcedLookAtCoroutine = null;
        PlayerController.GainControl();
        
        StopAllCoroutines();
    }

    public void RotateInput(InputAction.CallbackContext context)
    {
        bool usingMouse = context.control.device is Mouse && Input.GetMouseButton(1);
        
        if (usingMouse || context.control.device is Gamepad)
            _rotateInput = context.ReadValue<Vector2>();
    }

    private void SetScreenShakeMultiplier(System.Single value)
    {
        _shakeIntensity = math.clamp(value,0,1);
    }

    public void SetForcedTarget(Transform target)
    {
        _forcedTarget = target;
    }
    
    public void ForceLookAt(ForceLookEvent forceLookEvent)
    {
        PlayerController.LoseControl();

        _forcedLookAtCoroutine = StartCoroutine(ForceLookAtCoroutine(forceLookEvent));
        
        StartCoroutine(SafetyControlRegainRoutine());
    }

    public void RegainControl()
    {
        PlayerController.GainControl();
    }

    private IEnumerator SafetyControlRegainRoutine()
    {
        yield return new WaitForSecondsRealtime(5);
        RegainControl();
    }
}