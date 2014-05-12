namespace ScreenCapturePlugin
{
	public struct CapturedFrame
	{
		public byte[] Bytes;
		public int Width;
		public int Height;
		public string StreamName;
		public int FrameIndex;
		public bool Flip;
	}
}