using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP.LWM2M.ContentFormats
{
	/// <summary>
	/// OMA LWM2M JSON
	/// </summary>
	public class Json : ICoapContentFormat
	{
		/// <summary>
		/// 11543
		/// </summary>
		public const int ContentFormatCode = 11543;

		/// <summary>
		/// OMA LWM2M JSON
		/// </summary>
		public Json()
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
			get { return "application/vnd.oma.lwm2m+json"; }
		}
	}
}
