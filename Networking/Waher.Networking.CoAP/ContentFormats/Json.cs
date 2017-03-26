using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.CoAP.ContentFormats
{
	/// <summary>
	/// JSON
	/// </summary>
	public class Json : ICoapContentFormat
	{
		/// <summary>
		/// JSON
		/// </summary>
		public Json()
		{
		}

		/// <summary>
		/// Content format number
		/// </summary>
		public int ContentFormat
		{
			get { return 50; }
		}

		/// <summary>
		/// Internet content type.
		/// </summary>
		public string ContentType
		{
			get { return "application/json"; }
		}
	}
}
