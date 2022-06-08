using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using System;

public class GameController : MonoBehaviour
{
    private static GameController instance;
    public static GameController Instance { get {
        if(instance == null) {
            instance = FindObjectOfType<GameController>();
            if(instance == null) {
                GameObject obj = new GameObject();
                obj.name = "GameController";
                instance = obj.AddComponent<GameController>();
                DontDestroyOnLoad(obj);
            }
        }
        return instance;
    }}

    public float MenuMusicVolume = .3f;
    public AudioClip MenuMusic;
    public float GameMusicVolume = .1f;
    public AudioClip GameMusic;
    public float ClickSFXVolume = .1f;
    public AudioClip ClickSFX;

    public string PlayerName = "WARRIOR";
    public Color PlayerColor = new Color32(255, 81, 251, 255);
    public float DifficultyMultiplier = 1f;

    private AudioSource source;
    private bool wasInGame = false;

    public void Awake() 
    {
        if(instance != null && instance != this) {
            Destroy(this);
        }
        else {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void Start() {
        source = GetComponent<AudioSource>();
        if(source == null) {source = gameObject.AddComponent<AudioSource>();}

        source.loop = true;
        if(SceneManager.GetActiveScene().Equals("Tourney")) {
            instance.PlayInGameMusic();
        }
        else {instance.PlayMenuMusic();}
        SceneManager.sceneLoaded += instance.OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(wasInGame && !scene.name.Equals("Tourney")) {
            wasInGame = false;
            StartCoroutine(FadeMusicTo(0f, .5f, delegate{PlayMenuMusic();}));
        }
        else if(scene.name.Equals("Tourney")) {
            wasInGame = true;
            StartCoroutine(FadeMusicTo(0f, .5f, delegate{PlayInGameMusic();}));
        }
    }

    public void PlayInGameMusic()
    {
        source.volume = 0f;
        source.clip = GameMusic;
        source.Play();
        StartCoroutine(FadeMusicTo(GameMusicVolume, 1f));
    }
    public void PlayMenuMusic()
    {
        source.volume = 0f;
        source.clip = MenuMusic;
        source.Play();
        StartCoroutine(FadeMusicTo(MenuMusicVolume, 1f));
    }

    public void PlayClickSFX()
    {
        source.PlayOneShot(ClickSFX, ClickSFXVolume);
    }

    public IEnumerator FadeMusicTo(float volume, float FadeTime, Action callback = null) 
    {
        float startVolume = source.volume;
        bool fadeOut = startVolume > volume;
        while((fadeOut && source.volume > volume) || (!fadeOut && source.volume < volume)) {
            source.volume += (volume - startVolume) * (Time.deltaTime / FadeTime);
            yield return null;
        }
        if(callback != null) {
            callback();
        }
    }

}