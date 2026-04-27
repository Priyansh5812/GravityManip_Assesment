using System.Collections;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StatsManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TimerText;
    [SerializeField] TextMeshProUGUI RemainingText;
    [SerializeField] TextMeshProUGUI ResultText;
    [SerializeField] Button btn_restart, btn_quit;
    [SerializeField] CanvasGroup cgMain;
    [SerializeField] int totalMins = 1;
    [SerializeField] int totalSecs = 30;

    [SerializeField] CollectibleIdentifier[] collectibles;

    float totalTimeRemaining = 0; // in secs;
    int CubesRemaining = 0;

    void OnEnable()
    {
        EventManager.OnGameStarted.AddListener(ResetStats);
        EventManager.OnGameEnded.AddListener(OnGameEnded);
        EventManager.OnCubeCollected.AddListener(UpdateRemainingCollectibles);

        btn_restart.onClick.AddListener(OnRestart);
        btn_quit.onClick.AddListener(OnQuit);
    }

    void Start()
    {   
        StartCoroutine(InitiateClock());
    }

    IEnumerator InitiateClock()
    {
        EventManager.OnGameStarted?.Invoke();

        while (totalTimeRemaining > 0 && CubesRemaining > 0)
        { 
            totalTimeRemaining -= Time.deltaTime;
            UpdateTimerView();
            yield return null;
        }

        EventManager.OnGameEnded?.Invoke();
    }

    void UpdateTimerView()
    {
        TimerText?.SetText("Timer: " + totalTimeRemaining.ToString("N0"));
    }

    void UpdateRemainingCollectiblesView()
    {   
        RemainingText?.SetText($"Remaining : {CubesRemaining}/{collectibles.Length}");
    }

    void UpdateRemainingCollectibles()
    {
        CubesRemaining--;
        UpdateRemainingCollectiblesView();
    }

    void UpdateResult()
    {
        ResultText?.SetText($"Cubes Collected : {collectibles.Length - CubesRemaining}");
    }

    void ResetStats()
    {   
        totalTimeRemaining = totalMins * 60 + totalSecs;
        CubesRemaining = collectibles.Length;
        UpdateTimerView();
        UpdateRemainingCollectiblesView();
        cgMain.alpha = 0.0f;
        cgMain.interactable = cgMain.blocksRaycasts = false;
    }

    void OnGameEnded()
    {
        cgMain.alpha = 1.0f;
        cgMain.interactable = cgMain.blocksRaycasts = true;
        UpdateResult();
    }

    void OnRestart()
    {
        SceneManager.LoadScene(0);
    }

    void OnQuit()
    {
#if !UNITY_EDITOR
    Application.Quit();
#else
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    private void OnDisable()
    {
        EventManager.OnGameStarted.RemoveListener(ResetStats);
        EventManager.OnGameEnded.RemoveListener(OnGameEnded);
        EventManager.OnCubeCollected.RemoveListener(UpdateRemainingCollectibles);


        btn_restart.onClick.RemoveListener(OnRestart);
        btn_quit.onClick.RemoveListener(OnQuit);
    }



}
