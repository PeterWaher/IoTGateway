using System;

namespace Waher.Networking.XMPP.Concentrator.Queries
{
	/// <summary>
	/// Represents an object item in the report.
	/// </summary>
	public class QueryObject : QueryItem
	{
		private readonly object obj;
		private readonly byte[] binary;
		private readonly string contentType;

		/// <summary>
		/// Represents an object item in the report.
		/// </summary>
		/// <param name="Parent">Parent item.</param>
		/// <param name="Object">Object value.</param>
		/// <param name="Binary">Binary representation of object.</param>
		/// <param name="ContentType">Content-Type</param>
		public QueryObject(QueryItem Parent, object Object, byte[] Binary, string ContentType)
			: base(Parent)
		{
			this.obj = Object;
			this.binary = Binary;
			this.contentType = ContentType;
		}

		/// <summary>
		/// Object
		/// </summary>
		public object Object => this.obj;

		/// <summary>
		/// Binary representation of object.
		/// </summary>
		public byte[] Binary => this.binary;

		/// <summary>
		/// Content-Type
		/// </summary>
		public string ContentType => this.contentType;
	}
}
