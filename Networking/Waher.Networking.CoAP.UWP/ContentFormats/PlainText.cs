using System;

namespace Waher.Networking.CoAP.ContentFormats
{
	/// <summary>
	/// Plain text
	/// </summary>
	public class PlainText : ICoapContentFormat
	{
		/// <summary>
		/// 0
		/// </summary>
		public const int ContentFormatCode = 0;

		/// <summary>
		/// Plain text
		/// </summary>
		public PlainText()
		{
		}

		/// <summary>
		/// Content format number
		/// </summary>
		public int ContentFormat
		{
			get { return ContentFormatCode; }
		}

		/// <summary>
		/// Internet content type.
		/// </summary>
		public string ContentType
		{
			get { return "text/plain; charset=utf-8"; }
		}
	}
}
