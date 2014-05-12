using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Duality.Editor;

namespace ScreenCapturePlugin.Editor
{
	/// <summary>
	/// Defines a Duality editor plugin.
	/// </summary>
    public class ScreenCapturePluginEditorPlugin : EditorPlugin
	{
		public override string Id
		{
			get { return "ScreenCapturePluginEditorPlugin"; }
		}
	}
}
