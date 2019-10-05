using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents one import instruction.
	/// </summary>
	public class Asn1Import
	{
		private readonly string identifier;
		private readonly string module;

		/// <summary>
		/// Represents one import instruction.
		/// </summary>
		/// <param name="Identifier">Identifier to import.</param>
		/// <param name="Module">Optional module reference.</param>
		public Asn1Import(string Identifier, string Module)
		{
			this.identifier = Identifier;
			this.module = Module;
		}

		/// <summary>
		/// Identifier to import.
		/// </summary>
		public string Identifier => this.identifier;

		/// <summary>
		/// Optional module reference.
		/// </summary>
		public string Module => this.module;
	}
}
