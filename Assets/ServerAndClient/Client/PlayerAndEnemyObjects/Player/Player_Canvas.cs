using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player_Canvas : MonoBehaviour
{
    [SerializeField]
    private Text Nick;
    [SerializeField]
    private Text Title;
    [SerializeField]
    private Image RangImg;

    Transform cam;
    Vector3 vect;

    // Use this for initialization
    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    // Update is called once per frame
    void Update()
    {
        vect = cam.position - transform.position;
        //Nick bar look at camera
        transform.rotation = Quaternion.LookRotation(-vect.normalized, Vector3.up);
    }

    public void SetNick(string nick)
    {
        Nick.text = nick;
    }

    public void SetTitle(string title)
    {
        Title.text = title;
    }

    public void SetRang(int rang)
    {
        Sprite img = null;

        if (rang > 0 && rang < 12)
        {
            img = Resources.Load<Sprite>("Rangs/" + rang.ToString());
        }

        if (img)
        {
            if (!RangImg.gameObject.activeInHierarchy)
            {
                RangImg.gameObject.SetActive(true);
            }
            RangImg.sprite = img;
        }
        else
        {
            RangImg.gameObject.SetActive(false);
        }
    }
}