using System;
using Duality;
using Duality.Editor;

namespace ScreenCapturePlugin
{
	[Serializable]
	public class ScreenCaptureComponent : Component, ICmpInitializable
	{
		private bool _captureEnabled;
		private int _quality;

		public bool CaptureEnabled
		{
			get { return _captureEnabled; }
			set
			{
				_captureEnabled = value;

				if (DualityApp.ExecContext != DualityApp.ExecutionContext.Game) 
					return;

				if (_captureEnabled)
					BeginScreenCapture(CaptureAllCameraPasses);
				else
					EndScreenCapture();
			}
		}

		[EditorHintRange(0, 100)]
		public int Quality
		{
			get { return _quality; }
			set
			{
				_quality = value;

				if (DualityApp.ExecContext == DualityApp.ExecutionContext.Game)
				{
					ScreenCapturePlugin.SetScreenshotQuality(_quality);
				}
			}
		}

		public bool CaptureAllCameraPasses { get; set; }

		public void OnInit(InitContext context)
		{
			if (context != InitContext.Activate)
				return;

			if (CaptureEnabled)
			{
				BeginScreenCapture(CaptureAllCameraPasses);
			}
		}

		public void OnShutdown(ShutdownContext context)
		{
			if (context != ShutdownContext.Deactivate || DualityApp.ExecContext != DualityApp.ExecutionContext.Game)
				return;

			if (CaptureEnabled)
			{
				EndScreenCapture();
			}
		}

		private static void BeginScreenCapture(bool captureAllPasses)
		{
			ScreenCapturePlugin.BeginCapture(captureAllPasses);
		}

		private void EndScreenCapture()
		{
			ScreenCapturePlugin.EndCapture();
		}
	}
}