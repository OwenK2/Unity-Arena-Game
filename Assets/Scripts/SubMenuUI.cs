using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SubMenuUI : MonoBehaviour
{
    private Button BackButton;
    // Start is called before the first frame update
    void Start()
    {
        BackButton = GameObject.Find("BackButton").GetComponent<Button>();
        BackButton.onClick.AddListener(OnBackButton);
    }

    void OnBackButton()
    {
        GameController.Instance.PlayClickSFX();
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
