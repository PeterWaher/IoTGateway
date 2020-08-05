using System;
using System.Xml;
using Waher.Script;
using Waher.Layout.Layout2D.Exceptions;

namespace Waher.Layout.Layout2D.Model.Attributes
{
	/// <summary>
	/// Manages an attribute value or expression.
	/// </summary>
	/// <typeparam name="T">Type of attribute</typeparam>
	public abstract class Attribute<T>
	{
		private readonly T presetValue;
		private readonly bool hasPresetValue;
		private readonly Expression expression;
		private readonly string name;

		/// <summary>
		/// Manages an attribute value or expression.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="CanEmbedScript">If script can be embedded.</param>
		public Attribute(XmlElement E, string AttributeName, bool CanEmbedScript)
		{
			this.name = AttributeName;

			if (E.HasAttribute(this.name))
			{
				string Value = E.GetAttribute(this.name);

				int c = Value.Length;

				if (CanEmbedScript && c >= 2 && Value[0] == '{' && Value[c - 1] == '}')
				{
					try
					{
						Expression Exp = new Expression(Value.Substring(1, c - 2));
						
						this.presetValue = default;
						this.hasPresetValue = false;
						this.expression = Exp;

						return;
					}
					catch (Exception)
					{
						// Ignore
					}
				}

				if (this.TryParse(Value, out T ParsedValue))
				{
					this.presetValue = ParsedValue;
					this.hasPresetValue = true;
					this.expression = null;
				}
				else
					throw new LayoutSyntaxException("Invalid value of attribute " + this.name + ": " + Value);
			}
			else
			{
				this.presetValue = default;
				this.expression = null;
				this.hasPresetValue = false;
			}
		}

		/// <summary>
		/// Attribute name
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Preset Value of attribute.
		/// </summary>
		public T PresetValue => this.presetValue;

		/// <summary>
		/// Expression returning the value of the attribute.
		/// </summary>
		public Expression Expression => this.expression;

		/// <summary>
		/// If the attribute has a preset value.
		/// </summary>
		public bool HasPresetValue => this.hasPresetValue;

		/// <summary>
		/// Tries to parse a string value
		/// </summary>
		/// <param name="StringValue">String value for attribute.</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If the value could be parsed.</returns>
		public abstract bool TryParse(string StringValue, out T Value);

		/// <summary>
		/// Converts a value to a string.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <returns>String representation.</returns>
		public abstract string ToString(T Value);

		/// <summary>
		/// Exports the attribute.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public void Export(XmlWriter Output)
		{
			if (this.hasPresetValue)
				Output.WriteAttributeString(this.name, this.ToString(this.presetValue));
			else if (!(this.expression is null))
				Output.WriteAttributeString(this.name, "{" + this.expression.Script + "}");
		}

	}
}
