using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private void OnEnable()
    {
        Time.timeScale = 0.0f;
    }

    private void OnDisable()
    {
        Time.timeScale = 1.0f;
    }

    public void RestartScene()
    {
        gameObject.SetActive(false);

        Scene scene = SceneManager.GetSceneAt(1);

        GameManager.Instance.LoadLevel((SceneIndexes)scene.buildIndex, (SceneIndexes)scene.buildIndex);
    }

    public void LoadMainMenu()
    {
        gameObject.SetActive(false);

        Scene scene = SceneManager.GetSceneAt(1);

        GameManager.Instance.LoadLevel((SceneIndexes)scene.buildIndex, SceneIndexes.MENU);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
