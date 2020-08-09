using System;
using System.Xml;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Layout.Layout2D.Model.Attributes
{
	/// <summary>
	/// Expression attribute
	/// </summary>
	public class ExpressionAttribute : Attribute<Expression>
	{
		/// <summary>
		/// Expression attribute
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		public ExpressionAttribute(XmlElement E, string AttributeName)
			: base(E, AttributeName, false)
		{
		}

		/// <summary>
		/// Expression attribute
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Expression">Expression.</param>
		public ExpressionAttribute(string AttributeName, Expression Expression)
			: base(AttributeName, Expression)
		{
		}

		/// <summary>
		/// Tries to parse a string value
		/// </summary>
		/// <param name="StringValue">String value for attribute.</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If the value could be parsed.</returns>
		public override bool TryParse(string StringValue, out Expression Value)
		{
			try
			{
				Value = new Expression(StringValue);
				return true;
			}
			catch (Exception)
			{
				Value = null;
				return false;
			}
		}

		/// <summary>
		/// Converts a value to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation.</returns>
		public override string ToString(Expression Value)
		{
			return Value.Script;
		}

		/// <summary>
		/// Evaluates the expression defined by the attribute.
		/// </summary>
		/// <param name="Session">Session variables.</param>
		/// <returns>
		/// Result of the expression, 
		/// or <see cref="Exception"/> object if expression throws an exception, 
		/// or null if no expression is defined.
		/// </returns>
		public object Evaluate(Variables Session)
		{
			if (!this.HasPresetValue)
				return null;

			try
			{
				return this.PresetValue?.Evaluate(Session);
			}
			catch (Exception ex)
			{
				return ex;
			}
		}

		/// <summary>
		/// Evaluates the expression defined by the attribute.
		/// </summary>
		/// <param name="Session">Session variables.</param>
		/// <returns>
		/// Result of the expression, 
		/// or <see cref="Exception"/> object if expression throws an exception, 
		/// or null if no expression is defined.
		/// </returns>
		public IElement EvaluateElement(Variables Session)
		{
			if (!this.HasPresetValue)
				return null;

			IElement Result;

			try
			{
				Result = this.PresetValue?.Root?.Evaluate(Session);
			}
			catch (ScriptReturnValueException ex)
			{
				Result = ex.ReturnValue;
			}
			catch (Exception ex)
			{
				Result = new ObjectValue(ex);
			}

			return Result;
		}

		/// <summary>
		/// Copies the attribute object if undefined, or defined by an expression.
		/// Returns a reference to itself, if preset (set by a constant value).
		/// </summary>
		/// <returns>Attribute reference.</returns>
		public ExpressionAttribute CopyIfNotPreset()
		{
			return this;
		}

	}
}
