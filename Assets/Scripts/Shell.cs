using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour {

    public Rigidbody myRigidBody;
    public float forceMin,forceMax;

    float lifetime = 4;
    float fadetime = 2;
	// Use this for initialization
	void Start () {
        float force = Random.Range(forceMin, forceMax);
        //shift
        myRigidBody.AddForce(transform.right * force);
        //rotation by random vector
        myRigidBody.AddTorque(Random.insideUnitSphere * force);
        StartCoroutine(Fade());
	}
	
    IEnumerator Fade()
    {
        //wait for time to expire before fading
        yield return new WaitForSeconds(lifetime);

        float percent = 0;
        float fadeSpeed = 1 / fadetime;
        Material mat = GetComponent<Renderer>().material;
        Color initColor = mat.color;
        while(percent < 1)
        {
            percent += Time.deltaTime * fadeSpeed;
            mat.color = Color.Lerp(initColor, Color.clear, percent);
            yield return null;
        }
        Destroy(gameObject);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
