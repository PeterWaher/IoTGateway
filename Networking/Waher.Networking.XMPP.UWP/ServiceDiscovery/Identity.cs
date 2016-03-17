using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;

namespace Waher.Networking.XMPP.ServiceDiscovery
{
	/// <summary>
	/// Contains information about an identity of an entity.
	/// </summary>
	public class Identity
	{
		private string category;
		private string type;
		private string name;

		internal Identity(XmlElement E)
		{
			this.category = XML.Attribute(E, "category");
			this.type = XML.Attribute(E, "type");
			this.name = XML.Attribute(E, "name");
		}

		/// <summary>
		/// Contains information about an identity of an entity.
		/// </summary>
		/// <param name="Category">Category</param>
		/// <param name="Type">Type</param>
		/// <param name="Name">Name</param>
		public Identity(string Category, string Type, string Name)
		{
			this.category = Category;
			this.type = Type;
			this.name = Name;
		}

		/// <summary>
		/// Category
		/// </summary>
		public string Category { get { return this.category; } }

		/// <summary>
		/// Type
		/// </summary>
		public string Type { get { return this.type; } }

		/// <summary>
		/// Name
		/// </summary>
		public string Name { get { return this.name; } }

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.category);
			sb.Append(", ");
			sb.Append(this.type);
			sb.Append(", ");
			sb.Append(this.name);

			return sb.ToString();
		}
	}
}
