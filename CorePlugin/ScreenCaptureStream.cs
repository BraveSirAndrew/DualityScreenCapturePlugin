using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Duality;
using Duality.Resources;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ScreenCapturePlugin
{
	public class ScreenCaptureStream
	{
		private readonly int _numberOfCachedFrames;

		[DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
		public static extern void memcpy(IntPtr dest, IntPtr src, int count);

		private BlockingCollection<CapturedFrame> _frames;
		private readonly ContentRef<RenderTarget> _output;
		private List<CapturedFrame> _frameCache;
		private int[] _pboIds = new int[2];
		private Vector2 _previousTargetSize;
		private int _cachedFramesIndex;
		private int _nextPboIndex;
		private int _frameIndex;
		private int _pboIndex;

		public string StreamName { get; private set; }

		public ScreenCaptureStream(BlockingCollection<CapturedFrame> frames, string streamName, ContentRef<RenderTarget> output, int numberOfCachedFrames)
		{
			_frames = frames;
			_output = output;
			StreamName = streamName;

			_numberOfCachedFrames = numberOfCachedFrames;

			_pboIds[0] = GL.GenBuffer();
			_pboIds[1] = GL.GenBuffer();
		}

		public unsafe void Capture(Vector2 targetSize)
		{
			if (_previousTargetSize != targetSize)
			{
				SetupBuffers(targetSize);
				_previousTargetSize = targetSize;
			}

			_pboIndex = (_pboIndex + 1) % 2;
			_nextPboIndex = (_pboIndex + 1) % 2;

			var frame = _frameCache[_cachedFramesIndex++];
			_cachedFramesIndex %= _numberOfCachedFrames;

			frame.FrameIndex = _frameIndex++;

			GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
			GL.BindBuffer(BufferTarget.PixelPackBuffer, _pboIds[_pboIndex]);
			GL.ReadPixels(0, 0, (int)targetSize.X, (int)targetSize.Y, PixelFormat.Bgra, PixelType.UnsignedByte, (IntPtr)0);

			GL.BindBuffer(BufferTarget.PixelPackBuffer, _pboIds[_nextPboIndex]);
			var ptr = GL.MapBufferRange(BufferTarget.PixelPackBuffer, (IntPtr)0, (IntPtr)GetTargetSizeInBytes(targetSize), BufferAccessMask.MapReadBit);

			fixed (byte* data = frame.Bytes)
			{
				memcpy((IntPtr)data, ptr, GetTargetSizeInBytes(targetSize));
			}

			GL.UnmapBuffer(BufferTarget.PixelPackBuffer);
			GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
			GL.ReadBuffer(ReadBufferMode.Back);

			_frames.TryAdd(frame);
		}

		private void SetupBuffers(Vector2 size)
		{
			GL.BindBuffer(BufferTarget.PixelPackBuffer, _pboIds[0]);
			GL.BufferData(BufferTarget.PixelPackBuffer, (IntPtr)(GetTargetSizeInBytes(size)), (IntPtr)0, BufferUsageHint.DynamicRead);

			GL.BindBuffer(BufferTarget.PixelPackBuffer, _pboIds[1]);
			GL.BufferData(BufferTarget.PixelPackBuffer, (IntPtr)(GetTargetSizeInBytes(size)), (IntPtr)0, BufferUsageHint.DynamicRead);

			GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);

			_frameCache = new List<CapturedFrame>();
			for (var i = 0; i < _numberOfCachedFrames; i++)
			{
				_frameCache.Add(new CapturedFrame
				{
					Width = (int)size.X,
					Height = (int)size.Y,
					StreamName = StreamName,
					Flip = _output == null,
					Bytes = new byte[(int)(size.X * size.Y * 4)]
				});
			}
		}

		private static int GetTargetSizeInBytes(Vector2 size)
		{
			return (int)(size.X * size.Y * 4);
		}
	}
}