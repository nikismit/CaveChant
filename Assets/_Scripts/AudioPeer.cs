// CREATED BY PEERPLAY
// WWW.PEERPLAY.NL
// v1.8

using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

[RequireComponent (typeof (AudioSource))]
public class AudioPeer : MonoBehaviour {
	AudioSource _audioSource;
	public bool _listenToAudioListener;

	//FFT values
	private float[] _samplesLeft = new float[512];
	private float[] _samplesRight = new float[512];

    public static float[] _freqBand = new float[8];
	private float[] _bandBuffer = new float[8];
	private float[] _bufferDecrease = new float[8];
	private float[] _freqBandHighest = new float[8];

	//audio band values
	[HideInInspector]
	public static float[] _audioBand, _audioBandBuffer;


	//Amplitude variables
	[HideInInspector]
	public static float _Amplitude, _AmplitudeBuffer;
	private float _AmplitudeHighest;
	//audio profile
	public float _audioProfile;

	//stereo channels
	public enum _channel {Stereo, Left, Right};
	public _channel channel = new _channel ();



	public static bool  _resetAudioProfile;




	void Awake()
	{
		DontDestroyOnLoad (this.gameObject);
	}

	// Use this for initialization
	void Start () {
		_audioBand = new float[8];
		_audioBandBuffer = new float[8];


		_audioSource = GetComponent<AudioSource> ();
		AudioProfile (_audioProfile);
	}

	// Update is called once per frame
	void Update () {
		if (_audioSource.clip != null) {
			GetSpectrumAudioSource ();
			MakeFrequencyBands ();

			BandBuffer ();

			CreateAudioBands ();

			GetAmplitude ();

		}
			
	}


	public void AudioProfile(float audioProfile)
	{
		for (int i = 0; i < 8; i++) {
			_freqBandHighest [i] = audioProfile;
		}
	}

	void GetAmplitude()
	{
		float _CurrentAmplitude = 0;
		float _CurrentAmplitudeBuffer = 0;
		for (int i = 0; i < 8; i++) {
			_CurrentAmplitude += _audioBand [i];
			_CurrentAmplitudeBuffer += _audioBandBuffer [i];
		}
		if (_CurrentAmplitude > _AmplitudeHighest) {
			_AmplitudeHighest = _CurrentAmplitude;
		}
		_Amplitude = _CurrentAmplitude / _AmplitudeHighest;
		_AmplitudeBuffer = _CurrentAmplitudeBuffer / _AmplitudeHighest;
	}

	void CreateAudioBands()
	{
		for (int i = 0; i < 8; i++) 
		{
			if (_freqBand [i] > _freqBandHighest [i]) {
				_freqBandHighest [i] = _freqBand [i];
			}
			_audioBand [i] = Mathf.Clamp((_freqBand [i] / _freqBandHighest [i]), 0, 1);
			_audioBandBuffer [i] = Mathf.Clamp((_bandBuffer [i] / _freqBandHighest [i]), 0, 1);
		}
	}



	void GetSpectrumAudioSource()
	{
		if (_listenToAudioListener) {
			AudioListener.GetSpectrumData (_samplesLeft, 0, FFTWindow.Hanning);
			AudioListener.GetSpectrumData (_samplesRight, 1, FFTWindow.Hanning);
		}
		if (!_listenToAudioListener) {
			_audioSource.GetSpectrumData (_samplesLeft, 0, FFTWindow.Hanning);
			_audioSource.GetSpectrumData (_samplesRight, 1, FFTWindow.Hanning);
		}
	}


	void BandBuffer()
	{
		for (int g = 0; g < 8; ++g) {
			if (_freqBand [g] > _bandBuffer [g]) {
				_bandBuffer [g] = _freqBand [g];
				_bufferDecrease [g] = 0.005f;
			}

			if ((_freqBand [g] < _bandBuffer [g]) && (_freqBand [g] > 0)) {
				_bandBuffer[g] -= _bufferDecrease [g];
				_bufferDecrease [g] *= 1.2f;
			}

		}
	}



	void MakeFrequencyBands()
	{
		int count = 0;

		for (int i = 0; i < 8; i++) {


			float average = 0;
			int sampleCount = (int)Mathf.Pow (2, i) * 2;

			if (i == 7) {
				sampleCount += 2;
			}
			for (int j = 0; j < sampleCount; j++) {
				if (channel == _channel.Stereo) {
					average += (_samplesLeft [count] + _samplesRight [count]) * (count + 1);
				}
				if (channel == _channel.Left) {
					average += _samplesLeft [count] * (count + 1);
				}
				if (channel == _channel.Right) {
					average += _samplesRight [count] * (count + 1);
				}
				count++;

			}

			average /= count;

			_freqBand [i] = average * 10;

		}
	}

}