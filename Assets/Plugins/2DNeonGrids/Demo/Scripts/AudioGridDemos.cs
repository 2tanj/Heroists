using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioGridDemos : MonoBehaviour
{
    [SerializeField]
    [Range(0, 7)]
    private int band = 0;

    [SerializeField]
    private Material _gridMainMaterial;

    [SerializeField]
    private float _minColorIntensityFactor = 1f;

    [SerializeField]
    private Texture mainGridTex;

    [SerializeField]
    private Texture secondaryGridTex;

    [SerializeField]
    [Range(0f, 1f)]
    private float _minAlpha = 0.1f;

    [SerializeField]
    private Color _mainGridColor;

    [SerializeField]
    private Color _secondaryGridColor;

    [SerializeField]
    private float _minTileScale = 4f;

    [SerializeField]
    private float _tileScaleFactor = 16f;

    [SerializeField]
    private Texture overrideScanlineTexture;

    [SerializeField]
    private LineRenderer lineRenderer;

    [SerializeField]
    private Camera lineRenderTextureCam;

    [SerializeField]
    private float lineCamZoomLevel = -6.5f;

    [SerializeField]
    private float _lineHeightScale = 1f;

    [SerializeField]
    private float _linePositionXPadding = 1f;

    [SerializeField]
    private float _linePositionYPadding = 0f;

    [SerializeField]
    private int linePositionScale = 2;

    [SerializeField]
    [Range(0f, 10f)]
    private float lineWidth = 0.25f;

    [SerializeField]
    [Range(-10f, 10f)]
    private float mainGridRotation = 0f;

    [SerializeField]
    [Range(-10f, 10f)]
    private float secondaryGridRotation = 0f;

    [SerializeField]
    private bool _changeColorToBeat = false;

    [Range(0f, 1f)]
    [SerializeField]
    private float minColorValueForBeats = 0f;

    [SerializeField]
    private bool rotateGridsToBeat = false;

    [Range(1f, 16f)]
    [SerializeField]
    private float gridRotationEaseFactor = 1f;

    [SerializeField]
    private bool _gridTileScaleToBeat = true;

    [SerializeField]
    private bool _distortToBeat = true;

    [Range(1f, 512f)]
    [SerializeField]
    private float _distortionFactor = 1f;

    [SerializeField]
    private bool _amplitudeForOpacity;

    [SerializeField]
    private bool _amplitudeForTiling;

    [Range(0f, 1f)]
    [SerializeField]
    private float _hexTransformMinAmplitudeScale;

    [Range(1f, 50f)]
    [SerializeField]
    private float _hexTransformAmplitudeScale = 2f;

    [SerializeField]
    private Transform[] hexTransforms;

    private int _linePositionCount;

    private int _currentBandColorIteration;

    private Color _beatColor;

    private Color _beatColorComplimentary;

    private float _timeSinceLastChange;

    public enum AudioBands
    {
        Eight,
        SixtyFour
    }

    public AudioBands _audioBands;

    void Start()
    {
        SetBands();
        Debug.Log("line audiobandbuffer length: " + AudioTools.Instance._audioBandBuffer.Length);
        
    }

    private void SetBands()
    {
        if (_audioBands == AudioBands.Eight)
        {
            _linePositionCount = AudioTools.Instance._audioBandBuffer.Length * linePositionScale;
            lineRenderer.positionCount = _linePositionCount;
        }
        else
        {
            _linePositionCount = AudioTools.Instance._audioBandBuffer64.Length * linePositionScale;
            lineRenderer.positionCount = _linePositionCount;
        }
    }

    void UpdateLineRenderer()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        for (var x = 0; x < _linePositionCount; x++)
        {
            var bandSelector = (int)(x / linePositionScale);

            var band = (_audioBands == AudioBands.Eight)
                ? AudioTools.Instance._audioBandBuffer[bandSelector]
                : AudioTools.Instance._audioBandBuffer64[bandSelector];

            var lineY = band * _lineHeightScale;

            lineRenderer.SetPosition(x, new Vector2(x + _linePositionXPadding, lineY + _linePositionYPadding));
        }

        lineRenderTextureCam.transform.position = new Vector3(
            lineRenderTextureCam.transform.position.x,
            lineRenderTextureCam.transform.position.y,
            lineCamZoomLevel);
    }

    // Update is called once per frame
    void Update()
    {
        SetBands();
        UpdateLineRenderer();

        _gridMainMaterial.SetTexture("_ScanlineTexture", overrideScanlineTexture);
        _gridMainMaterial.SetTexture("_MainGridTexture", mainGridTex);
        _gridMainMaterial.SetTexture("_SecondaryGridTexture", secondaryGridTex);

        var mainAmplitude = AudioTools.Instance._amplitudeBuffer64;
        var ampScale = _hexTransformMinAmplitudeScale + (mainAmplitude * _hexTransformAmplitudeScale);
        foreach (var t in hexTransforms)
        {
            t.localScale = new Vector3(ampScale, ampScale, ampScale);
        }

        if (_distortToBeat)
        {
            var _mainGridDistortion = _distortionFactor * AudioTools.Instance._audioBandBuffer[band];
            var _secondaryGridDistortion = _distortionFactor * AudioTools.Instance._audioBandBuffer[band];
            _gridMainMaterial.SetFloat("_MainGridDistortionNoiseScale", _mainGridDistortion);
            _gridMainMaterial.SetFloat("_SecondaryGridDistortionNoiseScale", _secondaryGridDistortion);
        }

        var _colorIntensity = _minColorIntensityFactor + Random.Range(2f, 5f) * AudioTools.Instance._audioBandBuffer[band];

        if (_changeColorToBeat)
        {
            _timeSinceLastChange += Time.deltaTime;

            if (_audioBands == AudioBands.SixtyFour)
            {
                var h = _currentBandColorIteration / 64f * 1f;
                _beatColor = Color.HSVToRGB(h, 1f, 1f);
                _gridMainMaterial.SetColor("_MainGridColor", _beatColor * _colorIntensity);

                var complimentaryH = (h + 0.5f) % 1f;
                _beatColorComplimentary = Color.HSVToRGB(complimentaryH, 1f, 1f);
                _gridMainMaterial.SetColor("_SecondaryGridColor", _beatColorComplimentary * _colorIntensity);
            }
            else
            {
                var h = _currentBandColorIteration / 8f * 1f;
                _beatColor = Color.HSVToRGB(h, 1f, 1f);
                _gridMainMaterial.SetColor("_MainGridColor", _beatColor * _colorIntensity);

                var complimentaryH = (h + 0.5f) % 1f;
                _beatColorComplimentary = Color.HSVToRGB(complimentaryH, 1f, 1f);
                _gridMainMaterial.SetColor("_SecondaryGridColor", _beatColorComplimentary * _colorIntensity);
            }

            if (AudioTools.Instance._audioBandBuffer64[band] > 0.65f && _timeSinceLastChange > 0.4f)
            {

                if (_currentBandColorIteration >= 63)
                {
                    _currentBandColorIteration = 0;
                }
                else
                {
                    _currentBandColorIteration++;
                }

                _timeSinceLastChange = 0f;
            }
        }
        else
        {

            _gridMainMaterial.SetColor("_MainGridColor", _mainGridColor * _colorIntensity);
            _gridMainMaterial.SetColor("_SecondaryGridColor", _secondaryGridColor * _colorIntensity);
        }

        if (rotateGridsToBeat)
        {
            mainGridRotation = AudioTools.Instance._audioBandBuffer[band] / gridRotationEaseFactor;
            secondaryGridRotation = AudioTools.Instance._audioBandBuffer[band] / gridRotationEaseFactor;
            _gridMainMaterial.SetFloat("_MainGridRotationSpeed", mainGridRotation);
            _gridMainMaterial.SetFloat("_SecondaryGridRotationSpeed", secondaryGridRotation);
        }
        
        if (_gridTileScaleToBeat)
        {
            if (_amplitudeForTiling)
            {
                if (_audioBands == AudioBands.Eight)
                {
                    _gridMainMaterial.SetVector("_Tiling", new Vector4(_minTileScale + AudioTools.Instance._amplitudeBuffer * _tileScaleFactor, _minTileScale + AudioTools.Instance._amplitudeBuffer * _tileScaleFactor, 0, 0));
                }
                else
                {
                    _gridMainMaterial.SetVector("_Tiling", new Vector4(_minTileScale + mainAmplitude * _tileScaleFactor, _minTileScale + AudioTools.Instance._amplitudeBuffer64 * _tileScaleFactor, 0, 0));
                }
            }
            else
            {
                _gridMainMaterial.SetVector("_Tiling", new Vector4(_minTileScale + AudioTools.Instance._audioBandBuffer[band] * _tileScaleFactor, _minTileScale + AudioTools.Instance._audioBandBuffer[band] * _tileScaleFactor, 0, 0));
            }
        }
        
        if (_amplitudeForOpacity)
        {
            if (_audioBands == AudioBands.Eight)
            {
                _gridMainMaterial.SetFloat("_OverallAlpha", Mathf.Clamp(AudioTools.Instance._amplitudeBuffer, _minAlpha, 1f));
            }
            else
            {
                _gridMainMaterial.SetFloat("_OverallAlpha", Mathf.Clamp(mainAmplitude, _minAlpha, 1f));
            }
        }
        else
        {
            _gridMainMaterial.SetFloat("_OverallAlpha", Mathf.Clamp(AudioTools.Instance._audioBandBuffer[band], _minAlpha, 1f));
        }
    }
}
