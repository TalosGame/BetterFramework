//
// AudioManager.cs
//
// Author:
//       wangquan <wangquancomi@gmail.com>
//       QQ: 408310416
// Desc:
//      1.预缓存声音,能控制音乐音效是否循环和延迟播放.
//      2.能支持同时播放多个相同的声音有叠加效果.
//      3.当缓存池个数不够的情况,需要把播放列表中最早播放的声音顶掉.
//
// Copyright (c) 2017 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public abstract class AudioManager<T, TD> : MonoBehaviour 
    where T : AudioManager<T, TD>
    where TD : class
{
	private const string BASE_SOUND_PREFAB = "BaseAudio";

	// 是否播放音乐
	private bool isPlayingMusic = true;

	// 是否播放音效
	private bool isPlaySound = true;

    // 音乐音量
    private float musicVolume = 1.0f;

    // 音效音量
    private float soundVolume = 1.0f;

    // 上一个播放的音效及Date
    private int lastPlayAudioId = -1;

	// 声音基本数据
    protected Dictionary<int, TD> audioDatas = new Dictionary<int, TD>();

	// 音源池相关
	private Dictionary<int, Queue<AudioSource>> cacheAudios = new Dictionary<int, Queue<AudioSource>>();
	private Dictionary<int, Queue<AudioSource>> playingAudios = new Dictionary<int, Queue<AudioSource>>();
	private List<AudioSource> removeAuidos = new List<AudioSource>();

	private static T _instance;
	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType<T>();

				if (_instance == null)
				{
					GameObject go = new GameObject(typeof(T).Name);
					_instance = go.AddComponent<T>();

					go.AddToDDOLRoot();
				}

				_instance.Init();
			}

			return _instance;
		}
	}

	protected virtual void Init() { }

	void OnDestroy()
	{
		_instance = null;
		Destroy();
		Debug.Log(this.gameObject.name + " OnDestroy!");
	}

	protected virtual void Destroy() { }

    #region audio base interface
    protected abstract bool IsAudioLoad(int audioId);

    protected abstract AudioType GetAudioType(int audioId);

    protected abstract int GetAudioCacheNum(int audioId);

    protected abstract string GetAudioName(int audioId);

	protected abstract bool IsAudioLoop(int audioId);

    protected abstract float GetAudioDelay(int audioId);
    #endregion

    #region preload audios
    public void PreloadAudio(int audioId)
    {
        if(PreloadAudioData(audioDatas, audioId))
        {
            return;
        }

		Queue<AudioSource> audios = new Queue<AudioSource>();
		cacheAudios.Add(audioId, audios);
		int preloadCnt = GetAudioCacheNum(audioId);
		for (int i = 0; i < preloadCnt; i++)
		{
			AudioSource audioSource = MLResourceManager.Instance.LoadInstance<AudioSource>(BASE_SOUND_PREFAB, ResourceType.RES_AUDIO);

            string audioName = GetAudioName(audioId);
			AudioClip audioClip = MLResourceManager.Instance.LoadResource(audioName, ResourceType.RES_AUDIO) as AudioClip;

			audioSource.clip = audioClip;
			audioSource.playOnAwake = false;
			audioSource.name = audioName;
			audioSource.transform.parent = this.transform;
            audioSource.loop = IsAudioLoop(audioId);

			audios.Enqueue(audioSource);
		}
    }

    protected abstract bool PreloadAudioData(Dictionary<int, TD> audioDatas, int audioId);
	#endregion

	#region play audio
    public AudioSource PlayAudio(int audioId)
	{
        if(!IsAudioLoad(audioId))
        {
            return null;
        }

		// 根据声音类型判断是否可以播放
        AudioType type = GetAudioType(audioId);
		bool isNeedPlay = true;
		if (type == AudioType.Music)
		{
			isNeedPlay = isPlayingMusic;

            // 即使设置不播放音乐,也需要加到播放列表中
			if (!isPlayingMusic)
                GetReadyPlayAudio(audioId, type);
		}
		else
		{
			isNeedPlay = isPlaySound;
		}

		if (!isNeedPlay)
			return null;

        AudioSource audio = GetReadyPlayAudio(audioId, type);
		if (audio == null)
			return null;
        
        // 设置音源音量
        audio.volume = (type == AudioType.Music ? musicVolume : soundVolume);

        bool delay = CheckPlayDelay(audioId);
        if (delay == true)
        {
            audio.PlayDelayed(GetAudioDelay(audioId));
        }else
        {
            audio.Play();
        }

		return audio;
	}

    private bool CheckPlayDelay(int audioId)
    {
        // 首次播放不同的声音不需要延迟
		if (lastPlayAudioId != audioId)
		{
			lastPlayAudioId = audioId;
			return false;
		}

        return true;
    }
	#endregion

	#region stop & pause audio   
    public void StopAudio(int audioId)
	{
		if (playingAudios.Count <= 0)
		{
			return;
		}

		var enumerator = playingAudios.GetEnumerator();
		while (enumerator.MoveNext())
		{
            int id = enumerator.Current.Key;
			if (id != audioId)
				continue;

			Queue<AudioSource> audios = enumerator.Current.Value;
			while (audios.Count > 0)
			{
				AudioSource source = audios.Dequeue();
				source.Stop();

				Recycle2CacheAudios(id, source);
			}
		}
	}

    public void StopAudios(AudioType type)
    {
        if (playingAudios.Count <= 0)
        {
            return;
        }

        var enumerator = playingAudios.GetEnumerator();
        while (enumerator.MoveNext())
        {
            int id = enumerator.Current.Key;
            AudioType audioType = GetAudioType(id);
            if (audioType != type)
                continue;

            Queue<AudioSource> audios = enumerator.Current.Value;
            while (audios.Count > 0)
            {
                AudioSource source = audios.Dequeue();
                source.Stop();

                Recycle2CacheAudios(id, source);
            }
        }
    }

	public void StopAudios()
	{
		if (playingAudios.Count <= 0)
		{
			return;
		}

		var enumerator = playingAudios.GetEnumerator();
		while (enumerator.MoveNext())
		{
            int id = enumerator.Current.Key;
			Queue<AudioSource> audios = enumerator.Current.Value;
			while (audios.Count > 0)
			{
				AudioSource source = audios.Dequeue();
				source.Stop();

				Recycle2CacheAudios(id, source);
			}
		}
	}

	public void PauseAndUnPauseAudios(bool isPause)
	{
		if (playingAudios.Count <= 0)
		{
			return;
		}

		var enumerator = playingAudios.GetEnumerator();
		while (enumerator.MoveNext())
		{
            int id = enumerator.Current.Key;
            AudioType audioType = GetAudioType(id);
			if (audioType == AudioType.Sound)
				continue;

            Queue<AudioSource> audios = enumerator.Current.Value;
			foreach (AudioSource source in audios)
			{
				if (isPause)
				{
					source.Pause();
					continue;
				}

				source.UnPause();
				if (!source.isPlaying)
					source.Play();
			}
		}
	}

    public void AddOrSubAudiosVolume(AudioType type, float volume)
    {
        if (type == AudioType.Music)
        {
            musicVolume = volume;
            SettingMusicAudioVolume(volume);
            return;
        }

        if (type == AudioType.Sound)
        {
            soundVolume = volume;
            return;
        }
    }

    private void SettingMusicAudioVolume(float volume)
    {
        if (playingAudios.Count <= 0)
        {
            return;
        }

        var enumerator = playingAudios.GetEnumerator();
        while (enumerator.MoveNext())
        {
            int id = enumerator.Current.Key;
            AudioType audioType = GetAudioType(id);
            if (audioType == AudioType.Sound)
                continue;

            Queue<AudioSource> audios = enumerator.Current.Value;
            foreach (AudioSource source in audios)
            {
                source.volume = volume;
            }
        }
    }

    public void FadeOutAudioVolume(float time)
	{
		if (playingAudios.Count <= 0 || !isPlayingMusic)
		{
			return;
		}

		var enumerator = playingAudios.GetEnumerator();
		while (enumerator.MoveNext())
		{
            int id = enumerator.Current.Key;
			Queue<AudioSource> audios = enumerator.Current.Value;

            AudioType type = GetAudioType(id);
			if (type == AudioType.Sound || audios.Count <= 0)
				continue;

			AudioSource source = audios.Peek();
			if (!source.isPlaying)
				continue;

			StartCoroutine(FadeOutAudioVolume(source, time));
		}
	}

	private IEnumerator FadeOutAudioVolume(AudioSource audioSource, float time)
	{
		float startVolume = audioSource.volume;
		while (audioSource.volume > 0)
		{
			audioSource.volume -= startVolume * Time.deltaTime / time;
			yield return null;
		}

		audioSource.Stop();
		audioSource.volume = startVolume;
	}

	public void RelaseAllAudios()
	{
		StopAudios();

        audioDatas.Clear();
		cacheAudios.Clear();
		playingAudios.Clear();

		int childCnt = this.transform.childCount;
		for (int i = childCnt - 1; i >= 0; i--)
		{
			Transform childTrans = this.transform.GetChild(i);
			GameObject.DestroyObject(childTrans.gameObject);
		}
	}

    #endregion

	#region setting audio
	public void MusicSetting(bool isPlaying)
	{
		this.isPlayingMusic = isPlaying;

		PauseAndUnPauseAudios(!isPlaying);
	}

	public void SoundSetting(bool isPlaying)
	{
		this.isPlaySound = isPlaying;
	}
	#endregion

    private AudioSource GetReadyPlayAudio(int audioId, AudioType type)
	{
		Queue<AudioSource> sources = null;
		if (!cacheAudios.TryGetValue(audioId, out sources))
		{
			return null;
		}

		AudioSource source = null;
		if (type == AudioType.Music)
		{
			// 已在播放队列中
			if (sources.Count <= 0)
				return null;

			source = sources.Dequeue();
			Cache2PlayingAudios(audioId, source);
			return source;
		}

		// 已全部都在播放队列的情况
		if (sources.Count <= 0)
		{
			// 顶掉播放队首的音效
			source = GetPlayingAudio(audioId);
			Cache2PlayingAudios(audioId, source);
			return source;
		}

		source = sources.Dequeue();
		Cache2PlayingAudios(audioId, source);
		return source;
	}

    private void Cache2PlayingAudios(int audioId, AudioSource source)
	{
		Queue<AudioSource> audios = null;
		if (!playingAudios.TryGetValue(audioId, out audios))
		{
			audios = new Queue<AudioSource>();
			playingAudios.Add(audioId, audios);
		}

		audios.Enqueue(source);
	}

    private void Recycle2CacheAudios(int audioId, AudioSource source)
	{
		Queue<AudioSource> audios = null;
		if (!cacheAudios.TryGetValue(audioId, out audios))
		{
			Debug.LogError("Recycle to cache audios error! name==" + audioId);
			return;
		}

		audios.Enqueue(source);
	}

    private AudioSource GetPlayingAudio(int audioId)
	{
		Queue<AudioSource> audios = null;
		if (!playingAudios.TryGetValue(audioId, out audios))
		{
			return null;
		}

		return audios.Dequeue();
	}

	void Update()
	{
		if (playingAudios.Count <= 0)
		{
			return;
		}

		var enumerator = playingAudios.GetEnumerator();
		while (enumerator.MoveNext())
		{
            int id = enumerator.Current.Key;

			// 如果音源播放是循环的情况， 不用后续回收逻辑
			if(IsAudioLoop(id))
            {
                continue;
            }

			Queue<AudioSource> audios = enumerator.Current.Value;
			if (audios.Count <= 0)
			{
				continue;
			}

			AudioSource source = audios.Peek();
			if (source.isPlaying)
				continue;

			audios.Dequeue();
			Recycle2CacheAudios(id, source);
		}
	}
}
