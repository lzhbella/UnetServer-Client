using UnityEngine;
using System.Collections.Generic;


public class Player_AnimateControl : MonoBehaviour
{
    [SerializeField]
    Player_Weapon weapon;
    Animator anim;
    [SerializeField]
    private float moveLerpRate = 7f;
    [SerializeField]
    Transform RightHand;
    public Transform rightHand { get { return RightHand; } }

    Vector3 newPos;
    float move;

    private Vector3 oldPos;
    private Vector3 vect;
    private float value;
    [SerializeField]
    private float magnStr = 100f;
    float strZ;
    float strX;
    private float angle;
    //player index
    public int index;

    //If this zombie
    [HideInInspector]
    public bool zombie;

    private Quaternion newRotate;

    Player_Canvas Canvas;
    public Player_Canvas canvas { get { return Canvas; } }
    Transform lookAtItem;
    GameObject pickUpIcon;
    private bool lookAtTarget;

    void Awake()
    {
        Canvas = transform.GetComponentInChildren<Player_Canvas>();
    }

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
        oldPos = transform.position;
        newPos = transform.position;
        newRotate = transform.rotation;
        //If this local player
        if (Data_MyData.charID == index)
        {
            GameObject icon = null;
#if UNITY_WP_8 || UNITY_WP_8_1 || UNITY_ANDROID || UNITY_IPHONE
        icon = Resources.Load<GameObject>("Weapons/iconHand");
#else
            icon = Resources.Load<GameObject>("Weapons/iconE");
#endif
            pickUpIcon = Instantiate(icon);
            pickUpIcon.SetActive(false);
        }
    }

    public void Move(Vector3 vect, Quaternion rotate)
    {
        newPos = vect;
        newRotate = rotate;
    }

    void OnAnimatorIK()
    {
        if (!zombie)
        {
            if (weapon)
            {
                //Right hand
                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
                anim.SetIKPosition(AvatarIKGoal.RightHand, weapon.Handle.position);
                anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
                anim.SetIKRotation(AvatarIKGoal.RightHand, weapon.Handle.rotation);
                //Left hand
                if (!weapon.Reload)
                {
                    anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
                    anim.SetIKPosition(AvatarIKGoal.LeftHand, weapon.Barrel.position);
                    anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, weapon.Barrel.rotation);
                }
                else
                {
                    anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.2f);
                    anim.SetIKPosition(AvatarIKGoal.LeftHand, weapon.Barrel.position);
                }
            }
        }
    }

    public void Respawn()
    {
        if (!zombie)
        {
            anim.SetLayerWeight(1, 1f);
        }
        anim.SetBool("death", false);
    }

    // Update is called once per frame
    void Update()
    {
        //Sync move and rotate
        transform.position = Vector3.Lerp(transform.position, newPos, moveLerpRate * Time.deltaTime);
        if (Data_MyData.charID != index)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotate, 12f * Time.deltaTime);
        }

        AnimateCharacter();
        LookAtTarget();

        oldPos = transform.position;
    }

    //If look at item
    void LookAtTarget()
    {
        if (lookAtTarget)
        {
            if (!CheckLookAtItem())
            {
                lookAtTarget = false;
                pickUpIcon.SetActive(false);
                Player_PlayerLocate.LookAtObj(null);
            }
        }
    }

    //Check if item in sight
    bool CheckLookAtItem()
    {
        bool look = false;

        if (lookAtItem)
        {
            Vector3 vect = lookAtItem.position - transform.position;
            if (vect.sqrMagnitude < 1.5f)
            {
                float angle = Vector3.Angle(transform.forward, vect);
                if (angle < 20)
                {
                    look = true;
                    vect.Set(lookAtItem.position.x, lookAtItem.position.y + 2.4f, lookAtItem.position.z);
                    pickUpIcon.SetActive(true);
                    pickUpIcon.transform.position = vect;
                    pickUpIcon.transform.LookAt(Player_PlayerLocate.camera.transform);
                    Player_PlayerLocate.LookAtObj(lookAtItem);
                }
            }
        }

        return look;
    }

    //Activate animation based on moving direction
    void AnimateCharacter()
    {
        move = (transform.position - oldPos).sqrMagnitude;
        value = move * magnStr;
        if (value > 0.001f)
        {
            if (!zombie)
            {
                vect = newPos - transform.position;
                angle = Vector3.Angle(vect, transform.forward);

                if (angle < 25)
                {
                    strZ += Time.deltaTime * 3;
                    strX = Mathf.Lerp(strX, 0, Time.deltaTime * 3);
                }
                else if (angle > 155)
                {
                    strZ -= Time.deltaTime * 3;
                    strX = Mathf.Lerp(strX, 0, Time.deltaTime * 3);
                }
                else
                {
                    angle = Vector3.Angle(vect, transform.right);

                    if (angle < 25)
                    {
                        strX += Time.deltaTime * 3;
                        strZ = Mathf.Lerp(strZ, 0, Time.deltaTime * 3);
                    }
                    else if (angle > 155)
                    {
                        strX -= Time.deltaTime * 3;
                        strZ = Mathf.Lerp(strZ, 0, Time.deltaTime * 3);
                    }
                    else
                    {
                        angle = Vector3.Angle(vect, transform.right + transform.forward);

                        if (angle < 25)
                        {
                            strX += Time.deltaTime * 3;
                            strZ += Time.deltaTime * 3;
                        }
                        else if (angle > 155)
                        {
                            strX -= Time.deltaTime * 3;
                            strZ -= Time.deltaTime * 3;
                        }
                        else
                        {
                            angle = Vector3.Angle(vect, -transform.right + transform.forward);

                            if (angle < 25)
                            {
                                strX -= Time.deltaTime * 3;
                                strZ += Time.deltaTime * 3;
                            }
                            else if (angle > 155)
                            {
                                strX += Time.deltaTime * 3;
                                strZ -= Time.deltaTime * 3;
                            }
                            else
                            {
                                strX = Mathf.Lerp(strX, 0, Time.deltaTime * 3);
                                strZ = Mathf.Lerp(strZ, 0, Time.deltaTime * 3);
                            }
                        }
                    }
                }

                if (strZ > 1)
                {
                    strZ = 1;
                }
                else if (strZ < -1)
                {
                    strZ = -1;
                }

                if (strX > 1)
                {
                    strX = 1;
                }
                else if (strX < -1)
                {
                    strX = -1;
                }
                anim.SetFloat("y", strZ);
                anim.SetFloat("x", strX);
            }
            else
            {
                anim.SetBool("run", true);
                vect = newPos - transform.position;
                if (vect != Vector3.zero)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(vect), 200 * Time.fixedDeltaTime);
                }
            }
        }
        else
        {
            if (!zombie)
            {
                strZ = 0f;
                strX = 0f;
                anim.SetFloat("y", strZ);
                anim.SetFloat("x", strX);
            }
            else
            {
                anim.SetBool("run", false);
            }
        }
    }

    //Only for player
    public void Fire(Vector3[] direct, BulletType type)
    {
        if (weapon)
        {
            weapon.Fire(direct, type);
        }
    }

    //Only for zombie
    public void ZombieAttack()
    {
        anim.SetTrigger("attack");
    }

    //Death action
    public void Death()
    {
        anim.SetBool("death", true);
        if (!zombie)
        {
            anim.SetLayerWeight(1, 0f);
        }
        if (Data_MyData.charID == index)
        {
            Player_PlayerLocate.death = true;
        }
    }

    //Reload action
    public void Reload(float time)
    {
        float speed = 2.5f / time;
        anim.SetFloat("speed", speed);
        anim.SetTrigger("reload");
        weapon.StartReload(time);
    }

    public void TakeWeapon(Player_Weapon weap)
    {
        weapon = weap;
    }

    public void DropWeapon()
    {
        if (weapon)
        {
            weapon.Drop();
            weapon = null;
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (Data_MyData.charID == index)
        {
            if (!lookAtTarget)
            {
                if (col.CompareTag("Item"))
                {
                    lookAtItem = col.transform;
                    if (CheckLookAtItem())
                    {
                        lookAtTarget = true;
                    }
                }
            }
        }
    }
}


