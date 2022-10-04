using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneIndexes
{
    MANAGER = 0,
    MENU = 1,
    LEVEL_DEMO = 2,
    LEVEL_COVER = 3,
    LEVEL_SCRIPTEDMOVEMENT = 4
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    [SerializeField]
    GameObject m_LoadingScreen;
    [SerializeField]
    Slider m_ProgressBar;
    [SerializeField]
    GameObject m_PauseMenu;

    [SerializeField]
    float m_ExtraTime = 2.0f;

    private List<AsyncOperation> m_ScenesLoading = new List<AsyncOperation>();
    private float m_TotalSceneProgress;
    private float m_Timer;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("POI Manager destroyed.");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        m_LoadingScreen.SetActive(false);

        SceneManager.LoadSceneAsync((int)SceneIndexes.MENU, LoadSceneMode.Additive);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(m_PauseMenu.activeSelf)
            {
                m_PauseMenu.SetActive(false);
            }
            else
            {
                m_PauseMenu.SetActive(true);
            }
        }
    }

    public void LoadLevel(SceneIndexes _unloadScene, SceneIndexes _loadScene)
    {
        m_LoadingScreen.gameObject.SetActive(true);

        m_ScenesLoading.Add(SceneManager.UnloadSceneAsync((int)_unloadScene));
        m_ScenesLoading.Add(SceneManager.LoadSceneAsync((int)_loadScene, LoadSceneMode.Additive));

        StartCoroutine(GetSceneLoadProgress());
        StartCoroutine(GetTotalProgress());
    }

    public IEnumerator GetSceneLoadProgress()
    {
        for(int i = 0; i < m_ScenesLoading.Count; i++)
        {
            while (!m_ScenesLoading[i].isDone)
            {
                m_TotalSceneProgress = 0;

                foreach(AsyncOperation operation in m_ScenesLoading)
                {
                    m_TotalSceneProgress += operation.progress;
                }

                m_TotalSceneProgress = (m_TotalSceneProgress / m_ScenesLoading.Count);

                //m_ProgressBar.value = m_TotalSceneProgress;

                yield return null;
            }
        }
    }

    public IEnumerator GetTotalProgress()
    {
        float totalProgress = 0.0f;
        m_Timer = 0.0f;

        while(m_Timer <= m_ExtraTime)
        {
            if(!AllScenesLoaded())
            {
                totalProgress = m_TotalSceneProgress / 2.0f;
            }
            else
            {
                totalProgress = (m_TotalSceneProgress + (m_Timer / m_ExtraTime)) / 2.0f;
                m_Timer += Time.deltaTime;
            }

            m_ProgressBar.value = totalProgress;

            yield return null;
        }

        m_LoadingScreen.SetActive(false);
    }

    private bool AllScenesLoaded()
    {
        for (int i = 0; i < m_ScenesLoading.Count; i++)
        {
            if (!m_ScenesLoading[i].isDone)
                return false;
        }

        return true;
    }
}
