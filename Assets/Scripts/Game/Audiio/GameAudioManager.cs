using UnityEngine;
using System.Collections.Generic;

public class GameAudioManager : AudioManager<GameAudioManager, AudioMeta>
{
	protected override bool PreloadAudioData(Dictionary<int, AudioMeta> audioDatas, int audioId)
	{
		AudioMeta audioMeta = null;
		if (audioDatas.TryGetValue(audioId, out audioMeta))
        {
            return true;
        }

        audioMeta = PBMetaManager.Instance.GetMeta<AudioMetaTable, AudioMeta>(audioId);
        if (audioMeta == null)
        {
            Debug.LogWarning("Preload audio error! Sound data not find. id==" + audioId);
            return true;
        }

        audioDatas.Add(audioId, audioMeta);
        return false;
	}

    private AudioMeta GetAudioData(int audioId)
	{
        AudioMeta audioInfo = null;
        if (!audioDatas.TryGetValue(audioId, out audioInfo))
        {
          Debug.LogWarning("Can't get audio info name==" + audioId);
          return null;
        }

        return audioInfo;
    }

	protected override bool IsAudioLoad(int audioId)
    {
        if (GetAudioData(audioId) != null)
        {
            return true;
        }

        return false;
    }

	protected override string GetAudioName(int audioId)
	{
        AudioMeta audioMeta = GetAudioData(audioId);
        if(audioMeta == null)
        {
            return null;
        }

        return audioMeta.name;
	}

    protected override int GetAudioCacheNum(int audioId)
    {
		AudioMeta audioMeta = GetAudioData(audioId);
		if (audioMeta == null)
		{
            return 0;
		}

        return audioMeta.cache;
    }

    protected override AudioType GetAudioType(int audioId)
    {
		AudioMeta audioMeta = GetAudioData(audioId);
		if (audioMeta == null)
		{
			return AudioType.None;
		}

        return (AudioType)audioMeta.type;
    }

    protected override bool IsAudioLoop(int audioId)
    {
		AudioMeta audioMeta = GetAudioData(audioId);
        if (audioMeta == null)
        {
            return false;
        }

        return audioMeta.loop;
    }

	protected override float GetAudioDelay(int audioId)
	{
		AudioMeta audioMeta = GetAudioData(audioId);
		if (audioMeta == null)
		{
            return 0f;
		}

        return audioMeta.delayTime;
	}
}
