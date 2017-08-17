//
// MLResourceManager.cs
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
using LuaInterface;

public class SoundManager : DDOLSingleton<SoundManager>
{
	private const string BASE_SOUND_PREFAB = "BaseAudio";   // base sound prefab

	// 是否播放音乐
	private bool isPlayingMusic = true;

	// 是否播放音效
	private bool isPlaySound = true;

    // 音乐音量
    private float musicVolume = 1.0f;

    // 音效音量
    private float soundVolume = 1.0f;

    // 上一个播放的音效及Date
    private string lastPlayName = "";
	private long lastPlayTime = 0;

	// 声音基本数据
    private Dictionary<string, SoundData> audioInfos = new Dictionary<string, SoundData>();

	// 音源池相关
	private Dictionary<string, Queue<AudioSource>> cacheAudios = new Dictionary<string, Queue<AudioSource>>();
	private Dictionary<string, Queue<AudioSource>> playingAudios = new Dictionary<string, Queue<AudioSource>>();
	private List<AudioSource> removeAuidos = new List<AudioSource>();

    private StringBuilder strBuilder = new StringBuilder();

	#region preload audios
    // 预加载音乐
	public void PreloadAudio(string audioName, string path)
	{
		if (string.IsNullOrEmpty(audioName))
			return;

		SoundData soundData = SoundDataTable.Get(audioName);
		if (soundData == null)
		{
			Debugger.LogWarning("Preload audio error! Sound data not find. name==" + audioName);
			return;
		}

        SoundData audioInfo = null;
		if (audioInfos.TryGetValue(audioName, out audioInfo))
			return;

		// 创建声音基本信息数据
		audioInfos.Add(audioName, soundData);

		// 缓存音源
		Queue<AudioSource> audios = new Queue<AudioSource>();
		cacheAudios.Add(audioName, audios);
		int preloadCnt = soundData.Cache;
		for (int i = 0; i < preloadCnt; i++)
		{
            AudioSource audioSource = MLResourceManager.Instance.LoadInstance<AudioSource>(BASE_SOUND_PREFAB, ResourceType.RES_AUDIO);

            strBuilder.Length = 0;
            strBuilder.AppendFormat("{0}/{1}", path, audioName);
            AudioClip audioClip = MLResourceManager.Instance.LoadResource(strBuilder.ToString(), ResourceType.RES_AUDIO) as AudioClip;

			audioSource.clip = audioClip;
			audioSource.playOnAwake = false;
			audioSource.name = audioName;
			audioSource.transform.parent = this.transform;
			audioSource.loop = soundData.Loop;

			audios.Enqueue(audioSource);
		}
	}
	#endregion

	#region play audio
	public AudioSource PlayAudio(string audioName)
	{
        SoundData audioInfo = GetAudioInfo(audioName);
		if (audioInfo == null)
		{
			return null;
		}

		// 根据声音类型判断是否可以播放
		AudioType type = (AudioType)audioInfo.Type;
		bool isNeedPlay = true;
		if (type == AudioType.Music)
		{
			isNeedPlay = isPlayingMusic;

			if (!isPlayingMusic)
				GetReadyPlayAudio(audioInfo);
		}
		else
		{
			isNeedPlay = isPlaySound;
		}

		if (!isNeedPlay)
			return null;

		AudioSource audio = GetReadyPlayAudio(audioInfo);
		if (audio == null)
			return null;
        
        // 设置音源音量
        audio.volume = (type == AudioType.Music ? musicVolume : soundVolume);

        bool delay = true;
		if (type == AudioType.Sound && audioInfo.FirstDelay)
			delay = IsNeedDelay(audioName);
		if (delay == true)
			audio.PlayDelayed(audioInfo.DelayTime);
		else
			audio.Play();

		return audio;
	}

	private bool IsNeedDelay(string audioName)
	{
		long now = TimeUtil.DateTimeToMS(TimeUtil.GetNow());
		if (lastPlayName != audioName)
		{
			lastPlayName = audioName;
			lastPlayTime = now;
			return false;
		}
		else
		{
			long dis = now - lastPlayTime;
			lastPlayName = audioName;
			lastPlayTime = now;
			if (dis > 500)
				return false;
			return true;
		}
	}

