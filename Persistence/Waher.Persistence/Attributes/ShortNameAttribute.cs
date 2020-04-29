using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Attributes
{
	/// <summary>
	/// This attribute defines a short name for a member (field or property). Short names are preferred for serialization.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class ShortNameAttribute : Attribute
	{
		private readonly string name;

		/// <summary>
		/// This attribute defines a short name for a member (field or property). Short names are preferred for serialization.
		/// </summary>
		/// <param name="Name">Short name.</param>
		public ShortNameAttribute(string Name)
			: base()
		{
			this.name = Name;
		}

		/// <summary>
		/// Short name.
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}
	}
}
