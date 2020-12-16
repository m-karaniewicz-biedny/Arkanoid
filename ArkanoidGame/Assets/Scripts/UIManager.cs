using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public TextMeshProUGUI title;
    public GameObject pauseMenu;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);

        pauseMenu.SetActive(false);
    }

    public void SetGamePause(bool pause)
    {
        pauseMenu.SetActive(pause);

        if (pause)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            Time.timeScale = 1;
        }

    }

    public void TitleFadeout(float duration)
    {
        StartCoroutine(UITextFadeout(duration, title, 2));
    }

    public void ExitGame()
    {
        StartCoroutine(ExitSequence());
    }

    private IEnumerator ExitSequence()
    {


#if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
#elif (UNITY_STANDALONE)
        Application.Quit();
#elif (UNITY_WEBGL)
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
#endif
        yield return null;
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
