
public class MyAudioClip
{
	public string name;


	public long timeStart;

	public MyAudioClip(string filename)
	{
		name = filename;
	}

	public void Play()
	{
		timeStart = mSystem.currentTimeMillis();
	}

	public bool isPlaying()
	{
		return false;
	}
}
