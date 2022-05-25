using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameController : MonoBehaviour
{
  public Material flameMat;
  public float _intensity;
  public Color _emissionColorValue;
  public AudioAnalyse audioAnalyse;
  public WalkMovement walkController;

  private float sat = 0.94f;
  private float hue = 0.32f;

    // Start is called before the first frame update
    void Start()
    {
      _intensity = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
      if (audioAnalyse.On == true && _intensity < 1.0f){
        _intensity += walkController.lightUpRate*3;
      }
      else if(audioAnalyse.On == false && _intensity > 0f){
        _intensity -= walkController.lightDecayRate;
      }

      var _renderer = GetComponent<Renderer>();
      _renderer.material.SetVector("_Color", _emissionColorValue * _intensity);
      // flameMat.SetVector("_MainColor", _emissionColorValue * _intensity);
      //flameMat.color = _emissionColorValue.HSVToRGB(hue, sat, _intensity);
      // flameMat.color = _emissionColorValue;



    }
}
