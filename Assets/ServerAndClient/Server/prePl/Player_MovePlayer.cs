using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
#if UNITY_5_5_OR_NEWER
using UnityEngine.AI;
#endif

public class Player_MovePlayer : MonoBehaviour
{
    [SerializeField]
    Transform Weapon;
    public Transform weapon { get { return Weapon; } }
    //Connection of this player
    public NetworkConnection playerCon;
    [HideInInspector]
    public Player_Weapon_Sr weaponOnMe;
    public int index;
    Rigidbody controll;
    CapsuleCollider coll;

    //Move data
    [HideInInspector]
    public Vector3 rotate;
    [HideInInspector]
    public float axisY;
    Vector3 move;
    Vector3 oldPos;
    bool w;
    bool s;
    bool a;
    bool d;
    bool syncMove;
    float tempF;
    Vector3 Vect;

    //Player shoot data
    bool reload;
    RaycastHit hit;
    public float defBulletRage = 0.001f;
    public float bulletRageLerp = 0.1f;
    public float maxBulletRage;
    

    //If true this zombie else player
    [HideInInspector]
    public bool zombie;

    //Zombie controll
    NavMeshAgent nav;

    //Zombie target
    Player_MovePlayer target;
    Vector3 targetOldPos = Vector3.zero;
    private Quaternion oldRotate;

    //Zombie attack data
    bool attack;
    List<int> ZombieFollowMe = new List<int>();

    //If this object death
    public bool death = false;

    //Sync time move and rotate
    static float syncTime = 0.07f;
    public float speed = 3.5f;
    public ForceMode force;

    void Start()
    {
        controll = GetComponent<Rigidbody>();
        if (zombie)
        {
            coll = GetComponent<CapsuleCollider>();
            nav = GetComponent<NavMeshAgent>();
            nav.stoppingDistance = 1.8f;
        }
        oldPos = transform.position;
        oldRotate = transform.rotation;
    }

    //If zombie follow on player
    public void AddFollowZombie(int index)
    {
        if (!ZombieFollowMe.Contains(index))
        {
            ZombieFollowMe.Add(index);
        }
    }

    /// <summary>
    /// If zombie stop follow player
    /// </summary>
    /// <param name="indexZombie">stoped follow zombie</param>
    public void ZombieStopFollow(int indexZombie)
    {
        int id = -1;
        id = ZombieFollowMe.IndexOf(indexZombie);
        if (id != -1)
        {
            ZombieFollowMe.RemoveAt(id);
        }
    }

    /// <summary>
    /// Damage zombie
    /// </summary>
    /// <param name="damage">damage</param>
    /// <param name="indexPlayer">player index</param>
    public void ZombieDamage(int damage, int indexPlayer)
    {
        //Take damage
        Data_PlayerFile_Sr zm = RoomsManager.GetPlayerData(index);
        zm.Damage(damage);
        //If zombie not have target follow the attacking
        if (target == null)
        {
            Player_MovePlayer player = RoomsManager.GetPlayerController(indexPlayer);
            if (player)
            {
                target = player;
                player.AddFollowZombie(index);
            }
        }
        //If zombie death
        if (zm.HP <= 0)
        {
            Death();
            string zmName = RoomsManager.GetPlayerData(index).nick;
            if (!string.IsNullOrEmpty(zmName))
            {
                SQL_SavePlayerData.Kill(indexPlayer, zmName);
            }
        }
    }

    //If player click on fire
    public void StartFire()
    {
        if (!death)
        {
            if (!w && !s && !d && !a)
            {
                if (weaponOnMe)
                {
                    weaponOnMe.Fire();
                    Debug.Log(string.Format("Player {0} fire", index));
                }
            }
        }
    }

    //Zombie start attack and block move
    public void ZombieAttack()
    {
        attack = true;
        NetworkWriter wr = new NetworkWriter();
        wr.StartMessage(Networking_msgType_Sr.ZombieAttack);
        wr.Write(index);
        wr.FinishMessage();
        RoomsManager.SendReliableAtRoom(wr, index);
        Invoke("ZombieAttackDone", 1.8f);
    }

