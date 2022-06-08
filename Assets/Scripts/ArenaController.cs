using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class ArenaController : MonoBehaviour
{
    public CameraFollower CameraFollower;
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public GameObject RangedEnemyPrefab;
    public Transform PlayerSpawn;
    public AudioClip CountdownSFX;
    public AudioClip CountdownLastSFX;
    private int round = 1;
    private int enemiesAlive = 0;
    private List<Transform> Spawnpoints;
    private GameObject Player = null;
    private Player cc = null;
    private int FirstRangedEnemyRound = 5;

    // Enemy Stats
    private float EnemiesPerRound = 3f;
    private float EnemyMaxHealth = 50f;
    private float EnemySpeed = 1f;
    private float EnemyDamage = 10f;
    private float EnemyDamageTimeout = 1f;


    // UI Related
    private Text RoundTitleUI;
    private Text CountdownUI;
    private Text HealthUI;
    private Text DamageUI;
    private Text EnemiesLeftUI;
    private RectTransform SpecialBarUI;

    // Start is called before the first frame update
    void Start()
    {
        RoundTitleUI = transform.Find("UI/RoundTitle").GetComponent<Text>();
        CountdownUI = transform.Find("UI/Countdown").GetComponent<Text>();
        HealthUI = transform.Find("UI/Health").GetComponent<Text>();
        DamageUI = transform.Find("UI/Damage").GetComponent<Text>();
        EnemiesLeftUI = transform.Find("UI/EnemiesLeft").GetComponent<Text>();
        SpecialBarUI = transform.Find("UI/SpecialBarWrapper/SpecialBarFull/SpecialBar").GetComponent<RectTransform>();
        SpecialBarUI.GetComponent<Image>().color = GameController.Instance.PlayerColor;
        transform.Find("UI/SpecialBarWrapper/SpecialBarFull").GetComponent<Image>().color = GameController.Instance.PlayerColor;
        transform.Find("UI/SpecialBarWrapper/SpecialArrow").GetComponent<Image>().color = GameController.Instance.PlayerColor;
        transform.Find("UI/SpecialBarWrapper/SpecialArrow").GetComponent<Image>().color = GameController.Instance.PlayerColor;
        GetSpawnpoints();
        SpawnPlayer();
        Invoke(nameof(CountdownForNewRound), 1f);

        if(GameController.Instance.DifficultyMultiplier == 1f) {
            FirstRangedEnemyRound = 5;
        }
        else if(GameController.Instance.DifficultyMultiplier == 1.1f) {
            FirstRangedEnemyRound = 3;
        }
        else if(GameController.Instance.DifficultyMultiplier == 1.2f) {
            FirstRangedEnemyRound = 2;
        }
    }

    void SpawnPlayer()
    {
        DespawnPlayer();
        Player = Instantiate(PlayerPrefab, PlayerSpawn.position, Quaternion.identity);
        CameraFollower.target = Player.transform;
        CameraFollower.UpdatePosition();
        cc = Player.GetComponent<Player>();
        PauseMenuUI.Player = cc;
        cc.ArenaController = this;
        Player.layer = PlayerSpawn.gameObject.layer;
        Player.GetComponent<SpriteRenderer>().sortingLayerName = LayerMask.LayerToName(PlayerSpawn.gameObject.layer);
        UpdatePlayerHealthUI();
        UpdatePlayerDamageUI();
        UpdatePlayerSpecialUI(1f);
    }
    void DespawnPlayer()
    {
        if(Player != null) {
            CameraFollower.target = null;
            Destroy(Player); 
        }
    }

    void CountdownForNewRound()
    {
        RoundTitleUI.text = "ROUND " + round.ToString();
        StartCoroutine(Countdown(3));
        Invoke(nameof(OnNewRound), 3f);
    }

    IEnumerator Countdown(int countdown)
    {
        while(countdown > 0) {
            CountdownUI.text = countdown.ToString();
            AudioSource.PlayClipAtPoint(CountdownSFX, Player.transform.position, .5f);
            yield return new WaitForSeconds(1f);
            countdown -= 1;
        }
        AudioSource.PlayClipAtPoint(CountdownLastSFX, Player.transform.position, .5f);
        CountdownUI.text = "";
    }

    void OnNewRound()
    {
        RoundTitleUI.text = "";
        enemiesAlive = (int)EnemiesPerRound;
        int sp = UnityEngine.Random.Range(0, Spawnpoints.Count);
        int numRanged = round < FirstRangedEnemyRound ? 0 : (int)Mathf.Round(enemiesAlive * 0.2f);
        for(int i = 0; i < enemiesAlive-numRanged; ++i) {
            SpawnEnemy(EnemyPrefab, Spawnpoints[(sp++) % Spawnpoints.Count]);
        }
        for(int i = 0; i < numRanged; ++i) {
            SpawnEnemy(RangedEnemyPrefab, Spawnpoints[(sp++) % Spawnpoints.Count]);
        }
        EnemiesLeftUI.text = enemiesAlive.ToString();

        // Make enemies harder for next round
        if(round <= 3) {
            EnemiesPerRound *= 1.5f * GameController.Instance.DifficultyMultiplier;
            EnemyMaxHealth *= 1.2f * GameController.Instance.DifficultyMultiplier;
        }
        else {
            ++EnemiesPerRound;
            EnemyMaxHealth *= 1.05f * GameController.Instance.DifficultyMultiplier;
            EnemyDamage *= 1.05f * GameController.Instance.DifficultyMultiplier;
        }
        EnemyDamageTimeout = Mathf.Clamp(EnemyDamageTimeout * .95f, .5f, 1f);
        EnemySpeed = Mathf.Clamp(EnemySpeed * 1.01f * GameController.Instance.DifficultyMultiplier, 1f, 2f);
        round += 1;
    }

    void SpawnEnemy(GameObject prefab, Transform spawnpoint)
    {
        GameObject enemy = Instantiate(prefab, spawnpoint.position, Quaternion.identity);
        EnemyController ec = enemy.GetComponent<EnemyController>();
        ec.Target = Player.transform;
        ec.ArenaController = this;
        ec.SetStats(EnemyMaxHealth, EnemySpeed, EnemyDamage, EnemyDamageTimeout);
        enemy.gameObject.layer = spawnpoint.gameObject.layer;
        enemy.GetComponent<SpriteRenderer>().sortingLayerName = LayerMask.LayerToName(spawnpoint.gameObject.layer);
    }

    public void OnEnemyDeath()
    {
        if(--enemiesAlive <= 0) {
            PauseMenuUI.OpenMidRoundMenu();
            Invoke(nameof(CountdownForNewRound), .5f);
        }
        EnemiesLeftUI.text = enemiesAlive.ToString();
    }

    void GetSpawnpoints()
    {
        Spawnpoints = new List<Transform>();
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach(Transform child in children) {
            if(child.gameObject.tag == "Spawnpoint") {
                Spawnpoints.Add(child);
            }
        }
    }

    public void UpdatePlayerHealthUI()
    {
        HealthUI.text = string.Format("{0:N0}", cc.Health < 0f ? 0f : cc.Health);
    }
    public void UpdatePlayerDamageUI()
    {
        DamageUI.text = string.Format("{0:N0}", cc.Damage);
    }
    public void UpdatePlayerSpecialUI(float percent)
    {
        SpecialBarUI.sizeDelta = new Vector2(400f * percent, 0f);
    }

    public void GameOver()
    {
        SceneManager.LoadSceneAsync("GameOver");
    }
}