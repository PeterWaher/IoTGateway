using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.Concentrator.Attributes
{
	/// <summary>
	/// Defines a required parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class RequiredAttribute : Attribute
	{
		/// <summary>
		/// Defines a required parameter.
		/// </summary>
		public RequiredAttribute()
		{
		}
	}
}
