using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Abstract base class for resource records referring to a name.
	/// </summary>
	public class ResourceNameRecord : ResourceRecord
	{
		private readonly string name2;

		/// <summary>
		/// Abstract base class for resource records referring to a name.
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Name2">Name being referred to.</param>
		public ResourceNameRecord(string Name, TYPE Type, CLASS Class, uint Ttl, 
			string Name2)
			: base(Name, Type, Class, Ttl)
		{
			this.name2 = Name2;
		}

		/// <summary>
		/// Name being referred to.
		/// </summary>
		public string Name2 => this.name2;

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return base.ToString() + "\t" + this.name2;
		}
	}
}
