using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
  #region Singleton

  private static AudioManager _instance;
  public static AudioManager Instance
  {
    get
    {
      return _instance;
    }
    private set
    {
      _instance = value;
    }
  }

  AudioSource[] sources;
  public int activeSourceIndex = 0;

  #endregion

  bool ongoing = false;
  public List<SoundGroup> groups = new List<SoundGroup>();

  public void PlayOnSource(string groupName, string soundName, float volume = 0.5f, bool loop = false, float transitionDuration = 0, Action onFinished = null, float startAfter = 0)
  {
  start:
    if (!ongoing)
    {
      ongoing = true;
      sources[activeSourceIndex].volume = volume;
      for (int i = 0; i < groups.Count; i++)
      {
        if (groups[i].name == groupName)
        {
          for (int j = 0; j < groups[i].sounds.Count; j++)
          {
            if (groups[i][j].name == soundName)
            {
              StartCoroutine(PlayBetweenSourcesFading(groups[i][j].clip, volume, loop, transitionDuration, onFinished, startAfter));
              return;
            }
          }
        }
      }
    }
    else
    {
      StopAllCoroutines();
      ongoing = false;
      goto start;
    }
  }

  public void PlayOnSource(AudioClip clip, float volume = 0.5f, bool loop = false, float transitionDuration = 0, Action onFinished = null, float startAfter = 0)
  {
    StartCoroutine(PlayBetweenSourcesFading(clip, volume, loop, transitionDuration, onFinished, startAfter));
  }


  public void PlayOneShot(string groupName, string soundName, float volume = 0.5f, Vector3 pos = new Vector3())
  {

    for (int i = 0; i < groups.Count; i++)
    {
      if (groups[i].name == groupName)
      {
        for (int j = 0; j < groups[i].sounds.Count; j++)
        {
          if (groups[i][j].name == soundName)
          {
            AudioSource.PlayClipAtPoint(groups[i][j].clip, pos, volume);
            return;
          }
        }
      }
    }
  }

  IEnumerator PlayBetweenSourcesFading(AudioClip clip, float volume, bool loop, float duration, Action onFinished, float startAfter)
  {
    yield return new WaitForSeconds(startAfter);
    int nextActiveSource = (activeSourceIndex + 1) % sources.Length;
    sources[nextActiveSource].volume = 0;
    sources[nextActiveSource].loop = loop;
    sources[activeSourceIndex].volume = volume;
    float speed = 1f * volume / duration;
    sources[nextActiveSource].clip = clip;
    sources[nextActiveSource].Play();
    while (sources[nextActiveSource].volume < volume)
    {
      sources[activeSourceIndex].volume -= speed * Time.deltaTime;
      sources[nextActiveSource].volume += speed * Time.deltaTime;
      yield return null;
    }
    sources[activeSourceIndex].Stop();
    activeSourceIndex = nextActiveSource;
    ongoing = false;
    if (onFinished != null)
      onFinished();
  }

  public void PlaySequence(string groupName, string[] sounds, System.Action onFinished = null)
  {
    StartCoroutine(PlayBySequence(groupName, sounds, onFinished));
  }


  IEnumerator PlayBySequence(string groupName, string[] sounds, System.Action onFinished)
  {
    Queue<AudioClip> clips = new Queue<AudioClip>();
    int currentClipIdx = 0;
    for (int i = 0; i < groups.Count; i++)
    {
      if (groups[i].name == groupName)
      {
        for (int j = 0; j < groups[i].sounds.Count; j++)
        {
          if (groups[i][j].name == sounds[currentClipIdx])
            clips.Enqueue(groups[i][j].clip);
          currentClipIdx++;
        }
        break;
      }
    }
    int _i = 0;
    while (clips.Count != 0)
    {
      AudioClip clipToPlay = clips.Dequeue();
      Instance.PlayOnSource(clipToPlay, 0.25f, false, 5f);
      yield return new WaitForSeconds(clipToPlay.length - groups.Find(e => e.name == groupName).sounds.Find(s => s.name == sounds[_i]).clipEndDelay);
      _i++;
    }
    yield return null;
    if (onFinished != null)
      onFinished();
  }


  void Awake()
  {
    if (Instance != null)
    {
      Destroy(gameObject);
    }
    else
    {
      Instance = this;
    }
    DontDestroyOnLoad(gameObject);

    sources = new AudioSource[transform.childCount];
    for (int i = 0; i < transform.childCount; i++)
    {
      AudioSource s = transform.GetChild(i).GetComponent<AudioSource>();
      if (s != null)
      {
        sources[i] = s;
        //sources[i].enabled = false;
      }
      else
      {
        throw new MissingComponentException("AudioManager child " + i + " does not have an AudioSource component.");
      }
    }
    //sources[0].enabled = true;
  }
}
