using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;

    [Header("Game Over")]
    public TMPro.TextMeshProUGUI gameResultText;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // 게임 시작하면, 여기부터 실행.. 아마도...
    public void StartGame()

    {
        Time.timeScale = 1f;
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (GameManager.Instance) GameManager.Instance.enabled = true;

        Debug.Log("Game Started");
    }

    public void TogglePause()
    {
        bool isPaused = Time.timeScale == 0f;
        Time.timeScale = isPaused ? 1f : 0f;
        if (pauseMenuPanel) pauseMenuPanel.SetActive(!isPaused);
    }

    public void ShowGameOver(bool playerWon)
    {
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (gameResultText) gameResultText.text = playerWon ? "YOU WIN!" : "GAME OVER";
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}