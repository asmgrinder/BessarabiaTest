using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageController : MonoBehaviour
{
    public float TextDelay = 3;
    public TMP_Text text;
    public bool IsActive => gameObject.activeSelf;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show(string Text)
    {
        gameObject.SetActive(true);
        StartCoroutine(showAndWait(Text));
    }

    IEnumerator showAndWait(string Text)
    {
        text.text = Text;
        yield return new WaitForSeconds(TextDelay);
        gameObject.SetActive(false);
    }
}
