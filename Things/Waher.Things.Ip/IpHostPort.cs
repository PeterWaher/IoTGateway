using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;

namespace Waher.Things.IP
{
	public abstract class IpHostPort : IpHost
	{
		private int port = 0;

		/// <summary>
		/// Port number.
		/// </summary>
		[Page(1, "IP", 0)]
		[Header(4, "Port Number:", 60)]
		[ToolTip(5, "Port number to use when communicating with device.")]
		[DefaultValue(0)]
		[Range(0, 65535)]
		public int Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

	}
}
