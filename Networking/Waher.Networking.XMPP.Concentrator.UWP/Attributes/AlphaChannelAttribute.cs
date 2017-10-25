using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator.Attributes
{
	/// <summary>
	/// Includes the alpha channel of a color property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class AlphaChannelAttribute : Attribute
	{
		/// <summary>
		/// Includes the alpha channel of a color property.
		/// </summary>
		public AlphaChannelAttribute()
		{
		}
	}
}