	#endregion

	#region stop & pause audio   
	public void StopAudio(string audioName)
	{
		if (playingAudios.Count <= 0)
		{
			return;
		}

		var enumerator = playingAudios.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string name = enumerator.Current.Key;
			if (name != audioName)
				continue;

			Queue<AudioSource> audios = enumerator.Current.Value;
			while (audios.Count > 0)
			{
				AudioSource source = audios.Dequeue();
				source.Stop();

				Recycle2CacheAudios(name, source);
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
            string audioName = enumerator.Current.Key;
            SoundData audioInf = GetAudioInfo(audioName);
            if ((AudioType)audioInf.Type != type)
                continue;

            Queue<AudioSource> audios = enumerator.Current.Value;
            while (audios.Count > 0)
            {
                AudioSource source = audios.Dequeue();
                source.Stop();

                Recycle2CacheAudios(audioName, source);
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
			string audioName = enumerator.Current.Key;
			Queue<AudioSource> audios = enumerator.Current.Value;
			while (audios.Count > 0)
			{
				AudioSource source = audios.Dequeue();
				source.Stop();

				Recycle2CacheAudios(audioName, source);
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
			string audioName = enumerator.Current.Key;
			Queue<AudioSource> audios = enumerator.Current.Value;

            SoundData audioInf = GetAudioInfo(audioName);
			if ((AudioType)audioInf.Type == AudioType.Sound)
				continue;

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

    //调整音乐音量
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
            string audioName = enumerator.Current.Key;
            Queue<AudioSource> audios = enumerator.Current.Value;

            SoundData audioInf = GetAudioInfo(audioName);
            if ((AudioType)audioInf.Type == AudioType.Sound)
                continue;

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
			string audioName = enumerator.Current.Key;
			Queue<AudioSource> audios = enumerator.Current.Value;

            SoundData audioInf = GetAudioInfo(audioName);
			if ((AudioType)audioInf.Type == AudioType.Sound || audios.Count <= 0)
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

		audioInfos.Clear();
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

    private SoundData GetAudioInfo(string audioName)
	{
        SoundData audioInfo = null;
		if (!audioInfos.TryGetValue(audioName, out audioInfo))
		{
			Debugger.LogWarning("Can't get audio info name==" + audioName);
			return null;
		}

		return audioInfo;
	}

    private AudioSource GetReadyPlayAudio(SoundData soundInfo)
	{
		AudioType type = (AudioType)soundInfo.Type;
		string audioName = soundInfo.NAME;
		Queue<AudioSource> sources = null;
		if (!cacheAudios.TryGetValue(audioName, out sources))
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
			Cache2PlayingAudios(audioName, source);
			return source;
		}

		// 已全部都在播放队列的情况
		if (sources.Count <= 0)
		{
			// 顶掉播放队首的音效
			source = GetPlayingAudio(audioName);
			Cache2PlayingAudios(audioName, source);
			return source;
		}

		source = sources.Dequeue();
		Cache2PlayingAudios(audioName, source);
		return source;
	}

	private void Cache2PlayingAudios(string audioName, AudioSource source)
	{
		Queue<AudioSource> audios = null;
		if (!playingAudios.TryGetValue(audioName, out audios))
		{
			audios = new Queue<AudioSource>();
			playingAudios.Add(audioName, audios);
		}

		audios.Enqueue(source);
	}

	private void Recycle2CacheAudios(string audioName, AudioSource source)
	{
		Queue<AudioSource> audios = null;
		if (!cacheAudios.TryGetValue(audioName, out audios))
		{
			Debugger.LogError("Recycle to cache audios error! name==" + audioName);
			return;
		}

		audios.Enqueue(source);
	}

	private AudioSource GetPlayingAudio(string audioName)
	{
		Queue<AudioSource> audios = null;
		if (!playingAudios.TryGetValue(audioName, out audios))
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
			string name = enumerator.Current.Key;
			SoundData audioInfo = GetAudioInfo(name);

			// 如果音源播放是循环的情况， 不用走后续逻辑
			if (audioInfo.Loop)
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
			Recycle2CacheAudios(audioInfo.NAME, source);
		}
	}
}
