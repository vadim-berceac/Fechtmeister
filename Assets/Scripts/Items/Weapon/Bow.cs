using System;
using System.Collections;
using UnityEngine;

public class Bow : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] [Range(0, 1)] private float tensionForce;
    [SerializeField] private float tensionDelay;
    [SerializeField] private float tensionTime;
    [SerializeField] private Vector3 stringRestPosition = new Vector3(0, 0f, 0.15f);
    [SerializeField] private Vector3 tensionOffset = new Vector3(0, -0.1f, 0.65f);
    [SerializeField] private AudioClip tensionClip;
    [SerializeField] private AudioClip releaseClip;

    private CharacterCore _characterCore;
    private Coroutine _tensionCoroutine;

    private void Start()
    {
        _characterCore = GetComponentInParent<CharacterCore>();
        _characterCore.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        _characterCore.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(State state)
    {
        if (state is LoadState)
        {
            if (_tensionCoroutine == null)
                _tensionCoroutine = StartCoroutine(TensionRoutine());

            AudioSource.PlayClipAtPoint(tensionClip, transform.position);
        }
        if (state is LoadState || state is AimState) return;

        if (_tensionCoroutine != null)
        {
            StopCoroutine(_tensionCoroutine);
            _tensionCoroutine = null;
        }

        if (tensionForce <= 0) return;

        UpdateStringPosition(0f);

        if (state is not ReleaseState) return;

        AudioSource.PlayClipAtPoint(releaseClip, transform.position);
    }

    private IEnumerator TensionRoutine()
    {
        yield return new WaitForSeconds(tensionDelay);

        if (tensionTime <= 0)
        {
            UpdateStringPosition(1f);
            _tensionCoroutine = null;
            yield break;
        }

        var elapsed = 0f;

        while (elapsed < tensionTime)
        {
            elapsed += Time.deltaTime;
            UpdateStringPosition(Mathf.Lerp(0f, 1f, elapsed / tensionTime));
            yield return null;
        }

        UpdateStringPosition(1f);
        _tensionCoroutine = null;
    }

    private void UpdateStringPosition(float tension)
    {
        tensionForce = tension;
        lineRenderer.SetPosition(1, Vector3.Lerp(stringRestPosition, stringRestPosition + tensionOffset, tension));
    }
}