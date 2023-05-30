﻿using System;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.TurtleModel
{
	/// <summary>
	/// Represents a string literal.
	/// </summary>
	public class StringLiteral : SemanticLiteral
	{
		/// <summary>
		/// Represents a string literal.
		/// </summary>
		public StringLiteral()
			: base()
		{
		}

		/// <summary>
		/// Represents a string literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		public StringLiteral(string Value)
			: base(Value, Value)
		{
		}

		/// <summary>
		/// Represents a string literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		public StringLiteral(string Value, string StringValue)
			: base(Value, StringValue)
		{
		}

		/// <summary>
		/// Type name
		/// </summary>
		public override string StringType => string.Empty;

		/// <summary>
		/// How well the type supports a given data type.
		/// </summary>
		/// <param name="DataType">Data type.</param>
		/// <returns>Support grade.</returns>
		public override Grade Supports(string DataType)
		{
			return string.IsNullOrEmpty(DataType) || DataType == "http://www.w3.org/2001/XMLSchema#string" ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// Tries to parse a string value of the type supported by the class..
		/// </summary>
		/// <param name="Value">String value.</param>
		/// <param name="DataType">Data type.</param>
		/// <returns>Parsed literal.</returns>
		public override ISemanticLiteral Parse(string Value, string DataType)
		{
			return new StringLiteral(Value);
		}
	}
}
