using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectorUI : MonoBehaviour
{
    Button StartButton;
    Button BackButton;
    InputField PlayerNameField;
    Dropdown ColorDropdown;
    Dropdown DifficultyDropdown;

    // Start is called before the first frame update
    void Start()
    {
        StartButton = GameObject.Find("StartButton").GetComponent<Button>();
        StartButton.onClick.AddListener(OnStartGame);
        BackButton = GameObject.Find("BackButton").GetComponent<Button>();
        BackButton.onClick.AddListener(OnBackButton);
        PlayerNameField = GameObject.Find("PlayerNameField").GetComponent<InputField>();
        PlayerNameField.onValueChanged.AddListener(delegate{OnPlayerNameChange();});
        ColorDropdown = GameObject.Find("ColorDropdown").GetComponent<Dropdown>();
        ColorDropdown.onValueChanged.AddListener(delegate{OnColorChange();});
        DifficultyDropdown = GameObject.Find("DifficultyDropdown").GetComponent<Dropdown>();
        DifficultyDropdown.onValueChanged.AddListener(delegate{OnDifficultyChange();});
    }


    void OnStartGame()
    {
        GameController.Instance.PlayClickSFX();
        SceneManager.LoadSceneAsync("Tourney");
    }
    void OnBackButton()
    {
        GameController.Instance.PlayClickSFX();
        SceneManager.LoadSceneAsync("MainMenu");
    }

    void OnPlayerNameChange()
    {
        string name = PlayerNameField.text.Trim().ToUpper();
        if(name.Equals("")) {
            GameController.Instance.PlayerName = "WARRIOR";
        }
        else {
            GameController.Instance.PlayerName = name;
        }
    }
    void OnColorChange()
    {
        switch(ColorDropdown.value) {
            case 0:
                GameController.Instance.PlayerColor = new Color32(255, 81, 251, 255);
                break;
            case 1:
                GameController.Instance.PlayerColor = new Color32(192, 57, 43, 255);
                break;
            case 2:
                GameController.Instance.PlayerColor = new Color32(41, 128, 185, 255);
                break;
            case 3:
                GameController.Instance.PlayerColor = new Color32(39, 174, 96, 255);
                break;
            default: 
                GameController.Instance.PlayerColor = new Color32(255, 81, 251, 255);
                break;
        }
    }
    void OnDifficultyChange()
    {
        switch(DifficultyDropdown.value) {
            case 0: 
                GameController.Instance.DifficultyMultiplier = 1f;
                break;
            case 1: 
                GameController.Instance.DifficultyMultiplier = 1.1f;
                break;
            case 2: 
                GameController.Instance.DifficultyMultiplier = 1.2f;
                break;
            default:
                GameController.Instance.DifficultyMultiplier = 1f;
                break;
        }
    }

}
