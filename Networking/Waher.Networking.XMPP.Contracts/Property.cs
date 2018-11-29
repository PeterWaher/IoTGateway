using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Named property
	/// </summary>
	public class Property
	{
		private string name;
		private string value;

		/// <summary>
		/// Named property
		/// </summary>
		/// <param name="Name">Name of property</param>
		/// <param name="Value">Property value</param>
		public Property(string Name, string Value)
		{
			this.name = Name;
			this.value = Value;
		}

		/// <summary>
		/// Name of property
		/// </summary>
		public string Name
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Property value
		/// </summary>
		public string Value
		{
			get => this.value;
			set => this.value = value;
		}
	}
}
