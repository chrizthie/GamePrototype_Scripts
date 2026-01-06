using UnityEngine;

public abstract class PauseMenu : MonoBehaviour
{
    protected virtual void Awake()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnOpen()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnClose()
    {
        gameObject.SetActive(false);
    }
}