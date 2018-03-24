using System;
using System.Collections.Generic;
using System.Text;
using Waher.Things.Queries;

namespace Waher.Networking.XMPP.Concentrator.Queries
{
	/// <summary>
	/// Abstract base class for all query items.
	/// </summary>
    public abstract class QueryItem
    {
		private QueryItem parent;

		/// <summary>
		/// Abstract base class for all query items.
		/// </summary>
		/// <param name="Parent">Parent item.</param>
		public QueryItem(QueryItem Parent)
		{
			this.parent = Parent;
		}

		/// <summary>
		/// Parent item, if any, null otherwise.
		/// </summary>
		public QueryItem Parent => this.parent;
    }
}
