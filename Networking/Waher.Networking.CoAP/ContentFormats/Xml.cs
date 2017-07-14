using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP.ContentFormats
{
	/// <summary>
	/// XML
	/// </summary>
	public class Xml : ICoapContentFormat
	{
		/// <summary>
		/// 41
		/// </summary>
		public const int ContentFormatCode = 41;

		/// <summary>
		/// XML
		/// </summary>
		public Xml()
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
			get { return "application/xml; charset=utf-8"; }
		}
	}
}
