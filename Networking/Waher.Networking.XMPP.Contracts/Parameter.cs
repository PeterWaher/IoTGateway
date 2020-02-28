using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.XMPP.Contracts.HumanReadable;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Abstract base class for contractual parameters
	/// </summary>
	public abstract class Parameter : LocalizableDescription
	{
		private string name;

		/// <summary>
		/// Parameter name
		/// </summary>
		public string Name
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Parameter value.
		/// </summary>
		public abstract object ObjectValue
		{
			get;
		}

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		public abstract void Serialize(StringBuilder Xml);
	}
}
