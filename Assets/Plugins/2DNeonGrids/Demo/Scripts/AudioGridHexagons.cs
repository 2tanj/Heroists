using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioGridHexagons : MonoBehaviour
{
    [SerializeField]
    private GameObject[] buttons;

    [SerializeField]
    [Range(0, 7)]
    private int band = 0;

    [SerializeField]
    private Transform[] hexTransforms;

    [SerializeField]
    private Material _gridMainMaterial;

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

    private int linePositionCount;

    [Range(0f, 1f)]
    [SerializeField]
    private float _minAmplitudeScale;

    [Range(1f, 50f)]
    [SerializeField]
    private float _amplitudeScale = 2f;

    [Range(0f, 1f)]
    [SerializeField]
    private float _bandFalloffRate = 0.2f;

    [SerializeField]
    private bool useColorSpectrumForBars;

    [SerializeField]
    AudioBandSpriteStretcher[] spectrumSpriteBars;

    public enum AudioBands
    {
        Eight,
        SixtyFour
    }

    public AudioBands _audioBands;

    // Start is called before the first frame update
    void Start()
    {
        SetBands();
    }

    private void SetBands()
    {
        if (_audioBands == AudioBands.Eight)
        {
            linePositionCount = AudioTools.Instance._audioBandBuffer.Length * linePositionScale;
            lineRenderer.positionCount = linePositionCount;
        }
        else
        {
            linePositionCount = AudioTools.Instance._audioBandBuffer64.Length * linePositionScale;
            lineRenderer.positionCount = linePositionCount;
        }
    }

    void UpdateLineRenderer()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        for (var x = 0; x < linePositionCount; x++)
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
        Controls();

        AudioTools.Instance._falloffRate = _bandFalloffRate;
        var mainAmplitude = AudioTools.Instance._amplitudeBuffer64;
        var ampScale = _minAmplitudeScale + (mainAmplitude * _amplitudeScale);

        foreach (var t in hexTransforms)
        {
            t.localScale = new Vector3(ampScale, ampScale, ampScale);
        }

        foreach (var bar in spectrumSpriteBars)
        {
            bar.useColorSpectrumForBars = useColorSpectrumForBars;
        }

        _gridMainMaterial.SetTexture("_ScanlineTexture", overrideScanlineTexture);
        _gridMainMaterial.SetVector("_Tiling", new Vector4(_minTileScale + AudioTools.Instance._amplitudeBuffer64 * _tileScaleFactor, _minTileScale + AudioTools.Instance._amplitudeBuffer64 * _tileScaleFactor, 0, 0));
    }

    void Controls()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
        else if (Input.GetKeyUp(KeyCode.H))
        {
            foreach (var go in buttons)
            {
                go.SetActive(!go.activeSelf);
            }
        }
    }
}
