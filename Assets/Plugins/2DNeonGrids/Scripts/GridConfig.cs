using UnityEngine;
using System;

[Serializable]
public class GridConfig
{
    public Vector4 tiling;
    public Color mainGridColor;
    public Color secondaryGridColor;
    public float verticalScanTilingFactor;
    public float scanlineEffectOpacity;
    public float scanlineSpeed;
    public float overallAlpha;
    public float mainGridRotationSpeed;
    public float secondaryGridRotationSpeed;
    public float secondaryGridXMoveFactor;
    public float secondaryGridYMoveFactor;
    public bool invertMainGridColor;
    public bool invertSecondaryGridColor;
    public bool mainGridUseGradientNoise;
    public bool secondaryGridUseSimpleNoise;
    public float mainGridDistortionNoiseScale;
    public float secondaryGridDistortionNoiseScale;
    public float mainGridColorIntensity;
    public float secondaryGridColorIntensity;
    public string mainGridTextureName;
    public string secondaryGridTextureName;
    public string scanlineTextureName;
}