using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameScaleController : MonoBehaviour
{
  public float _intensity;
  public AudioAnalyse audioAnalyse;
  public WalkMovement walkController;

    void Start()
    {
          _intensity = 0f;
    }


    void Update()
    {
      if (audioAnalyse.On == true && _intensity < 1.0f){
        _intensity += walkController.lightUpRate*3;
      }
      else if(audioAnalyse.On == false && _intensity > 0f){
        _intensity -= walkController.glowDecayRate*0.5f;
      }
      transform.localScale = new Vector3(_intensity, _intensity, _intensity);
    }
}
