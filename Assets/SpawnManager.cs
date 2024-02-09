using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
public GameObject tunnel01;
public GameObject tunnel02;
public List<GameObject> goList;
public bool ON;
    
    void Start()
    {
        goList.Add(tunnel01);
	goList.Add(tunnel02);
	ON = false;
    }

    
    void FixedUpdate()
    {
    }
	public void moveTunnel(){
		GameObject movedTunnel = goList[0];
		float newZ = goList[0].transform.position.z + 16f;
		movedTunnel.transform.position = new Vector3 (0, 0, newZ);
		//movedTunnel.transform.Translate(0, 0, newZ);
		goList.Remove(movedTunnel);
		goList.Add(movedTunnel);
	}

}


