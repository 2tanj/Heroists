using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AudioBandSpriteStretcher : MonoBehaviour
{
    [Range(0, 7)]
    public int _band;

    [Range(0, 63)]
    public int _band64;

    public float bandScale = 1f;

    public bool use64Bands;

    private SpriteRenderer sr;

    public bool useColorSpectrumForBars;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Sliced;
    }

    // Update is called once per frame
    void Update()
    {
        var band = use64Bands ? AudioTools.Instance._audioBandBuffer64[_band64] : AudioTools.Instance._audioBandBuffer[_band];
        var bandScaled = band * bandScale;
        sr.size = new Vector2(sr.size.x, bandScaled);

        if (useColorSpectrumForBars)
        {
            if (use64Bands)
            {
                sr.color = Color.HSVToRGB(_band64 / 64f * 1f , 1f, 1f);
            }
            else
            {
                sr.color = Color.HSVToRGB(_band / 8f, 1f, 1f);
            }
        }
    }
}
