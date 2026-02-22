using System;
using UnityEngine;

public class Bow : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] [Range(0, 1)] private float tensionForce;
    [SerializeField] private Vector3 tensionOffset = new Vector3(0, -0.1f, 0.65f);
    [SerializeField] private AudioClip tensionClip;
    [SerializeField] private AudioClip releaseClip;
    
    private CharacterCore _characterCore;
    public event Action<float> OnTensionChanged;

    private Vector3 _startPosition;
    
    private void OnValidate()
    {
        if (lineRenderer == null) return;
        UpdateStringPosition(tensionForce);
    }

    private void Start()
    {
        _characterCore = GetComponentInParent<CharacterCore>();
        _startPosition = lineRenderer.GetPosition(1);
        OnTensionChanged += UpdateStringPosition;
        _characterCore.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        OnTensionChanged -= UpdateStringPosition;
        _characterCore.OnStateChanged -= OnStateChanged;
    }

    public void SetTension(float value)
    {
        tensionForce = Mathf.Clamp01(value);
        OnTensionChanged?.Invoke(tensionForce);
    }

    private void OnStateChanged(State state)
    {
        if (state is AimState)
        {
            UpdateStringPosition(1);
            AudioSource.PlayClipAtPoint(tensionClip, transform.position);
            return;
        }
        if(tensionForce <= 0) return;
        
        UpdateStringPosition(0);
        
        if(state is not ReleaseState) return;
        
        AudioSource.PlayClipAtPoint(releaseClip, transform.position);
    }

    private void UpdateStringPosition(float tension)
    {
        tensionForce = tension;
        var tensionPosition = Vector3.Lerp(_startPosition, _startPosition + tensionOffset, tension);
        lineRenderer.SetPosition(1, tensionPosition);
    }
}