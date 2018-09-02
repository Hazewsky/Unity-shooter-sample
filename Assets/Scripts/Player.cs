using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//force to add PlayerCOntroller
[RequireComponent (typeof(PlayerController), typeof(GunController), typeof(Crosshairs))]
public class Player : Entity {

    public float moveSpeed = 5;
    PlayerController controller;
    Camera viewCamera;
    GunController gunController;
    public Crosshairs crosshairs;
    // Use this for initialization

    protected override void Start () {
        base.Start();
       
	}

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNum)
    {
        FullHealth();
        gunController.EquipGun(waveNum - 1);
    }

	// Update is called once per frame
	void Update () {
        MovementInput();
        LookInput();
        WeaponInput();
        //fall off insta death
        if(transform.position.y < -10)
        {
            TakeDamage(health);
        }

	}


    void WeaponInput()
    {
        //left mb
        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }
    }

     void LookInput()
    {
        //get mouse position/ Return ray from camera through this pos
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        //1 - normal, direction
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);
        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            //len from camera to intersection
            Vector3 point = ray.GetPoint(rayDistance);
            // Debug.DrawLine(ray.origin, point,Color.green);
            controller.LookAt(point);
            crosshairs.transform.position = point;
            crosshairs.DetectTargets(ray);
            //distance check for aiming crosshair. if too close - stop aiming
            //sqrMagnitude is faster than magnitude
            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1)
            {
                gunController.aim(point);
            }

        }
    }
    void MovementInput()
    {
        // Raw - no default smoothing
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        //turn to direction
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

       
    }
    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }


}
