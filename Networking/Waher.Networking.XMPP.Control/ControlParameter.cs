using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Networking.XMPP.Control
{
	/// <summary>
	/// Abstract base class for control parameters.
	/// </summary>
	public abstract class ControlParameter
	{
		private string name;

		/// <summary>
		/// Abstract base class for control parameters.
		/// </summary>
		/// <param name="Name">Parameter name.</param>
		public ControlParameter(string Name)
		{
			this.name = Name;
		}

		/// <summary>
		/// Parameter Name
		/// </summary>
		public string Name
		{
			get { return this.name; }
		}
	}
}
