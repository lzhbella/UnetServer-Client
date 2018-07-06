using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class Player_Weapon_Sr : Player_Item_Sr
{

    //Physics
    Rigidbody rig;
    BoxCollider box;

    [Header("----Weapon setting----")]
    [SerializeField]
    [Tooltip("Damage weapon")]
    protected int damage = 5;
    [SerializeField]
    [Tooltip("Reload time in second")]
    protected float reloadTime = 2;
    [SerializeField]
    //Maximum ammo weapon magazine
    [Tooltip("Maximum ammo in magazine")]
    int ammoNumber = 1;
    public int maxAmmo { get { return ammoNumber; } }
    //Now ammo in weapon
    protected int ammo = 1;
    [SerializeField]
    [Tooltip("Number of bullets in one shot")]
    protected int bulletsNumberAtShot = 1;
    [SerializeField]
    [Tooltip("Maximum shot distance")]
    protected float shotDistace = 15f;
    [SerializeField]
    [Tooltip("The interval between shot. If interval equal 0 then one click one shot. If the sniper weapon delay between shots.")]
    protected float shotTime = 0;
    [SerializeField]
    [Tooltip("Minimum spread")]
    protected float minBulletRage = 0.005f;
    [SerializeField]
    [Tooltip("Maximum spread")]
    protected float maxBulletRage = 0.01f;
    [SerializeField]
    [Tooltip("How fast reduced recoil")]
    protected float bulletRageLerp = 0.1f;
    //Who take this weapon
    protected int shooterIndex = -1;
    public int ShoterIndex { get { return shooterIndex; } }
    //If weapon reload
    protected bool reload = false;
    //If weapon droped
    private bool dropped = true;
    public bool Dropped { get { return dropped; } }

    //Temp values
    protected RaycastHit hit;
    protected float tempF;
    protected Vector3 temp;

    //Const
    protected int layerZombie = 1 << 8;
    Vector3 euler = new Vector3(90, 0, 0);

    [SerializeField]
    protected BulletType_Sr type;
    [SerializeField]
    protected weaponType_Sr typeWeapon;

    bool waitDestroy;

    protected virtual void Initialize()
    {
        GetPhysics();
        itemType = ItemType_Sr.weapon;
        weapon = typeWeapon;
        ammo = maxAmmo;
    }

    protected virtual void GetPhysics()
    {
        rig = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();
    }

    public virtual void Fire()
    {
        if (shooterIndex != -1)
        {
            if (!reload)
            {
                Shoot();
            }
        }
    }

    protected virtual void ReducedRecoil()
    {
        tempF = Mathf.Lerp(tempF, minBulletRage, bulletRageLerp);
    }

    protected abstract void Shoot();

    public virtual void StopFire()
    {
        if (shotTime > 0)
        {
            CancelInvoke("Shoot");
        }
    }

    public virtual void StartReload()
    {
        reload = true;
        Message_Sr.Reload_Sr rel = new Message_Sr.Reload_Sr();
        rel.index = shooterIndex;
        rel.reloadTime = reloadTime;
        RoomsManager.SendUnreliableToRoom(Networking_msgType_Sr.Reload, rel, roomID);
        Invoke("ReloadDone", reloadTime);
    }

    protected virtual void ReloadDone()
    {
        ammo = maxAmmo;
        reload = false;
    }

    /// <summary>
    /// Droped weapon
    /// </summary>
    /// <param name="force">Add force if true</param>
    public virtual void DropWeapon(bool force)
    {
        if (shooterIndex != -1)
        {
            Player_MovePlayer controll = RoomsManager.GetPlayerController(shooterIndex);
            controll.weaponOnMe = null;
        }
        shooterIndex = -1;
        updatePos = true;
        dropped = true;
        transform.SetParent(null);
        SetPhysics(true);
        if (force)
        {
            rig.AddForce(transform.up * 2, ForceMode.Force);
        }
    }

    /// <summary>
    /// Player take this weapon
    /// </summary>
    /// <param name="obj">hand</param>
    /// <param name="sessionID">player index</param>
    public virtual void TakeWeapon(Transform obj, int sessionID)
    {
        updatePos = false;
        dropped = false;
        SetPhysics(false);
        transform.SetParent(obj, false);
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = euler;
        shooterIndex = sessionID;
        Player_MovePlayer controll = RoomsManager.GetPlayerController(sessionID);
        controll.weaponOnMe = this;
        //Send action to all player
        Message_Sr.PickUpWeapon_Sr up = new Message_Sr.PickUpWeapon_Sr();
        up.index = controll.index;
        up.indexItem = index;
        up.thisPlayer = false;
        RoomsManager.SendToAllOtherPlayer(Networking_msgType_Sr.PickUp, up, controll.index);
        up.thisPlayer = true;
        up.ammo = ammo;
        up.ammoMax = maxAmmo;
        RoomsManager.SendToThisPlayer(Networking_msgType_Sr.PickUp, up, controll.index);
    }

    /// <summary>
    /// If true object physics
    /// </summary>
    /// <param name="b">physic</param>
    void SetPhysics(bool b)
    {
        if (b)
        {
            box.isTrigger = false;
            rig.useGravity = true;
            rig.isKinematic = false;
        }
        else
        {
            box.isTrigger = true;
            rig.useGravity = false;
            rig.isKinematic = true;
        }
    }

    void Update()
    {
        base.Sync();

        if (dropped)
        {
            if (!waitDestroy)
            {
                Invoke("DestroyItem", 60);
                waitDestroy = true;
            }
        }
        else
        {
            if(waitDestroy)
            {
                CancelInvoke("DestroyItem");
                waitDestroy = false;
            }
        }
    }

    void DestroyItem()
    {
        RoomsManager.DestroyItemInRoom(index, roomID);
        RoomsManager.SendIntToAllRoom(Networking_msgType_Sr.RemoveItemOnScene, index, roomID);
        Destroy(gameObject);
    }
}

public enum weaponType_Sr : int
{
    Machine = 1,
    ShotGun,
    Rifle,
    GaussGun
}

public enum BulletType_Sr : int
{
    Standart,
    Through,
    Fraction
}
