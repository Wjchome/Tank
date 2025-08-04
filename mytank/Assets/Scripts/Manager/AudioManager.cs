using UnityEngine;
using System.Collections.Generic;

public class AudioManager : SingletonMono<AudioManager>
{
    
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        public bool loop = false;
        [HideInInspector] public AudioSource source;
    }
    
    public List<Sound> sounds;
    public Dictionary<string, Sound> soundsMap;
    
    void Awake()
    {
        
        soundsMap = new Dictionary<string, Sound>();
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.loop = s.loop;
            soundsMap.Add(s.name, s);
        }
    }
    
    public void Play(string name)
    {
        if(soundsMap.TryGetValue(name, out Sound s))
        {
             s.source.Play();
            
        }
        else
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
    }
    
    public void Stop(string name)
    {
        if(soundsMap.TryGetValue(name, out Sound s))
        {
            s.source.Stop();
            
        }
        else
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
    }
}