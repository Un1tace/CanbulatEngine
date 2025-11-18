using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using MiniAudioEx;
using MiniAudioEx.Core.StandardAPI;

namespace CSCanbulatEngine.Audio;

public class AudioEngine : IDisposable
{
    
    public readonly Dictionary<string, AudioClip> _loadedClips;
    private readonly List<AudioSource> _playingSources;

    public AudioEngine()
    {
        try
        {
            AudioContext.Initialize(44100, 2);
            _loadedClips = new Dictionary<string, AudioClip>();
            _playingSources = new List<AudioSource>();
            EngineLog.Log("[AudioEngine] MiniAudioExNET Context Initialized.");
        }
        catch (Exception e)
        {
            EngineLog.Log($"[AudioEngine] CRITICAL FAILURE to initialize MiniAudioExNET: {e.Message}");
            throw;
        }
    }

    public void LoadSound(string name, string path, bool streamFromFile = false)
    {
        if (_loadedClips.ContainsKey(name))
        {
            EngineLog.Log($"[AudioEngine] Sound '{name}' already loaded.");
            return;
        }

        try
        {
            var clip = new AudioClip(path, streamFromFile);
            
            clip.Name = name;
            
            _loadedClips[name] = clip;
            EngineLog.Log($"[AudioEngine] Loaded sound '{name}' from '{path}' (Streaming: {streamFromFile}).");
        }
        catch (Exception ex)
        {
            EngineLog.Log($"[AudioEngine] Failed to load sound '{name}' from '{path}'. Error: {ex.Message}");
        }
    }

    public void PlaySound(string name)
    {
        if (!_loadedClips.TryGetValue(name, out var clip))
        {
            EngineLog.Log($"[AudioEngine] Sound '{name}' isn't found.");
            return;
        }

        try
        {
            var source = new AudioSource();
            
            _playingSources.Add(source);
            
            source.Play(clip);
        }
        catch (Exception ex)
        {
            EngineLog.Log($"[AudioEngine] Failed to play sound '{name}'. Error: {ex.Message}");
        }
    }
    
    private void OnSoundFinished(object sender, EventArgs e)
    {
    }

    public void Update()
    {
        for (int i = _playingSources.Count - 1; i >= 0; i--)
        {
            var source = _playingSources[i];
            if (!source.IsPlaying)
            {
                _playingSources.RemoveAt(i);
                source.Dispose();
            }
        }
        AudioContext.Update();
    }

    public void Dispose()
    {
        foreach (var clip in _loadedClips.Values)
        {
            clip.Dispose();
        }
        _loadedClips.Clear();
        AudioContext.Deinitialize();
        EngineLog.Log("[AudioEngine] MiniAudioExNET Deinitialized.");
    }
}

public class AudioInfo
{
    public string Name;
    public bool isLoaded;
    public string pathToAudio;
}