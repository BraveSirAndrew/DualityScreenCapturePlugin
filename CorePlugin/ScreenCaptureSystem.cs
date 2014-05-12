using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using Duality;
using Duality.Components;
using Duality.Resources;
using OpenTK;

namespace ScreenCapturePlugin
{
	public class ScreenCaptureSystem
	{
		private const int NumberOfCachedFrames = 10;
		
		private Dictionary<string, ScreenCaptureStream> _streams = new Dictionary<string, ScreenCaptureStream>();
		private ScreenCaptureWriter _writer = new ScreenCaptureWriter();
		private BlockingCollection<CapturedFrame> _frames;
		private ImageCodecInfo _imageCodecInfo;
		private bool _captureAllPasses;
		private int _passIndex;
		private Camera _camera;
		private Task[] _processingTasks;

		public void SetScreenshotQuality(int quality)
		{
			_writer.SetQuality(quality);
		}

		public void BeginCapture(bool captureAllPasses)
		{
			_camera = Scene.Current.FindComponent<Camera>();

			if (_camera == null)
			{
				Log.Game.WriteWarning("ScreenCapturePlugin: Screen capture can only capture frames from a camera but the current scene doesn't contain one.");
				return;
			}

			_captureAllPasses = captureAllPasses;
			_frames = new BlockingCollection<CapturedFrame>(NumberOfCachedFrames);
			_streams.Clear();

			_processingTasks = new[]
			{
				Task.Factory.StartNew(SaveBitmaps),
				Task.Factory.StartNew(SaveBitmaps),
				Task.Factory.StartNew(SaveBitmaps),
				Task.Factory.StartNew(SaveBitmaps)
			};

			if (_captureAllPasses)
			{
				foreach (var pass in _camera.Passes)
				{
					var streamName = pass.ToString().Replace(" => ", "-");

					var index = 0;
					while (_streams.ContainsKey(streamName))
						streamName = streamName + "(" + index + ")";

					_streams.Add(streamName, new ScreenCaptureStream(_frames, streamName, pass.Output, NumberOfCachedFrames));
				}

				_camera.RenderPassCompleted += OnRenderPassCompleted;
			}
			else
			{
				_streams.Add("Frame", new ScreenCaptureStream(_frames, "Frame", null, NumberOfCachedFrames));
			}

			_camera.RenderFrameCompleted += OnRenderFrameCompleted;
		}

		public void EndCapture()
		{
			if (_camera != null)
			{
				_camera.RenderPassCompleted -= OnRenderPassCompleted;
				_camera.RenderFrameCompleted -= OnRenderFrameCompleted;
			}

			_frames.CompleteAdding();
			Task.WaitAll(_processingTasks);
			_frames.Dispose();
		}

		private void OnRenderFrameCompleted(object sender, RendererFrameCompleteEventArgs e)
		{
			if(_captureAllPasses == false)
				CaptureStream("Frame", e.DrawDevice.TargetSize);

			_passIndex = 0;
		}

		private void OnRenderPassCompleted(object sender, RendererPassCompleteEventArgs e)
		{
			CaptureStream(_streams.Keys.ElementAt(_passIndex++), e.DrawDevice.TargetSize);
		}

		private void CaptureStream(string streamName, Vector2 targetSize)
		{
			ScreenCaptureStream stream;
			if (_streams.TryGetValue(streamName, out stream) == false)
			{
				Log.Editor.WriteError("ScreenCaptureSystem couldn't find a stream for pass {0}", streamName);
				return;
			}

			stream.Capture(targetSize);
		}

		private void SaveBitmaps()
		{
			foreach (var frame in _frames.GetConsumingEnumerable())
			{
				try
				{
					_writer.Save(frame);
				}
				catch (InvalidOperationException)
				{
				}
				catch (ArgumentException)
				{
				}
			}
		}
	}
}