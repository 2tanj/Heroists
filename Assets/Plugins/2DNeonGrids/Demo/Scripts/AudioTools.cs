using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioTools : MonoBehaviour
{
    public static AudioTools Instance;

    AudioSource _audioSource;

    public float _falloffRate = 0.2f;

    float[] _freqBand;
    float[] _bandBuffer;
    float[] _bufferDecrease;
    public float[] _freqBandHighest;

    // 64 bands
    float[] _freqBand64;
    float[] _bandBuffer64;
    float[] _bufferDecrease64;
    public float[] _freqBandHighest64;

    public float[] _samplesLeft;
    public float[] _samplesRight;

    float _amplitudeHighest;
    float _amplitudeHighest64;

    public float[] _audioBand;
    public float[] _audioBandBuffer;

    public float[] _audioBand64;
    public float[] _audioBandBuffer64;


    public float _amplitude, _amplitudeBuffer, _amplitude64, _amplitudeBuffer64;

    public enum _channel
    {
        Stereo,
        Left,
        Right
    }

    public _channel channel = new _channel();
    

    private void Awake()
    {
        Instance = this;

        _audioSource = GetComponent<AudioSource>();

        _freqBand = new float[8];
        _bandBuffer = new float[8];
        _bufferDecrease = new float[8];

        _freqBandHighest = new float[8];
        _audioBand = new float[8];
        _audioBandBuffer = new float[8];

        _freqBand64 = new float[64];
        _bandBuffer64 = new float[64];
        _bufferDecrease64 = new float[64];
        _freqBandHighest64 = new float[64];

        _audioBand64 = new float[64];
        _audioBandBuffer64 = new float[64];

        _samplesLeft = new float[512];
        _samplesRight = new float[512];
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void CreateAudioBands()
    {
        for (var i = 0; i < 8; i++)
        {
            if (_freqBand[i] > _freqBandHighest[i])
            {
                _freqBandHighest[i] = _freqBand[i];
            }

            _audioBand[i] = (_freqBand[i] / _freqBandHighest[i]);
            _audioBandBuffer[i] = (_bandBuffer[i] / _freqBandHighest[i]);
        }
    }

    void CreateAudioBands64()
    {
        for (var i = 0; i < 64; i++)
        {
            if (_freqBand64[i] > _freqBandHighest64[i])
            {
                _freqBandHighest64[i] = _freqBand64[i];
            }

            _audioBand64[i] = (_freqBand64[i] / _freqBandHighest64[i]);
            _audioBandBuffer64[i] = (_bandBuffer64[i] / _freqBandHighest64[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        MakeFrequencyBands64();
        BandBuffer();
        BandBuffer64();
        CreateAudioBands();
        CreateAudioBands64();
        GetAmplitude();
        GetAmplitude64();
    }

    void BandBuffer()
    {
        for (int g = 0; g < 8; ++g)
        {
            if (_freqBand[g] > _bandBuffer[g])
            {
                _bandBuffer[g] = _freqBand[g];
                _bufferDecrease[g] = 0.005f;
            }

            if (_freqBand[g] < _bandBuffer[g])
            {
                _bandBuffer[g] -= _bufferDecrease[g];
                _bufferDecrease[g] *= 1.0f + (0f + _falloffRate);
            }
        }
    }

    void BandBuffer64()
    {
        for (int g = 0; g < 64; ++g)
        {
            if (_freqBand64[g] > _bandBuffer64[g])
            {
                _bandBuffer64[g] = _freqBand64[g];
                _bufferDecrease64[g] = 0.005f;
            }

            if (_freqBand64[g] < _bandBuffer64[g])
            {
                _bandBuffer64[g] -= _bufferDecrease64[g];
                _bufferDecrease64[g] *= 1.0f + (0f + _falloffRate);
            }
        }
    }

    void MakeFrequencyBands()
    {
        var count = 0;

        for (var i = 0; i < 8; i++)
        {
            var average = 0f;
            var sampleCount = (int)Mathf.Pow(2, i) * 2;

            if (i == 7)
            {
                sampleCount += 2;
            }

            for (var j = 0; j < sampleCount; j++)
            {
                if (channel == _channel.Stereo)
                {
                    average += (_samplesLeft[count] + _samplesRight[count]) * (count + 1);
                }
                if (channel == _channel.Left)
                {
                    average += _samplesLeft[count] * (count + 1);
                }
                if (channel == _channel.Right)
                {
                    average += _samplesRight[count] * (count + 1);
                }

                count++;
            }

            average /= count;

            _freqBand[i] = average * 10;
        }

    }

    void MakeFrequencyBands64()
    {
        var count = 0;
        var sampleCount = 1;
        var power = 0;

        for (var i = 0; i < 64; i++)
        {
            var average = 0f;

            if (i == 16 || i == 32 || i == 40 || i == 48 || i == 56)
            {
                power++;
                sampleCount = (int)Mathf.Pow(2, power);
                if (power == 3)
                {
                    sampleCount -= 2;
                }
            }

            for (var j = 0; j < sampleCount; j++)
            {
                if (channel == _channel.Stereo)
                {
                    average += (_samplesLeft[count] + _samplesRight[count]) * (count + 1);
                }
                if (channel == _channel.Left)
                {
                    average += _samplesLeft[count] * (count + 1);
                }
                if (channel == _channel.Right)
                {
                    average += _samplesRight[count] * (count + 1);
                }

                count++;
            }

            average /= count;

            _freqBand64[i] = average * 80;
        }

    }

    void GetAmplitude()
    {
        var currentAmplitude = 0f;
        var currentAmplitudeBuffer = 0f;
        for (var i = 0; i < 8; i++)
        {
            currentAmplitude += _audioBand[i];
            currentAmplitudeBuffer += _audioBandBuffer[i];

            if (currentAmplitude > _amplitudeHighest)
            {
                _amplitudeHighest = currentAmplitude;
            }

            _amplitude = currentAmplitude / _amplitudeHighest;
            _amplitudeBuffer = currentAmplitudeBuffer / _amplitudeHighest;
        }
    }

    void GetAmplitude64()
    {
        var currentAmplitude = 0f;
        var currentAmplitudeBuffer = 0f;
        for (var i = 0; i < 64; i++)
        {
            currentAmplitude += _audioBand64[i];
            currentAmplitudeBuffer += _audioBandBuffer64[i];

            if (currentAmplitude > _amplitudeHighest64)
            {
                _amplitudeHighest64 = currentAmplitude;
            }

            _amplitude64 = currentAmplitude / _amplitudeHighest64;
            _amplitudeBuffer64 = currentAmplitudeBuffer / _amplitudeHighest64;
        }
    }

    void AudioProfile(float audioProfile)
    {
        for (var i = 0; i < 8; i++)
        {
            _freqBandHighest[i] = audioProfile;
        }
    }

    void AudioProfile64(float audioProfile)
    {
        for (var i = 0; i < 64; i++)
        {
            _freqBandHighest64[i] = audioProfile;
        }
    }

    void GetSpectrumAudioSource()
    {
        _audioSource.GetSpectrumData(_samplesLeft, 0, FFTWindow.Blackman);
        _audioSource.GetSpectrumData(_samplesRight, 1, FFTWindow.Blackman);
    }
}
