using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class DemoGrids : MonoBehaviour
{
    public enum ColorType
    {
        ThreeSet,
        RandomBright,
        Complimentary,
        Neon
    }

    [SerializeField]
    private GameObject[] buttons;

    [SerializeField]
    private Material _gridMainMaterial;

    [SerializeField]
    private Texture[] _textures;

    [SerializeField]
    private Texture[] _scanLineTextures;

    [SerializeField]
    private Color[] _neonColors;

    [SerializeField]
    private GridConfigSaveAndLoad gridConfigSaveAndLoadRef;

    [SerializeField]
    private string _savedGridJsonConfigsDir;

    [SerializeField]
    private GridConfig[] _loadedConfigs;

    [SerializeField]
    private float _gridDisplaySeconds = 3f;

    private int _gridIndexToLoad;

    public ColorType _colorTypeChoice;
    public GridConfig _currentGridConfig;

    public void GetRandomGrid()
    {
        var gridConfig = new GridConfig();

        var threeSetColors = ColorExtensions.GetRandomThreeSetColours();
        var randomBrightColors = ColorExtensions.GetRandomBrightColours(10);

        var intensityFactor1 = Random.Range(2f, 5f);
        var intensityFactor2 = Random.Range(2f, 5f);
        var color1 = Color.white;
        var color2 = Color.white;

        switch (_colorTypeChoice)
        {
            case ColorType.ThreeSet:
                color1 = threeSetColors[0] * intensityFactor1;
                color2 = threeSetColors[2] * intensityFactor2;
                break;
            case ColorType.RandomBright:
                var index = Random.Range(0, randomBrightColors.Length);
                var index2 = Random.Range(0, randomBrightColors.Length);
                if (index == index2) index2 = Random.Range(0, randomBrightColors.Length);

                color1 = randomBrightColors[index] * intensityFactor1;
                color2 = randomBrightColors[index2] * intensityFactor2;
                break;
            case ColorType.Complimentary:
                color1 = randomBrightColors[Random.Range(0, randomBrightColors.Length)] * intensityFactor1;
                color2 = ColorExtensions.InvertColor(color1) * intensityFactor2;
                break;
            case ColorType.Neon:
                color1 = _neonColors[Random.Range(0, _neonColors.Length)] * intensityFactor1;
                color2 = _neonColors[Random.Range(0, _neonColors.Length)] * intensityFactor2;
                break;
            default:
                break;
        }

        gridConfig.mainGridColor = color1;
        gridConfig.secondaryGridColor = color2;
        gridConfig.mainGridColorIntensity = intensityFactor1;
        gridConfig.secondaryGridColorIntensity = intensityFactor2;

        _gridMainMaterial.SetColor("_MainGridColor", color1);
        _gridMainMaterial.SetColor("_SecondaryGridColor", color2);

        var mainDistortion = Random.Range(0f, 96f);
        var secondaryDistortion = Random.Range(0f, 96f);
        gridConfig.mainGridDistortionNoiseScale = mainDistortion;
        gridConfig.secondaryGridDistortionNoiseScale = secondaryDistortion;

        _gridMainMaterial.SetFloat("_MainGridDistortionNoiseScale", mainDistortion);
        _gridMainMaterial.SetFloat("_SecondaryGridDistortionNoiseScale", secondaryDistortion);

        var scanEffectOpacity = Random.Range(0.35f, 0.9f);
        gridConfig.scanlineEffectOpacity = scanEffectOpacity;
        _gridMainMaterial.SetFloat("_ScanlineEffectOpacity", scanEffectOpacity);

        var overallAlpha = Random.Range(0.3f, 1f);
        gridConfig.overallAlpha = overallAlpha;
        _gridMainMaterial.SetFloat("_OverallAlpha", overallAlpha);

        var secondaryGridXMoveFactor = Random.Range(0f, 10f);
        var secondaryGridYMoveFactor = Random.Range(0f, 10f);
        gridConfig.secondaryGridXMoveFactor = secondaryGridXMoveFactor;
        gridConfig.secondaryGridYMoveFactor = secondaryGridYMoveFactor;
        _gridMainMaterial.SetFloat("_SecondaryGridXMoveFactor", secondaryGridXMoveFactor);
        _gridMainMaterial.SetFloat("_SecondaryGridYMoveFactor", secondaryGridYMoveFactor);

        var mainGridRotSpeed = 0f;
        var secondaryGridRotSpeed = 0f;
        if (Random.Range(0f, 1f) > 0.7f)
        {
            mainGridRotSpeed = Random.Range(0f, 0.5f);
            secondaryGridRotSpeed = Random.Range(0f, 0.5f);
            _gridMainMaterial.SetFloat("_MainGridRotationSpeed", mainGridRotSpeed);
            _gridMainMaterial.SetFloat("_SecondaryGridRotationSpeed", secondaryGridRotSpeed);
        }
        else if (Random.Range(0f, 1f) > 0.8f)
        {
            mainGridRotSpeed = Random.Range(-0.5f, 0.5f);
            _gridMainMaterial.SetFloat("_MainGridRotationSpeed", mainGridRotSpeed);
            _gridMainMaterial.SetFloat("_SecondaryGridRotationSpeed", 0f);
        }
        else
        {
            _gridMainMaterial.SetFloat("_MainGridRotationSpeed", 0f);
            _gridMainMaterial.SetFloat("_SecondaryGridRotationSpeed", 0f);
        }

        gridConfig.mainGridRotationSpeed = mainGridRotSpeed;
        gridConfig.secondaryGridRotationSpeed = secondaryGridRotSpeed;

#pragma warning disable IDE0059 // Unnecessary assignment of a value
        var vertScanTilingFactor = 0f;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning disable IDE0059 // Unnecessary assignment of a value
        var scanlineSpeed = 0f;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        if (Random.Range(0f, 1f) > 0.7f)
        {
            vertScanTilingFactor = Random.Range(1f, 256f);
            scanlineSpeed = Random.Range(0f, 16f);
            _gridMainMaterial.SetFloat("_VerticalScanTilingFactor", vertScanTilingFactor);
            _gridMainMaterial.SetFloat("_ScanlineSpeed", scanlineSpeed);
        }
        else
        {
            vertScanTilingFactor = Random.Range(-3f, 3f);
            scanlineSpeed = Random.Range(0f, 1f);
            _gridMainMaterial.SetFloat("_VerticalScanTilingFactor", vertScanTilingFactor);
            _gridMainMaterial.SetFloat("_ScanlineSpeed", scanlineSpeed);
        }

        gridConfig.verticalScanTilingFactor = vertScanTilingFactor;
        gridConfig.scanlineSpeed = scanlineSpeed;

        if (Random.Range(0f, 1f) >= 0.5f)
        {
            _gridMainMaterial.SetFloat("_SecondaryGridUseGradientNoise", 1f);
            gridConfig.secondaryGridUseSimpleNoise = true;
        }
        else
        {
            _gridMainMaterial.SetFloat("_SecondaryGridUseGradientNoise", 0f);
            gridConfig.secondaryGridUseSimpleNoise = false;
        }

        if (Random.Range(0f, 1f) >= 0.5f)
        {
            _gridMainMaterial.SetFloat("_MainGridUseGradientNoise", 1f);
            gridConfig.mainGridUseGradientNoise = true;
        }
        else
        {
            _gridMainMaterial.SetFloat("_MainGridUseGradientNoise", 0f);
            gridConfig.mainGridUseGradientNoise = false;
        }
        
        if (Random.Range(0f, 1f) >= 0.85f)
        {
            _gridMainMaterial.SetFloat("_InvertMainGridColor", 1f);
            gridConfig.invertMainGridColor = true;
        }
        else
        {
            _gridMainMaterial.SetFloat("_InvertMainGridColor", 0f);
            gridConfig.invertMainGridColor = false;
        }

        if (Random.Range(0f, 1f) >= 0.85f)
        {
            _gridMainMaterial.SetFloat("_InvertSecondaryGridColor", 1f);
            gridConfig.invertSecondaryGridColor = true;
        }
        else
        {
            _gridMainMaterial.SetFloat("_InvertSecondaryGridColor", 0f);
            gridConfig.invertSecondaryGridColor = false;
        }

        var tileRandomValue = Random.Range(4f, 40f);
        var tilingRandom = new Vector4(tileRandomValue, tileRandomValue, 0f, 0f);
        gridConfig.tiling = tilingRandom;
        _gridMainMaterial.SetVector("_Tiling", tilingRandom);

        var mainTexIndex = Random.Range(0, _textures.Length);
        var mainTex = _textures[mainTexIndex];
        _gridMainMaterial.SetTexture("_MainGridTexture", mainTex);

        var secondaryTexIndex = Random.Range(0, _textures.Length);
        var secondaryTex = _textures[secondaryTexIndex];
        _gridMainMaterial.SetTexture("_SecondaryGridTexture", secondaryTex);

        var scanlineTexIndex = Random.Range(0, _scanLineTextures.Length);
        var scanlineTex = _scanLineTextures[scanlineTexIndex];
        _gridMainMaterial.SetTexture("_ScanlineTexture", scanlineTex);

        gridConfig.mainGridTextureName = _textures[mainTexIndex].name;
        gridConfig.secondaryGridTextureName = _textures[secondaryTexIndex].name;
        gridConfig.scanlineTextureName = _scanLineTextures[scanlineTexIndex].name;

        _currentGridConfig = gridConfig;
    }

    public void SaveCurrentGridConfig()
    {
        var gridConfig = new GridConfig
        {
#pragma warning disable IDE0075 // Simplify conditional expression
            invertMainGridColor = _gridMainMaterial.GetFloat("_InvertMainGridColor") == 1f ? true : false,
#pragma warning restore IDE0075 // Simplify conditional expression
#pragma warning disable IDE0075 // Simplify conditional expression
            invertSecondaryGridColor = _gridMainMaterial.GetFloat("_InvertSecondaryGridColor") == 1f ? true : false,
#pragma warning restore IDE0075 // Simplify conditional expression
            mainGridColor = _gridMainMaterial.GetColor("_MainGridColor"),
            secondaryGridColor = _gridMainMaterial.GetColor("_SecondaryGridColor"),
            mainGridDistortionNoiseScale = _gridMainMaterial.GetFloat("_MainGridDistortionNoiseScale"),
            secondaryGridDistortionNoiseScale = _gridMainMaterial.GetFloat("_SecondaryGridDistortionNoiseScale"),
            mainGridRotationSpeed = _gridMainMaterial.GetFloat("_MainGridRotationSpeed"),
            secondaryGridRotationSpeed = _gridMainMaterial.GetFloat("_SecondaryGridRotationSpeed"),
            mainGridTextureName = _gridMainMaterial.GetTexture("_MainGridTexture").name,
            secondaryGridTextureName = _gridMainMaterial.GetTexture("_SecondaryGridTexture").name,
            scanlineTextureName = _gridMainMaterial.GetTexture("_ScanlineTexture").name,
            overallAlpha = _gridMainMaterial.GetFloat("_OverallAlpha"),
            scanlineEffectOpacity = _gridMainMaterial.GetFloat("_ScanlineEffectOpacity"),
            scanlineSpeed = _gridMainMaterial.GetFloat("_ScanlineSpeed"),
            secondaryGridXMoveFactor = _gridMainMaterial.GetFloat("_SecondaryGridXMoveFactor"),
            secondaryGridYMoveFactor = _gridMainMaterial.GetFloat("_SecondaryGridYMoveFactor"),
            tiling = _gridMainMaterial.GetVector("_Tiling"),
            verticalScanTilingFactor = _gridMainMaterial.GetFloat("_VerticalScanTilingFactor"),
#pragma warning disable IDE0075 // Simplify conditional expression
            mainGridUseGradientNoise = _gridMainMaterial.GetFloat("_MainGridUseGradientNoise") == 1f ? true : false,
#pragma warning restore IDE0075 // Simplify conditional expression
#pragma warning disable IDE0075 // Simplify conditional expression
            secondaryGridUseSimpleNoise = _gridMainMaterial.GetFloat("_SecondaryGridUseGradientNoise") == 1f ? true : false,
#pragma warning restore IDE0075 // Simplify conditional expression
        };

        _currentGridConfig = gridConfig;
        gridConfigSaveAndLoadRef.SaveGridConfig(_currentGridConfig);
    }

    public void ColorModeDropDownValueChanged(Dropdown dropdown)
    {
        switch (dropdown.value)
        {
            case 0:
                _colorTypeChoice = ColorType.ThreeSet;
                break;
            case 1:
                _colorTypeChoice = ColorType.RandomBright;
                break;
            case 2:
                _colorTypeChoice = ColorType.Complimentary;
                break;
            case 3:
                _colorTypeChoice = ColorType.Neon;
                break;
            default:
                break;
        };
    }

    public void LoadAndEnterDemoMode()
    {
        if (string.IsNullOrEmpty(_savedGridJsonConfigsDir))
        {
            Debug.Log("No valid '_savedGridJsonConfigsDir' path set. Using the standard 2DNeonGrids/Demo path instead.");
            _savedGridJsonConfigsDir = Path.Combine(Application.dataPath, "2DNeonGrids/Demo", "SavedDemoGrids");
        }

        _loadedConfigs = gridConfigSaveAndLoadRef
            .LoadGridConfigsFromPath(_savedGridJsonConfigsDir)
            .ToArray();

        if (_loadedConfigs.Length <= 0)
        {
            Debug.LogWarning($"No Grid JSON configuration files found in path: {_savedGridJsonConfigsDir}");
        }
        else
        {
            StartCoroutine(LoadGridConfig(_gridDisplaySeconds));
        }
    }

    private IEnumerator LoadGridConfig(float delay)
    {
        var gridConfig = _loadedConfigs[_gridIndexToLoad];
        if (_gridIndexToLoad + 1 < _loadedConfigs.Length)
        {
            _gridIndexToLoad++;
        }
        else
        {
            _gridIndexToLoad = 0;
        }

        yield return new WaitForSeconds(delay);

        _currentGridConfig = gridConfig;

        _gridMainMaterial.SetColor("_MainGridColor", gridConfig.mainGridColor);
        _gridMainMaterial.SetColor("_SecondaryGridColor", gridConfig.secondaryGridColor);
        _gridMainMaterial.SetFloat("_MainGridDistortionNoiseScale", gridConfig.mainGridDistortionNoiseScale);
        _gridMainMaterial.SetFloat("_SecondaryGridDistortionNoiseScale", gridConfig.secondaryGridDistortionNoiseScale);
        _gridMainMaterial.SetFloat("_ScanlineEffectOpacity", gridConfig.scanlineEffectOpacity);
        _gridMainMaterial.SetFloat("_OverallAlpha", gridConfig.overallAlpha);
        _gridMainMaterial.SetFloat("_SecondaryGridXMoveFactor", gridConfig.secondaryGridXMoveFactor);
        _gridMainMaterial.SetFloat("_SecondaryGridYMoveFactor", gridConfig.secondaryGridYMoveFactor);
        _gridMainMaterial.SetFloat("_MainGridRotationSpeed", gridConfig.mainGridRotationSpeed);
        _gridMainMaterial.SetFloat("_SecondaryGridRotationSpeed", gridConfig.secondaryGridRotationSpeed);
        _gridMainMaterial.SetFloat("_VerticalScanTilingFactor", gridConfig.verticalScanTilingFactor);
        _gridMainMaterial.SetFloat("_ScanlineSpeed", gridConfig.scanlineSpeed);
        

        if (gridConfig.invertMainGridColor)
        {
            _gridMainMaterial.SetFloat("_InvertMainGridColor", 1f);
        }
        else
        {
            _gridMainMaterial.SetFloat("_InvertMainGridColor", 0f);
        }

        if (gridConfig.mainGridUseGradientNoise)
        {
            _gridMainMaterial.SetFloat("_MainGridUseGradientNoise", 1f);
        }
        else
        {
            _gridMainMaterial.SetFloat("_MainGridUseGradientNoise", 0f);
        }

        if (gridConfig.secondaryGridUseSimpleNoise)
        {
            _gridMainMaterial.SetFloat("_SecondaryGridUseGradientNoise", 1f);
        }
        else
        {
            _gridMainMaterial.SetFloat("_SecondaryGridUseGradientNoise", 0f);
        }

        if (gridConfig.invertSecondaryGridColor)
        {
            _gridMainMaterial.SetFloat("_InvertSecondaryGridColor", 1f);
        }
        else
        {
            _gridMainMaterial.SetFloat("_InvertSecondaryGridColor", 0f);
        }

        _gridMainMaterial.SetVector("_Tiling", gridConfig.tiling);

        var mainTex = _textures.Where((t) => t.name == gridConfig.mainGridTextureName).First();
        var secondaryTex = _textures.Where((t) => t.name == gridConfig.secondaryGridTextureName).First();
        var scanlineTex = _scanLineTextures.Where((t) => t.name == gridConfig.scanlineTextureName).First();

        _gridMainMaterial.SetTexture("_MainGridTexture", mainTex);
        _gridMainMaterial.SetTexture("_SecondaryGridTexture", secondaryTex);
        _gridMainMaterial.SetTexture("_ScanlineTexture", scanlineTex);

        StartCoroutine(LoadGridConfig(_gridDisplaySeconds));
    }

    private void Update()
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
