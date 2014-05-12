using Duality;

namespace ScreenCapturePlugin
{
	public class ScreenCapturePlugin : CorePlugin
	{
		private static ScreenCaptureSystem _screenCaptureSystem;

		public static void BeginCapture(bool captureAllPasses)
		{
			_screenCaptureSystem.BeginCapture(captureAllPasses);
		}

		public static void EndCapture()
		{
			_screenCaptureSystem.EndCapture();
		}

		public static void SetScreenshotQuality(int quality)
		{
			_screenCaptureSystem.SetScreenshotQuality(quality);
		}

		protected override void InitPlugin()
		{
			base.InitPlugin();

			_screenCaptureSystem = new ScreenCaptureSystem();	
		}
    }
}
