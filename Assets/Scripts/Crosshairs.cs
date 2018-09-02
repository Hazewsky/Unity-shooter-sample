using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour {

    public LayerMask targetMask;
    public Color dotHighlightColor;
    Color origDotColor;
    public SpriteRenderer dot;
    public void DetectTargets(Ray ray)
    {
        if (Physics.Raycast(ray, 100, targetMask))
        {
            Cursor.visible = false;
            dot.color = dotHighlightColor;
        }
        else
        {
            dot.color = origDotColor;
        }
    }
	// Use this for initialization
	void Start () {
        origDotColor = dot.color;

	}
	
	// Update is called once per frame 
	void Update () {
        transform.Rotate(Vector3.forward * 40 * Time.deltaTime);
	}
}
