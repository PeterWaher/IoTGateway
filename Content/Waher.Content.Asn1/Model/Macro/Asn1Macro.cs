using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1.Model.Macro
{
	/// <summary>
	/// ASN.1 Macro
	/// </summary>
	public class Asn1Macro : Asn1Node
	{
		private readonly UserDefinedItem typeNotation;
		private readonly UserDefinedItem valueNotation;
		private readonly SupportingSyntax[] supportingSyntax;

		/// <summary>
		/// ASN.1 Macro
		/// </summary>
		/// <param name="TypeNotation">Type Notation</param>
		/// <param name="ValueNotation">Value Notation</param>
		/// <param name="SupportingSyntax">Supporting Syntax</param>
		public Asn1Macro(UserDefinedItem TypeNotation, UserDefinedItem ValueNotation,
			SupportingSyntax[] SupportingSyntax)
		{
			this.typeNotation = TypeNotation;
			this.valueNotation = ValueNotation;
			this.supportingSyntax = SupportingSyntax;
		}

		/// <summary>
		/// Type notation
		/// </summary>
		public UserDefinedItem TypeNotation => this.typeNotation;

		/// <summary>
		/// Value notation
		/// </summary>
		public UserDefinedItem ValueNotation => this.valueNotation;

		/// <summary>
		/// Supporting syntax
		/// </summary>
		public SupportingSyntax[] SupportingSyntax => this.supportingSyntax;

	}
}
