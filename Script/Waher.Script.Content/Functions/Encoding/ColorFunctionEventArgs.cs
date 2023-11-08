using System;
using Waher.Content.QR.Encoding;

namespace Waher.Script.Content.Functions.Encoding
{
	/// <summary>
	/// Event arguments for events determoning if custom color coding of a QR code is to be applied.
	/// </summary>
	public class ColorFunctionEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for events determoning if custom color coding of a QR code is to be applied.
		/// </summary>
		/// <param name="Text">Content of QR code.</param>
		public ColorFunctionEventArgs(string Text)
		{
			this.Text = Text;
		}

		/// <summary>
		/// Contents of QR-code.
		/// </summary>
		public string Text { get; private set; }

		/// <summary>
		/// Custom color function.
		/// </summary>
		public ColorFunction Function { get; set; } = null;

		/// <summary>
		/// If anti-alias is to be applied.
		/// </summary>
		public bool AntiAlias { get; set; } = true;
	}
}
