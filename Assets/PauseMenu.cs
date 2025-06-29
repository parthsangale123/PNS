using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject g1;
    public GameObject g2;
    bool paused=false;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(paused){
                Resume();
            }
            else{
                Pause();
            }
        }
    }
    public void Pause(){
        g2.SetActive(true);
        g1.SetActive(false);
        Time.timeScale=0f;
    }
    public void Resume(){
        g2.SetActive(false);
        g1.SetActive(true);
        Time.timeScale=1f;
    }
    public void Restart(){
        Time.timeScale=1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }
    public void QUIT(){
        Time.timeScale=1f;
        SceneManager.LoadScene("MainMenu");
    }
}
