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

		/// <summary>
		/// <see cref="object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.name + "=" + this.value;
		}

		/// <summary>
		/// <see cref="Object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			return
				obj is Property P &&
				this.name == P.name &&
				this.value == P.value;
		}

		/// <summary>
		/// <see cref="object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			int Result = this.name.GetHashCode();
			Result ^= Result << 5 ^ this.value.GetHashCode();

			return Result;
		}

	}
}
