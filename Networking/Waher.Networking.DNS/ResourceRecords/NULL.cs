using System;
using System.Collections.Generic;
using System.IO;
using Waher.Networking.DNS.Enumerations;
using Waher.Persistence.Attributes;

namespace Waher.Networking.DNS.ResourceRecords
{
	/// <summary>
	/// NULL (Experimental)
	/// </summary>
	public class NULL : ResourceRecord
	{
		private byte[] data;

		/// <summary>
		/// NULL (Experimental)
		/// </summary>
		public NULL()
			: base()
		{
			this.data = null;
		}

		/// <summary>
		/// NULL (Experimental)
		/// </summary>
		/// <param name="Name">Name</param>
		/// <param name="Type">Resource Record Type</param>
		/// <param name="Class">Resource Record Class</param>
		/// <param name="Ttl">Time to live</param>
		/// <param name="Data">RR-specific binary data.</param>
		/// <param name="EndPos">End position of record.</param>
		public NULL(string Name, TYPE Type, CLASS Class, uint Ttl, Stream Data, long EndPos)
			: base(Name, Type, Class, Ttl)
		{
			int c = (int)(EndPos - Data.Position);
			this.data = new byte[c];
			Data.Read(this.data, 0, c);
		}

		/// <summary>
		/// Binary data
		/// </summary>
		[DefaultValueNull]
		public byte[] Data
		{
			get => this.data;
			set => this.data = value;
		}
	}
}
