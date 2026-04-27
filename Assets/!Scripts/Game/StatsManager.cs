using System.Collections;
using TMPro;
using UnityEngine;
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
        EventManager.OnCubeCollected.AddListener(UpdateRemainingCollectibles);
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

    void ResetStats()
    {
        totalTimeRemaining = totalMins * 60 + totalSecs;
        CubesRemaining = collectibles.Length;
        UpdateTimerView();
        UpdateRemainingCollectiblesView();
    }

    private void OnDisable()
    {
        EventManager.OnGameStarted.RemoveListener(ResetStats);
        EventManager.OnCubeCollected.RemoveListener(UpdateRemainingCollectibles);
    }



}
