using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    private Button MainMenuButton;
    private Button RetryButton;

    // Start is called before the first frame update
    void Start()
    {
        RetryButton = GameObject.Find("RetryButton").GetComponent<Button>();
        RetryButton.onClick.AddListener(OnRetryClick);
        MainMenuButton = GameObject.Find("MainMenuButton").GetComponent<Button>();
        MainMenuButton.onClick.AddListener(OnMainMenuClick);
    }

    void OnRetryClick()
    {
        GameController.Instance.PlayClickSFX();
        SceneManager.LoadSceneAsync("Tourney");
    }
    void OnMainMenuClick()
    {
        GameController.Instance.PlayClickSFX();
        SceneManager.LoadSceneAsync("MainMenu");
    }

}
