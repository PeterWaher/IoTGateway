using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP.ContentFormats
{
	/// <summary>
	/// Binary
	/// </summary>
	public class Binary : ICoapContentFormat
	{
		/// <summary>
		/// Binary
		/// </summary>
		public Binary()
		{
		}

		/// <summary>
		/// Content format number
		/// </summary>
		public int ContentFormat
		{
			get { return 42; }
		}

		/// <summary>
		/// Internet content type.
		/// </summary>
		public string ContentType
		{
			get { return "application/octet-stream"; }
		}
	}
}
