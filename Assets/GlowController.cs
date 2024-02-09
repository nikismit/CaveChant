using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowController : MonoBehaviour
{

	public Color alpha;
	public SpriteRenderer m_SpriteRenderer;

	
    void Start()
    {
        alpha = GameObject.FindGameObjectWithTag("Glow").GetComponent<SpriteRenderer>().color;
	alpha.a = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_SpriteRenderer.color = alpha;
    }
}