    //Zombie attack done and can move
    void ZombieAttackDone()
    {
        if (target != null)
        {
            Player_MovePlayer targ = target.GetComponent<Player_MovePlayer>();
            if (targ != null)
            {
                Data_PlayerFile_Sr data = RoomsManager.GetPlayerData(targ.index);
                data.Damage(RoomsManager.GetPlayerData(index).attackPower);
                if (data.HP <= 0)
                {
                    targ.Death();
                }
            }
        }
        attack = false;
    }

    //If player stop fire
    public void StopFire()
    {
        if (weaponOnMe)
        {
            weaponOnMe.StopFire();
        }
    }

    //If player click reload weapon, block fire
    public void StartReload()
    {
        if (weaponOnMe)
        {
            weaponOnMe.StartReload();
            
        }
    }

    /// <summary>
    /// Player drop weapon if he have him
    /// </summary>
    public void DropWeapon()
    {
        if (weaponOnMe)
        {
            weaponOnMe.DropWeapon(true);

            Message_Sr.DropWeapon_Sr dropped = new Message_Sr.DropWeapon_Sr();
            dropped.index = index;
            RoomsManager.SendReliableAtRoom(Networking_msgType_Sr.DropWeapon, dropped, index);
        }
    }

    

    //If player click on button W, S, A, D
    public void Newposit(int key, bool down)
    {
        KeyCode code = (KeyCode)key;

        if (code == KeyCode.W)
        {
            if (down)
            {
                w = true;
            }
            else
            {
                w = false;
            }
        }
        else if (code == KeyCode.S)
        {
            if (down)
            {
                s = true;
            }
            else
            {
                s = false;
            }
        }
        else if (code == KeyCode.A)
        {
            if (down)
            {
                a = true;
            }
            else
            {
                a = false;
            }
        }
        else if (code == KeyCode.D)
        {
            if (down)
            {
                d = true;
            }
            else
            {
                d = false;
            }
        }
    }

    void Update()
    {
        if (!death)
        {
            //If zombie see player
            if (zombie)
            {
                //If zombie don't have target
                if (target)
                {
                    //If target move
                    if (targetOldPos != target.transform.position)
                    {
                        if (!attack)
                        {
                            nav.destination = target.transform.position;
                            targetOldPos = target.transform.position;
                        }
                    }
                    Vect = (target.transform.position - transform.position);
                    //Check zombie attack distance
                    if (1.8f >= Vect.magnitude)
                    {
                        //If zombie can attack and don't attack now
                        if (!attack)
                        {
                            //Stop move
                            nav.destination = transform.position;
                            //Call attack 
                            ZombieAttack();
                        }
                    }
                }
            }
        }

        //If position or rotate change
        if (transform.position != oldPos || transform.rotation != oldRotate)
        {
            if (!syncMove)
            {
                syncMove = true;
                SyncMove();
            }
        }
        else
        {
            if (syncMove)
            {
                syncMove = false;
            }
        }
        oldPos = transform.position;
        oldRotate = transform.rotation;
    }

    //Sync move target
    void SyncMove()
    {
        Message_Sr.PlayerGoTo_Sr run = new Message_Sr.PlayerGoTo_Sr();
        run.index = index;
        run.sendVec = transform.position;
        run.rotate = transform.rotation;
        RoomsManager.SendReliableAtRoom(Networking_msgType_Sr.PlayerMove, run, index);
        if (syncMove)
        {
            Invoke("SyncMove", syncTime);
        }
    }

    //If follow target death
    public void FollowTargetDead()
    {
        target = null;
        attack = false;
        nav.destination = transform.position;
        CancelInvoke();
    }

