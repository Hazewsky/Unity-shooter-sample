using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {

    Gun equippedGun;
    public Transform weaponHold;
    public Gun[] allGuns;

    

    public void EquipGun(Gun gunToEqiup)
    {
        if (equippedGun != null)
            Destroy(equippedGun.gameObject);
        equippedGun = Instantiate(gunToEqiup, weaponHold.position, weaponHold.rotation) as Gun;
        //become shild to able to transform
        equippedGun.transform.parent = weaponHold;

    }

    public void EquipGun(int weaponIndex)
    {
        EquipGun(allGuns[weaponIndex]);
    }

    public void OnTriggerHold()
    {
        if(equippedGun != null)
            equippedGun.OnTriggerHold();
    }

    public void OnTriggerRelease(){
        if (equippedGun != null)
            equippedGun.OnTriggerRelease();
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void aim(Vector3 aimPoint)
    {
        if (equippedGun != null)
        {
            equippedGun.Aim(aimPoint);
        }
    }

    public void Reload()
    {
        if(equippedGun != null)
        {
            equippedGun.Reload();
        }
    }


    public float GunHeight
    {
       get {
            return weaponHold.position.y;
        }
    }
}
