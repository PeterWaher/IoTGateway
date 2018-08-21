using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Waher.Things.Queries;

namespace Waher.Networking.XMPP.Concentrator.Queries
{
	/// <summary>
	/// Represents an object item in the report.
	/// </summary>
	public class QueryObject : QueryItem
	{
		private readonly object obj;

		/// <summary>
		/// Represents an object item in the report.
		/// </summary>
		/// <param name="Parent">Parent item.</param>
		/// <param name="Object">Object value.</param>
		public QueryObject(QueryItem Parent, object Object)
			: base(Parent)
		{
			this.obj = Object;
		}

		/// <summary>
		/// Object
		/// </summary>
		public object Object => this.obj;
	}
}
