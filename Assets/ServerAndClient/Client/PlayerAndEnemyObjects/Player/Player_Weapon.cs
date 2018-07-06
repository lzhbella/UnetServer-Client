using UnityEngine;
using System.Collections;

public class Player_Weapon : Player_Item
{

    [SerializeField]
    ParticleSystem fire;
    [SerializeField]
    LineRenderer line;
    BulletType bullet;
    [SerializeField]
    Transform barrel;
    public Transform Barrel { get { return barrel; } }
    [SerializeField]
    Transform handle;
    public Transform Handle { get { return handle; } }
    [SerializeField]

    public bool Dropped { get { return dropped; } }

    bool reload;
    public bool Reload { get { return reload; } }

    bool firstShoot;
    LineRenderer[] lines;

    RaycastHit Hit;
    RaycastHit[] hitAll;
    //zombie layer
    int layer = 1 << 10;

    [Header("-----Weapon point in model-----")]
    [SerializeField]
    Vector3 PositionInModel;
    [SerializeField]
    Quaternion RotateInModel;

    void Start()
    {
        lines = new LineRenderer[20];
        lines[0] = line;
        for (int i = 1; i < lines.Length; i++)
        {
            GameObject go = Instantiate(line.gameObject);
            go.transform.SetParent(line.transform.parent, false);
            go.transform.localPosition = line.transform.localPosition;
            go.transform.localRotation = line.transform.localRotation;
            LineRenderer l = go.GetComponent<LineRenderer>();
            lines[i] = l;
        }
    }

    public void Fire(Vector3[] dir, BulletType type)
    {
        StartCoroutine(FireCor(dir));
    }

    //Start fire particle and line render on direction
    IEnumerator FireCor(Vector3[] dir)
    {
        if (line)
        {
            //For standart weapon
            if (bullet == BulletType.Standart || bullet == BulletType.Fraction)
            {
                for (int i = 0; i < dir.Length; i++)
                {
                    lines[i].transform.rotation = Quaternion.LookRotation(dir[i], lines[i].transform.up);
                    if (Physics.Raycast(lines[i].transform.position, lines[i].transform.forward, out Hit, 15, layer))
                    {
                        lines[i].SetPosition(1, new Vector3(0, 0, Hit.distance));
                    }
                    else
                    {
                        lines[i].SetPosition(1, new Vector3(0, 0, 15));
                    }
                    lines[i].enabled = true;
                }
                //for rifle gun and gauss gun
            }
            else if (bullet == BulletType.Through)
            {
                for (int i = 0; i < dir.Length; i++)
                {
                    lines[i].transform.rotation = Quaternion.LookRotation(dir[i], lines[i].transform.up);
                    hitAll = Physics.RaycastAll(lines[i].transform.position, lines[i].transform.forward, 15);
                    line.SetPosition(1, new Vector3(0, 0, 15));
                    if (hitAll.Length > 0)
                    {
                        //If we hit to target
                    }
                    lines[i].enabled = true;
                }
            }
        }
        if (fire)
        {
            fire.Play();
        }
        yield return new WaitForSeconds(0.05f);
        if (lines.Length > 0)
        {
            foreach (LineRenderer l in lines)
            {
                l.enabled = false;
            }
        }
        if (fire)
        {
            fire.Stop();
        }
    }

    //start reload weapon
    public void StartReload(float time)
    {
        reload = true;
        Invoke("ReloadDone", time);
    }

    //If reload done
    void ReloadDone()
    {
        reload = false;
    }

    public override void Drop()
    {
        dropped = true;
        transform.SetParent(null);
    }

    public override void TakeWeapon(Transform parent, Player_AnimateControl controll)
    {
        transform.SetParent(parent);
        transform.localPosition = PositionInModel;
        transform.localRotation = RotateInModel;
        controll.TakeWeapon(this);
        dropped = false;
    }

    public void SetModelPoint()
    {
        PositionInModel = transform.localPosition;
        RotateInModel = transform.localRotation;
    }

    public override void PickUpThisItem()
    {
        base.PickUpThisItem();
    }
}

public enum BulletType : int
{
    Standart,
    Through,
    Fraction
}


