using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RotationScript))]
public class AudioBounceChange : MonoBehaviour
{
    private RotationScript _rotation;

    [SerializeField]
    [Range(0, 63)]
    private int _bandToMonitor = 1;

    [SerializeField]
    private float _beatThresholdForChange = 0.8f;

    [SerializeField]
    private Material _gridMainMaterial;

    [SerializeField]
    private float _gridMainDistortionMultiplier = 1f;

    // Start is called before the first frame update
    void Start()
    {
        _rotation = GetComponent<RotationScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (AudioTools.Instance._audioBandBuffer64[_bandToMonitor] >= _beatThresholdForChange)
        {
            _rotation.rotationsPerMinute = _rotation.rotationsPerMinute * -1f;
        }

        _gridMainMaterial.SetFloat("_MainGridDistortionNoiseScale", 1f + AudioTools.Instance._amplitudeBuffer64 * _gridMainDistortionMultiplier);
    }
}
