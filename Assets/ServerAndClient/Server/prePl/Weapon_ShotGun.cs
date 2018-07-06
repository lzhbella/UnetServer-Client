using UnityEngine;

public class Weapon_ShotGun : Player_Weapon_Sr {

    Vector3[] shotVect;

    void Awake()
    {
        base.Initialize();
        shotVect = new Vector3[bulletsNumberAtShot];
    }

    void FixedUpdate()
    {
        base.ReducedRecoil();
    }

    protected override void Shoot()
    {
        ammo--;
        for (int i = 0; i < shotVect.Length; i++)
        {
            shotVect[i].Set(transform.up.x + Random.Range(-tempF, tempF), transform.up.y, transform.up.z);
        }
        tempF += tempF;
        tempF = Mathf.Clamp(tempF, minBulletRage, maxBulletRage);
        //Send shoot all other player
        Message_Sr.Shot_Sr shot = new Message_Sr.Shot_Sr();
        shot.index = shooterIndex;
        shot.type = type;
        shot.vect = new Vector3[shotVect.Length];
        for (int i = 0; i < shotVect.Length; i++)
        {
            shot.vect[i] = shotVect[i];
        }
        shot.thisPlayer = false;
        RoomsManager.SendToAllOtherPlayer(Networking_msgType_Sr.Fire, shot, shooterIndex);
        //Send shoot me
        shot.thisPlayer = true;
        shot.ammo = ammo;
        RoomsManager.SendToThisPlayer(Networking_msgType_Sr.Fire, shot, shooterIndex);

        foreach (Vector3 v in shotVect)
        {
            if (Physics.Raycast(transform.position, v.normalized, out hit, shotDistace, layerZombie, QueryTriggerInteraction.Ignore))
            {
                Player_MovePlayer zombie = hit.transform.GetComponent<Player_MovePlayer>();
                if (zombie != null)
                {
                    zombie.ZombieDamage(damage, shooterIndex);
                }
            }
        }
        if (ammo > 0)
        {
            if (shotTime > 0)
            {
                Invoke("Shoot", shotTime);
            }
        }
        else
        {
            StartReload();
        }
    }
}
