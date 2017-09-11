using UnityEngine;

public class TPlaySound : MonoBehaviour 
{
	private const string PLAY_BG_MUSIC_TEXT = "播放背景音乐";
    private const string STOP_BG_MUSIC_TEXT = "停止背景音乐";

	private const string PLAY_GUN_SOUND_TEXT = "播放枪音效";
    private const string STOP_GUN_SOUND_ON_TEXT = "停止枪音效";

	private const string SET_BG_MUSIC_ON_TEXT = "设置音乐打开";
	private const string SET_BG_MUSIC_OFF_TEXT = "设置音乐关闭";

    private const string ADD_BG_MUSIC_VOLUME = "增大音乐音量";
    private const string SUB_BG_MUSIC_VOLUME = "降低音乐音量";

    private bool isPlayMusic = false;

    private GUIStyle btnStyle;

    private float musicVolume = 1.0f;
    private float volumeStep = 0.1f;

	void Start () 
    {
		MLResourceManager resMgr = MLResourceManager.Instance;
		resMgr.InitResourceDefine(new GameResDefine());
		resMgr.CreateResourceMgr(new ResourcesManager());
		resMgr.ChangeResourceMgr(ResManagerType.resourceMgr);

        GameAudioManager.Instance.PreloadAudio(GameConst.AUDIO_MUSIC_GAME_BG);
        GameAudioManager.Instance.PreloadAudio(GameConst.AUDIO_SOUND_FIRE_GUN);

		btnStyle = new GUIStyle("button");
		btnStyle.fontSize = 32;        
	}

	void OnGUI()
	{
		string strSetMusic = isPlayMusic ? SET_BG_MUSIC_ON_TEXT : SET_BG_MUSIC_OFF_TEXT;
		if (GUI.Button(new Rect(10, 10, 200, 50), strSetMusic, btnStyle))
		{
			SettingMusic(isPlayMusic);
			isPlayMusic = !isPlayMusic;
		}

		if (GUI.Button(new Rect(260, 10, 200, 50), ADD_BG_MUSIC_VOLUME, btnStyle))
		{
            float volume = musicVolume + volumeStep;
            musicVolume = volume > 1.0f ? 1.0f : volume;

            SetMusicVolume(musicVolume);
            return;
		}

		if (GUI.Button(new Rect(510, 10, 200, 50), SUB_BG_MUSIC_VOLUME, btnStyle))
		{
			float volume = musicVolume - volumeStep;
			musicVolume = volume < 0.0f ? 0.0f : volume;

            SetMusicVolume(musicVolume);
            return;
		}

		if (GUI.Button(new Rect(10, 70, 200, 50), PLAY_BG_MUSIC_TEXT, btnStyle))
		{
            PlayBGAudio();
			return;
		}

		if (GUI.Button(new Rect(260, 70, 200, 50), STOP_BG_MUSIC_TEXT, btnStyle))
		{
            StopBGAudio();
			return;
		}

		if (GUI.Button(new Rect(10, 130, 200, 50), PLAY_GUN_SOUND_TEXT, btnStyle))
		{
            PlayGunAudio();
			return;
		}

		if (GUI.Button(new Rect(260, 130, 200, 50), STOP_GUN_SOUND_ON_TEXT, btnStyle))
		{
            StopGunAudio();
			return;
		}
	}

    private void PlayBGAudio()
    {
        GameAudioManager.Instance.PlayAudio(GameConst.AUDIO_MUSIC_GAME_BG);
    }

    private void StopBGAudio()
    {
        GameAudioManager.Instance.StopAudio(GameConst.AUDIO_MUSIC_GAME_BG);
    }

    private void PlayGunAudio()
    {
        GameAudioManager.Instance.PlayAudio(GameConst.AUDIO_SOUND_FIRE_GUN);
    }

    private void StopGunAudio()
    {
        GameAudioManager.Instance.StopAudio(GameConst.AUDIO_SOUND_FIRE_GUN);
    }

    private void SettingMusic(bool isPlayMusic)
    {
        GameAudioManager.Instance.MusicSetting(isPlayMusic);
    }

    private void SetMusicVolume(float volume)
    {
        GameAudioManager.Instance.AddOrSubAudiosVolume(AudioType.Music, volume);
    }

}
