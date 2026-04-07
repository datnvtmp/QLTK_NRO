public class ScaleGUI
{
	public static bool scaleScreen;

	public static float WIDTH = 800;

	public static float HEIGHT = 600;

	public static void initScaleGUI()
	{
		WIDTH = Screen.width;
		HEIGHT = Screen.height;
		scaleScreen = false;
	}

	public static void BeginGUI() { }

	public static void EndGUI() { }

	public static float scaleX(float x)
	{
		if (!scaleScreen) return x;
		return x * WIDTH / Screen.width;
	}

	public static float scaleY(float y)
	{
		if (!scaleScreen) return y;
		return y * HEIGHT / Screen.height;
	}
}

