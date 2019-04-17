using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
  #region Singleton

  private static GameManager _instance;
  public static GameManager Instance {
    get {
      return _instance;
    }
    private set {
      _instance = value;
    }
  }

  #endregion

  public bool GameOver {
    get;
    set;
  }

  public bool Paused {
    get;
    set;
  }

  public System.Action<int> onPlayerDeath;
  public System.Action<int> onPlayerRoundLose;
  public System.Action onRoundRestart;
  public System.Action<int> onGameOver;
  public System.Action<bool> onPause;

  void Awake () {
    if (Instance != null) {
      Destroy (gameObject);
    } else {
      Instance = this;
    }
    DontDestroyOnLoad (gameObject);
    onPause += Pause;
  }

  void Start () {
    Instance.GameOver = false;
    onPause (false);

    SceneManager.sceneLoaded += (e, ex) => { };
  }

  public void LoadScene (string sceneName) {
    SceneManager.LoadScene (sceneName);
  }

  public void Pause (bool b) {
    Paused = b;
    print (Paused ? "Paused" : "Unpaused");
  }

}