    void FixedUpdate()
    {
        if (!death)
        {
            if (!zombie)
            {
                rotate.Set(transform.eulerAngles.x, axisY, transform.eulerAngles.z);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotate), 12f * Time.fixedDeltaTime);
                move = Vector3.zero;
                if (w)
                {
                    move += transform.TransformDirection(Vector3.forward);
                }
                if (s)
                {
                    move -= transform.TransformDirection(Vector3.forward);
                }
                if (a)
                {
                    move -= transform.TransformDirection(Vector3.right);
                }
                if (d)
                {
                    move += transform.TransformDirection(Vector3.right);
                }

                if (move != Vector3.zero)
                {
                    controll.MovePosition(transform.position + move * speed * Time.deltaTime);
                }
            }
        }
    }

    //If player standing close on zombie
    void OnTriggerStay(Collider col)
    {
        if (!death)
        {
            if (target == null)
            {
                //Check if this player
                if (col.CompareTag("Player"))
                {
                    Player_MovePlayer pl = col.gameObject.GetComponent<Player_MovePlayer>();
                    if (!pl.death)
                    {
                        Vect = col.transform.position - transform.position;
                        float angle = Vector3.Angle(transform.forward, Vect);
                        //Check zombie see player?
                        if (angle <= 90f)
                        {
                            target = pl;
                            pl.AddFollowZombie(index);
                        }
                        else
                        {
                            //If zombie don't see player but player wery close to zombie
                            if (4f > Vect.magnitude)
                            {
                                target = pl;
                                pl.AddFollowZombie(index);
                            }
                        }
                    }
                }
            }
        }
    }

    //If target respawn
    //If zombie respawn on random position on map
    //If player respawn on player layer on map
    public void Respawn()
    {
        death = false;
        if (zombie)
        {
            coll.enabled = true;
            do
            {
                Vect = new Vector3(Random.Range(0, 100), transform.position.y, Random.Range(0, 100));
            }
            while (Vect.x < 20f || Vect.z < 20f);
            nav.Warp(Vect);
        }
        else
        {
            do
            {
                Vect = new Vector3(Random.Range(1, 10), transform.position.y, Random.Range(1, 10));
            }
            while (Vect.x > 10f || Vect.z > 10f);
            transform.position = Vect;
        }
        transform.rotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
        Data_PlayerFile_Sr d = RoomsManager.GetPlayerData(index);
        d.SetHP(d.HPMax);
        NetworkWriter wr = new NetworkWriter();
        wr.StartMessage(Networking_msgType_Sr.Respawn);
        wr.Write(index);
        wr.Write(transform.position);
        wr.Write(transform.rotation);
        wr.FinishMessage();
        RoomsManager.SendReliableAtRoom(wr, index);

    }

    //If target death
    //if zombie respawn automaticly
    //If player block move and fire, wait click respawn
    void Death()
    {
        death = true;
        NetworkWriter wr = new NetworkWriter();
        wr.StartMessage(Networking_msgType_Sr.Death);
        wr.Write(index);
        wr.FinishMessage();
        RoomsManager.SendReliableAtRoom(wr, index);
        if (zombie)
        {
            coll.enabled = false;
            CancelInvoke();
            Invoke("Respawn", 20f);
            target.ZombieStopFollow(index);
            target = null;
            nav.destination = transform.position;
            CalculateDropChance();
        }
        else
        {
            w = false;
            s = false;
            a = false;
            d = false;
            move = Vector3.zero;

            while (ZombieFollowMe.Count != 0)
            {
                RoomsManager.GetPlayerController(ZombieFollowMe[0]).FollowTargetDead();
                ZombieFollowMe.RemoveAt(0);
            }
        }
    }

    void CalculateDropChance()
    {
        Data_PlayerFile_Sr zombie = RoomsManager.GetPlayerData(index);
        int dropID = 0;
        if (zombie.HPMax >= 100)
        {
            dropID = 2;
            if (zombie.HPMax >= 200)
            {
                dropID = Random.Range(2, 3);
            }
            if (zombie.HPMax >= 300)
            {
                dropID = Random.Range(3, 4);
            }

            float chance = zombie.HPMax * zombie.attackPower / (float)dropID / 1000;

            Random.seed = (int)Time.time;
            float var1 = Random.value;
            Random.seed = (int)Time.time * Random.Range(3, 7);
            float var2 = Random.value;

            if (Mathf.Abs(var1 - var2) < chance)
            {
                AccountData acc = Networking_OnConnect.GetAccountData(index);

                Player_Weapon_Sr weap = RoomsManager.InstantiateWeapon((weaponType_Sr)dropID, transform.position, acc.roomID);
                weap.DropWeapon(true);
            }
        }
    }
}