using System;
using System.Collections.Generic;
using System.Text;

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
		public NULL(byte[] Data)
			: base()
		{
			this.data = Data;
		}

		/// <summary>
		/// Binary data
		/// </summary>
		public byte[] Data => this.data;
	}
}
