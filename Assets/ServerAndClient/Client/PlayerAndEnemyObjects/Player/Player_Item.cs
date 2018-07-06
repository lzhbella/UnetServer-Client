using UnityEngine;

public abstract class Player_Item : MonoBehaviour
{

    public int index = -1;

    Vector3 oldPos;
    Quaternion oldRot;

    Vector3 newVect;
    Quaternion newRotate;

    static float lerpTime = 7f;
    protected bool dropped = false;

    bool sync;

    // Use this for initialization
    void Start()
    {
        oldPos = transform.position;
        oldRot = transform.rotation;
        newVect = transform.position;
        newRotate = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (dropped)
        {
            if (sync)
            {
                if (newVect != oldPos || newRotate != oldRot)
                {
                    transform.position = Vector3.Lerp(transform.position, newVect, lerpTime * Time.deltaTime);
                    transform.rotation = Quaternion.Lerp(transform.rotation, newRotate, lerpTime * Time.deltaTime);
                    oldPos = transform.position;
                    oldRot = transform.rotation;
                }
                else
                {
                    sync = false;
                }
            }
        }
    }

    public void SyncPosition(Vector3 vect, Quaternion rotate)
    {
        newVect = vect;
        newRotate = rotate;
        sync = true;
    }

    public abstract void Drop();

    public abstract void TakeWeapon(Transform parent, Player_AnimateControl controll);

    public virtual void PickUpThisItem()
    {
        if (index != -1)
        {
            Message.PickUpItem up = new Message.PickUpItem();
            up.itemIndex = index;
            Networking_client.net.SendUnreliable(Networking_msgType.PickUp, up);
        }
    }
}

public enum ItemType : int
{
    weapon = 1
}

public enum weaponType : int
{
    Machine = 1,
    ShotGun,
    Rifle,
    GaussGun
}