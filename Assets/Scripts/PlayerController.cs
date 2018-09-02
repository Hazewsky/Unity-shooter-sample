using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {


    Rigidbody myRigidBody;
    Vector3 velocity;
    Vector3 offset;
    

	void Start () {
        myRigidBody = GetComponent<Rigidbody>();
        offset = Camera.main.transform.position - myRigidBody.position;
    }
	
	public void Move(Vector3 _velocity)
    {
       
        velocity = _velocity;
    }

    private void FixedUpdate()
    {
        //move rigidbody
        //execute by little steps
        myRigidBody.MovePosition(myRigidBody.position + velocity * Time.fixedDeltaTime);
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, myRigidBody.position + offset, .5f);
        //Camera.main.transform.LookAt(myRigidBody.transform);
       
    }
    public void LookAt(Vector3 lookPoint)
    {
        //raise point to not to see at the feet
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }
    void Update () {
		
	}
}
