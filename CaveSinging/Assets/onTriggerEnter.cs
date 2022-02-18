using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onTriggerEnter : MonoBehaviour
{

	public SpawnManager spawnManager;

    // Start is called before the first frame update
    void Start()
    {
        spawnManager = GetComponent<SpawnManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter(Collider other){
		if (other.gameObject.CompareTag ("Trigger")) {
			spawnManager.moveTunnel();
		}
	}
}
