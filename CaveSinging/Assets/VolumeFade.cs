using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeFade : MonoBehaviour
{
  public AudioSource AS;
  [Range(0.0f, 1.0f)]
  public float Decrease;
  private float Vol;
    // Start is called before the first frame update
    void Start()
    {
        Vol = AS.volume;
    }

    // Update is called once per frame
    void Update()
    {
        Vol -= Decrease;
        AS.volume = Vol;
    }
}
