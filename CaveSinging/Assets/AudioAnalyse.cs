using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAnalyse : MonoBehaviour
{
  AudioClip microphoneInput;
  bool microphoneInitialized;
  private float sensitivity = 0.2f;
  public bool On;

  public int maxListLength;
  public List<float> levelRecorder = new List<float>();
  int listLength;

    void Awake()
    {
      if (Microphone.devices.Length>0){
    		microphoneInput = Microphone.Start(Microphone.devices[0],true,999,44100);
    		microphoneInitialized = true;
        Debug.Log(Microphone.devices[0]);
    	}
    }

    void FixedUpdate()
    {
      //get mic volume
      int dec = 128;
      float[] waveData = new float[dec];
      int micPosition = Microphone.GetPosition(null)-(dec+1); // null means the first microphone

      microphoneInput.GetData(waveData, micPosition);

      // Getting a peak on the last 128 samples
      float levelMax = 0;
      for (int i = 0; i < dec; i++) {
        float wavePeak = waveData[i] * waveData[i];
        // Debug.Log(waveData[i]);
        if (levelMax < wavePeak) {
          levelMax = wavePeak;
        }
      }
      float level = Mathf.Sqrt(Mathf.Sqrt(levelMax));
    // ...and then check if level is above the threshold. I use a variable called sensitivity.

      listLength = levelRecorder.Count;
        if (listLength < maxListLength){
            levelRecorder.Add(level-sensitivity);
        }
        else{
          levelRecorder.Remove(levelRecorder[0]);
          levelRecorder.Add(level-sensitivity);
        }
        int positiveCount = 0;
        int negativeCount = 0;
        for (int i = 0; i < listLength; i++){
            if(levelRecorder[i]>0){
              positiveCount++;
            }
            else{
              negativeCount++;
            }
        }
        if(positiveCount>=negativeCount){
          On = true;
        }
        else{
          On = false;
        }
      }

  }
