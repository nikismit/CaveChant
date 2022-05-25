using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroOpacityControl : MonoBehaviour
{
  public RawImage _rawImage;
  public Color alpha;
  public AudioAnalyse audioAnalyse;
  public WalkMovement walkMovement;
  public VideoPlayer VP;
  public float clock;
  [Range(0.0f, 1.0f)]
  public float DecreaseRate;
  public bool NextScene;
  public List<float> UpOrDown = new List<float>();
  public AudioSource Flame;
  public bool AtmosFadeOut;
  public AudioSource OptionalAtmos;

    void Start()
    {
        _rawImage = GetComponent<RawImage>();
        alpha= _rawImage.color;
        alpha.a = 0f;
        NextScene = false;
        VP.playOnAwake = false;
    }

    public void prevValue(){
      if (UpOrDown.Count <= 3){
        UpOrDown.Add(alpha.a);
      }
      else if(UpOrDown.Count >= 4){
        UpOrDown.Remove(UpOrDown[0]);
        UpOrDown.Add(alpha.a);
      }
    }

    public void PlayNextScene(){
      SceneManager.LoadScene(1);
    }


    void FixedUpdate()
    {
      _rawImage.color = alpha;
      prevValue();
      if (audioAnalyse.On == true && alpha.a < 1){
        alpha.a += DecreaseRate;
        clock += DecreaseRate;
        VP.Play();
        NextScene = false;
        if (alpha.a > 0.05f){
          Flame.Play();
        }
      }

      else if (audioAnalyse.On == false && alpha.a > 0.01f){
        alpha.a -= DecreaseRate;
        Flame.volume -= DecreaseRate;
        if (AtmosFadeOut == true){
          OptionalAtmos.volume -= DecreaseRate;
        }
        if (alpha.a < 0.02f && UpOrDown.Count > 3){
          VP.Stop();
          NextScene = true;
        }
      }

      if (NextScene == true){
        PlayNextScene();
      }

    }
}
