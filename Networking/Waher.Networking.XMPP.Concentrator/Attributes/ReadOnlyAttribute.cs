using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator.Attributes
{
	/// <summary>
	/// Defines a read-only parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class ReadOnlyAttribute : Attribute
	{
		/// <summary>
		/// Defines a read-only parameter.
		/// </summary>
		public ReadOnlyAttribute()
		{
		}
	}
}
