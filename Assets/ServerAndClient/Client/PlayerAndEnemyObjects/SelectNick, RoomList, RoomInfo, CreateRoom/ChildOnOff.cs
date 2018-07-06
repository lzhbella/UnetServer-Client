using UnityEngine;

public class ChildOnOff : MonoBehaviour
{

    [SerializeField]
    GameObject[] nextWindow;
    [SerializeField]
    GameObject[] lastWindow;
    Animator[] animNext;
    Animator[] animLast;

    // Use this for initialization
    void Start()
    {
        if (nextWindow.Length > 0)
        {
            animNext = new Animator[nextWindow.Length];
            for(int g = 0; g < nextWindow.Length; g++)
            {
                if (nextWindow[g])
                {
                    animNext[g] = nextWindow[g].GetComponent<Animator>();
                }
            }
        }
        else
        {
            Debug.Log(string.Format("Next window is NULL on GameObject '{0}'!", gameObject.name));
        }
        if (lastWindow.Length > 0)
        {
            animLast = new Animator[lastWindow.Length];
            for (int g = 0; g < lastWindow.Length; g++)
            {
                if (lastWindow[g])
                {
                    animLast[g] = lastWindow[g].GetComponent<Animator>();
                }
            }
        }
        else
        {
            Debug.Log(string.Format("Last window is NULL on GameObject '{0}'!", gameObject.name));
        }
    }

    public void NextWindowOn(int winId)
    {
        if (animNext.Length > winId && winId >= 0)
        {
            if (animNext[winId])
            {
                animNext[winId].SetTrigger("On");
            }
        }
    }

    public void LastWindowOn(int winId)
    {
        if (animLast.Length > winId && winId >= 0)
        {
            if (animLast[winId])
            {
                animLast[winId].SetTrigger("On");
            }
        }
    }
}
