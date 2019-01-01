using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.DNS.Enumerations;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// NULL (Experimental)
	/// </summary>
	public class NULL : ResourceRecord
	{
		private readonly byte[] data;

		/// <summary>
		/// NULL (Experimental)
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">Undefined data.</param>
		public NULL(string Name, TYPE Type, CLASS Class, uint Ttl, byte[] Data)
			: base(Name, Type, Class, Ttl)
		{
			this.data = Data;
		}

		/// <summary>
		/// Binary data
		/// </summary>
		public byte[] Data => this.data;
	}
}
