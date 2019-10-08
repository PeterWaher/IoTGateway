using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// TAG class
	/// </summary>
	public enum TagClass : byte
	{
		/// <summary>
		/// Universal
		/// </summary>
		Universal = 0,

		/// <summary>
		/// Application
		/// </summary>
		Application = 1,

		/// <summary>
		/// Context-specific
		/// </summary>
		ContextSpecific = 2,

		/// <summary>
		/// Private
		/// </summary>
		Private = 3
	}
}
