using UnityEngine;
using UnityEngine.Networking;

public class Player_Item_Sr : MonoBehaviour {

    public int index = -1;
    public int roomID = -1;

    Vector3 oldPos;
    Quaternion oldRot;
    protected ItemType_Sr itemType;
    public ItemType_Sr ItemType { get { return itemType; } }
    protected weaponType_Sr weapon;
    public weaponType_Sr WeaponType { get { return weapon; } }

    bool syncPos = false;
    protected bool updatePos = true;
    static float syncTime = 0.07f;
	
	// Update is called once per frame
	protected virtual void Sync () {
        if (updatePos)
        {
            if (transform.position != oldPos || transform.rotation != oldRot)
            {
                if (!syncPos)
                {
                    syncPos = true;
                    SyncPosition();

                    oldPos = transform.position;
                    oldRot = transform.rotation;
                }
                else
                {
                    if (syncPos)
                    {
                        syncPos = false;
                        CancelInvoke("SyncPosition");
                    }
                }
            }
        }
	}

    void SyncPosition()
    {
        NetworkWriter wr = new NetworkWriter();
        wr.StartMessage(Networking_msgType_Sr.ItemPositionUpdate);
        wr.Write(index);
        wr.Write(transform.position);
        wr.Write(transform.rotation);
        wr.FinishMessage();
        RoomsManager.SendUnreliableToRoom(wr, roomID);
        Invoke("SyncPosition", syncTime);
    }
}

public enum ItemType_Sr : int
{
    weapon = 1
}