using UnityEngine;
using UnityEngine.SceneManagement; 

public class StartEndGameScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }
    public void StartGame()
    {
        Debug.Log("Start button clicked!");
        SceneManager.LoadScene("scene");
        //SceneManager.UnloadSceneAsync("MainMenu");
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
