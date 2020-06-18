using System;
using System.Collections.Generic;

namespace Waher.Networking.XMPP.Concentrator.Queries
{
	/// <summary>
	/// Represents a section in the report.
	/// </summary>
	public class QuerySection : QueryItem
	{
		private readonly string header;
		private readonly List<QueryItem> content = new List<QueryItem>();
		private QueryItem[] contentFixed = null;

		/// <summary>
		/// Represents a section in the report.
		/// </summary>
		/// <param name="Parent">Parent item.</param>
		/// <param name="Header">Section header.</param>
		public QuerySection(QueryItem Parent, string Header)
			: base(Parent)
		{
			this.header = Header;
		}

		/// <summary>
		/// Section Header
		/// </summary>
		public string Header => this.header;

		/// <summary>
		/// Content of section
		/// </summary>
		public QueryItem[] Content
		{
			get
			{
				QueryItem[] Result = this.contentFixed;
				if (Result != null)
					return Result;

				lock (this.content)
				{
					Result = this.contentFixed = this.content.ToArray();
				}

				return Result;
			}
		}

		/// <summary>
		/// Adds an item to the section.
		/// </summary>
		/// <param name="Item">Item</param>
		public void Add(QueryItem Item)
		{
			lock (this.content)
			{
				this.content.Add(Item);
				this.contentFixed = null;
			}
		}
	}
}
