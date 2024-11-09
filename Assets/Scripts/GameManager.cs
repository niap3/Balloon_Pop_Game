using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Stats")]
    public int score = 0;
    public int lives = 9;

    [Header("Stats Display Texts")]
    public Text scoreText;
    public Text livesText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        UpdateScoreText();
        UpdateLivesText();
    }

    public void StartGame()
    {
        score = 0;
        lives = 9;

        UpdateScoreText();
        UpdateLivesText();

        BalloonManager.Instance.StartSpawning();

        Time.timeScale = 1;
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    public void AddLife(int amount)
    {
        lives += amount;
        UpdateLivesText();
    }

    public void ReduceLives(int amount)
    {
        lives -= amount;
        UpdateLivesText();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    private void UpdateLivesText()
    {
        if (livesText != null)
        {
            livesText.text = lives.ToString();
        }
    }

    public void GameOver()
    {
        UIManager.Instance.pauseButton.gameObject.SetActive(false);

        BalloonManager.Instance.StopSpawning();

        UIManager.Instance.OnGameOver();
    }
}
