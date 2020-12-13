using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public TextMeshProUGUI title;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
    }

    public void TitleFadeout(float duration)
    {
        StartCoroutine(UITextFadeout(duration, title, 2));
    }



    public static IEnumerator UITextFadeout(float duration, TextMeshProUGUI text, float smoothing)
    {
        float timer = 0;
        Color startingColor = text.color;

        while (timer < duration)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b,
                Mathf.Lerp(startingColor.a, 0, Mathf.Pow(timer / duration, smoothing)));
            //Color.Lerp(startingColor, Color.clear, timer / duration);

            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        text.color = Color.clear;

        yield return null;
    }

}
