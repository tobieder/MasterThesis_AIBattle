using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public void LoadLevelDemo()
    {
        GameManager.Instance.LoadLevel(SceneIndexes.MENU, SceneIndexes.LEVEL_DEMO);
    }

    public void LoadLevelCover()
    {
        GameManager.Instance.LoadLevel(SceneIndexes.MENU, SceneIndexes.LEVEL_COVER);
    }

    public void LoadLevelScriptedMovement()
    {
        GameManager.Instance.LoadLevel(SceneIndexes.MENU, SceneIndexes.LEVEL_SCRIPTEDMOVEMENT);
    }
}
