using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpacityController : MonoBehaviour
{
  public RawImage _rawImage;
  public Color alpha;
  public AudioAnalyse audioAnalyse;
  public WalkMovement walkMovement;
  public float clock;
  [Range(0.0f, 5f)]
  public float buffer;

  public PauseMenuController ButtonPress;
    // Start is called before the first frame update
    void Start()
    {
        _rawImage = GetComponent<RawImage>();
        alpha= _rawImage.color;
        alpha.a = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
      _rawImage.color = alpha;
      if (audioAnalyse.On == false && alpha.a < 1){
        clock += 0.02f;
        if (clock > buffer){
          alpha.a += walkMovement.glowDecayRate;
        }
        // alpha.a += walkMovement.glowDecayRate;
      }
      else if (audioAnalyse.On == true && alpha.a > 0){
        alpha.a -= walkMovement.lightDecayRate;
        clock = 0f;
      }
      // if (ButtonPress.Pressed == true && alpha.a < 1){
      //   clock += 0.02f;
      //   if (clock > buffer){
      //     alpha.a += walkMovement.glowDecayRate;
      //   }
      //   // alpha.a += walkMovement.glowDecayRate;
      // }
      // else if (ButtonPress.Pressed == false && alpha.a > 0){
      //   alpha.a -= walkMovement.lightDecayRate;
      //   clock = 0f;
      // }
    }
}
