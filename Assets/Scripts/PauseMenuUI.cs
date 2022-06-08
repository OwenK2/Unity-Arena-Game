using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    public static bool IsPaused = false;
    public static bool InMidRoundMenu = false;
    static private CanvasGroup PauseMenu;
    static private CanvasGroup MidRoundMenu;
    static private Button ResumeButton;
    static private Button ExitButton;

    static public Player Player;
    static private Text PlayerNameText;
    static private Button MaxHealthUpgradeButton;
    static private Button DamageUpgradeButton;
    static private Button SpeedUpgradeButton;

    void Start()
    {
        PauseMenu = transform.Find("PauseMenu").GetComponent<CanvasGroup>();
        MidRoundMenu = transform.Find("MidRoundMenu").GetComponent<CanvasGroup>();


        ResumeButton = PauseMenu.transform.Find("ResumeButton").GetComponent<Button>();
        ResumeButton.onClick.AddListener(OnResumeClick);
        ExitButton = PauseMenu.transform.Find("ExitButton").GetComponent<Button>();
        ExitButton.onClick.AddListener(OnExitClick);

        PlayerNameText = MidRoundMenu.transform.Find("PlayerName").GetComponent<Text>();
        PlayerNameText.text = "Congratulations " + GameController.Instance.PlayerName.ToUpper().Trim() + "!";
        MaxHealthUpgradeButton = MidRoundMenu.transform.Find("MaxHealthUpgradeButton").GetComponent<Button>();
        MaxHealthUpgradeButton.onClick.AddListener(OnHealthUpgradeClick);
        DamageUpgradeButton = MidRoundMenu.transform.Find("DamageUpgradeButton").GetComponent<Button>();
        DamageUpgradeButton.onClick.AddListener(OnDamageUpgradeClick);
        SpeedUpgradeButton = MidRoundMenu.transform.Find("SpeedUpgradeButton").GetComponent<Button>();
        SpeedUpgradeButton.onClick.AddListener(OnSpeedUpgradeClick);

        ResumeGame();
    }

    void Update()
    {
        if(InMidRoundMenu) {return;}
        if(Input.GetButtonDown("Cancel")) {
            if(!IsPaused) {
                PauseGame();
                ShowMenu(PauseMenu);
            }
            else {
                HideMenu(PauseMenu);
                ResumeGame();
            }
        }
    }

    public static void OpenMidRoundMenu()
    {
        InMidRoundMenu = true;
        ShowMenu(MidRoundMenu);
        PauseGame();
    }
    public static void CloseMidRoundMenu()
    {
        InMidRoundMenu = false;
        HideMenu(MidRoundMenu);
        ResumeGame();
    }

    public static void PauseGame()
    {
        Time.timeScale = 0;
        IsPaused = true;
    }
    public static void ResumeGame()
    {
        Time.timeScale = 1;
        IsPaused = false;
    }

    public static void ShowMenu(CanvasGroup menu)
    {
        menu.alpha = 1;
        menu.interactable = true;
        menu.blocksRaycasts = true;
    }
    public static void HideMenu(CanvasGroup menu) 
    {
        menu.alpha = 0;
        menu.interactable = false;
        menu.blocksRaycasts = false;
    }

    static void OnResumeClick()
    {
        GameController.Instance.PlayClickSFX();
        HideMenu(PauseMenu);
        ResumeGame();
    }
    static void OnExitClick()
    {
        ResumeGame();
        GameController.Instance.PlayClickSFX();
        SceneManager.LoadSceneAsync("MainMenu");
    }

    static void OnHealthUpgradeClick()
    {
        GameController.Instance.PlayClickSFX();
        Player.UpgradeHealth();
        CloseMidRoundMenu();
    }
    static void OnDamageUpgradeClick()
    {
        GameController.Instance.PlayClickSFX();
        Player.UpgradeDamage();
        CloseMidRoundMenu();
    }
    static void OnSpeedUpgradeClick()
    {
        GameController.Instance.PlayClickSFX();
        Player.UpgradeSpeed();
        CloseMidRoundMenu();
    }
}
