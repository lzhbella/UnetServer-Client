using UnityEngine;

public class Weapon_Sniper : Player_Weapon_Sr
{
    bool delay = false;
    RaycastHit[] hits;

    void Awake()
    {
        base.Initialize();
    }

    void FixedUpdate()
    {
        base.ReducedRecoil();
    }

    protected override void Shoot()
    {
        if (!delay)
        {
            delay = true;
            ammo--;
            temp.Set(transform.up.x + Random.Range(-tempF, tempF), transform.up.y, transform.up.z);
            tempF += tempF;
            tempF = Mathf.Clamp(tempF, minBulletRage, maxBulletRage);
            //Send shoot all other player
            Message_Sr.Shot_Sr shot = new Message_Sr.Shot_Sr();
            shot.index = shooterIndex;
            shot.type = type;
            shot.vect = new Vector3[1];
            shot.vect[0] = temp;
            shot.thisPlayer = false;
            RoomsManager.SendToAllOtherPlayer(Networking_msgType_Sr.Fire, shot, shooterIndex);
            //Send shoot me
            shot.thisPlayer = true;
            shot.ammo = ammo;
            RoomsManager.SendToThisPlayer(Networking_msgType_Sr.Fire, shot, shooterIndex);

            hits = Physics.RaycastAll(transform.position, temp.normalized, shotDistace, layerZombie, QueryTriggerInteraction.Ignore);
            if (hits.Length > 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    Player_MovePlayer zombie = hit.transform.GetComponent<Player_MovePlayer>();
                    if (zombie != null)
                    {
                        zombie.ZombieDamage(damage, shooterIndex);
                    }
                }
            }
            if (ammo <= 0)
            {
                StartReload();
                delay = false;
            }
            else
            {
                Invoke("Delay", shotTime);
            }
        }
    }

    void Delay()
    {
        delay = false;
    }
}
