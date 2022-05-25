using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
  public bool Pressed = false;
  public Light Test;

    void Update()
    {
      if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began){
        Test.intensity = 200f;
      }
    }
}
