using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ScreenCapturePlugin
{
	public class ScreenCaptureWriter
	{
		private ImageCodecInfo _imageCodecInfo;
		private EncoderParameters _encoderParams;
		private Encoder _encoder;

		public ScreenCaptureWriter()
		{
			var codecs = ImageCodecInfo.GetImageDecoders();
			foreach (var imageCodecInfo in codecs)
			{
				if (imageCodecInfo.FormatID == ImageFormat.Jpeg.Guid)
				{
					_imageCodecInfo = imageCodecInfo;
					break;
				}
			}

			_encoder = Encoder.Quality;
			_encoderParams = new EncoderParameters(1);
			var qualityParam = new EncoderParameter(_encoder, 100L);
			_encoderParams.Param[0] = qualityParam;
		}

		public void SetQuality(int quality)
		{
			_encoderParams.Param[0] = new EncoderParameter(_encoder, quality);
		}

		public unsafe void Save(CapturedFrame frame)
		{
			fixed (byte* scan0 = frame.Bytes)
			{
				using (var bm = new Bitmap(frame.Width, frame.Height, 4 * frame.Width, PixelFormat.Format32bppArgb, (IntPtr)scan0))
				{
					if (frame.Flip)
						bm.RotateFlip(RotateFlipType.RotateNoneFlipY);

					bm.Save(string.Format("screenshots\\{0}{1}.jpeg", frame.StreamName, frame.FrameIndex), _imageCodecInfo, _encoderParams);
				}
			}
		}
	}
}