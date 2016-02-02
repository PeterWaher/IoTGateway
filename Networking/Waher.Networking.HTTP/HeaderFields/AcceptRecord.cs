using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.HTTP
{
	/// <summary>
	/// Corresponds to an item in the Accept, Accept-Charset, Accept-Encoding and Accept-Language header fields.
	/// </summary>
	public class AcceptRecord
	{
		private string item = null;
		private double q = 1;
		private KeyValuePair<string, string>[] parameters = null;
		private int order = 0;

		/// <summary>
		/// Corresponds to an item in the Accept, Accept-Charset, Accept-Encoding and Accept-Language header fields.
		/// </summary>
		internal AcceptRecord()
		{
		}

		/// <summary>
		/// Item name.
		/// </summary>
		public string Item
		{
			get { return this.item; }
			internal set { this.item = value; }
		}

		/// <summary>
		/// Quality property, between 0 and 1.
		/// </summary>
		public double Quality
		{
			get { return this.q; }
			internal set { this.q = value; }
		}

		/// <summary>
		/// Any additional parameters avalable. If no parameters are available, null is returned.
		/// </summary>
		public KeyValuePair<string, string>[] Parameters
		{
			get { return this.parameters; }
			internal set { this.parameters = value; }
		}

		/// <summary>
		/// Order of record, as it appears in the header.
		/// </summary>
		public int Order
		{
			get { return this.order; }
			internal set { this.order = value; }
		}

		internal int Detail
		{
			get
			{
				if (this.parameters != null)
					return 3;

				if (!this.item.EndsWith("/*"))
					return 2;

				if (this.item == "*/*")
					return 0;
				else
					return 1;
			}
		}

	}
}
