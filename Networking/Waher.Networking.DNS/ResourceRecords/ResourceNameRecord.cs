using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// Abstract base class for resource records referring to a name.
	/// </summary>
	public class ResourceNameRecord : ResourceRecord	
	{
		private readonly string name;

		/// <summary>
		/// Abstract base class for resource records referring to a name.
		/// </summary>
		/// <param name="Name">Name being referred to.</param>
		public ResourceNameRecord(string Name)
		{
			this.name = Name;
		}

		/// <summary>
		/// Name being referred to.
		/// </summary>
		public string Name => this.name;
	}
}
