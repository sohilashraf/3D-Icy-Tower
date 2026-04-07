using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Restart")]
    [SerializeField] private float fallYLevel = -10f;
    [SerializeField] private float countdownStep = 1f;

    [Header("UI")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TMP_Text countdownText;

    private bool isGameOver;

    public bool IsGameOver => isGameOver;
    public float FallYLevel => fallYLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (countdownPanel != null)
            countdownPanel.SetActive(false);
    }

    public void TriggerGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameOver();

        StartCoroutine(GameOverRoutine());

    }

    private IEnumerator GameOverRoutine()
    {
        if (countdownPanel != null)
            countdownPanel.SetActive(true);

        Time.timeScale = 0f;

        yield return ShowCountdownNumber("3");
        yield return ShowCountdownNumber("2");
        yield return ShowCountdownNumber("1");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator ShowCountdownNumber(string number)
    {
        if (countdownText != null)
            countdownText.text = number;

        yield return new WaitForSecondsRealtime(countdownStep);
    }
}