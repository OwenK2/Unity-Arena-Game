using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    private Button StartButton;
    private Button HowToPlayButton;
    private Button QuitButton;
    private Button CreditsButton;


    // Start is called before the first frame update
    void Start()
    {
        StartButton = GameObject.Find("StartButton").GetComponent<Button>();
        StartButton.onClick.AddListener(OnStartGame);
        HowToPlayButton = GameObject.Find("HowToPlayButton").GetComponent<Button>();
        HowToPlayButton.onClick.AddListener(OnHowToPlay);
        CreditsButton = GameObject.Find("CreditsButton").GetComponent<Button>();
        CreditsButton.onClick.AddListener(OnCredits);
        QuitButton = GameObject.Find("QuitButton").GetComponent<Button>();
        QuitButton.onClick.AddListener(OnQuitGame);
    }

    void OnStartGame()
    {
        GameController.Instance.PlayClickSFX();
        SceneManager.LoadSceneAsync("CharacterSelector");
    }
    void OnHowToPlay()
    {
        GameController.Instance.PlayClickSFX();
        SceneManager.LoadSceneAsync("HowToPlay");
    }
    void OnCredits()
    {
        GameController.Instance.PlayClickSFX();
        SceneManager.LoadSceneAsync("Credits");
    }
    void OnQuitGame()
    {
        #if UNITY_STANDALONE
            Application.Quit();
        #endif
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

}
