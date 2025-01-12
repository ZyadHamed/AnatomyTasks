using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TimerScript : MonoBehaviour
{
    public float TimeLeft = 300;
    public bool TimerOn = false;
    public Image CanvasImage;
    public Text gameOverText;
    public Button gameoverButton;

    public Text TimerTxt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TimerOn = true;
    }

    void Update()
    {
        if (TimerOn)
        {
            if (TimeLeft > 0)
            {
                TimeLeft -= Time.deltaTime;
                updateTimer(TimeLeft);
            }
            else
            {
                Debug.Log("Time is UP!");
                TimeLeft = 0;
                TimerOn = false;
                CanvasImage.gameObject.SetActive(true);
                gameoverButton.gameObject.SetActive(true);
                gameOverText.gameObject.SetActive(true);

}
        }
    }

    void updateTimer(float currentTime)
    {
        currentTime += 1;

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        TimerTxt.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        SceneManager.UnloadSceneAsync("scene");
    }

}

