using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PausedController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (null != GameMngr.Instance
            && null != MainMenuController.Instance)
        {
            transform.localScale = !MainMenuController.Instance.IsActive && GameMngr.Instance.IsPaused
                                    ? Vector3.one
                                    : Vector3.zero;
        }
    }
}
