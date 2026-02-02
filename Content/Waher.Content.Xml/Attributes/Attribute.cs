using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Script;

namespace Waher.Content.Xml.Attributes
{
	/// <summary>
	/// Manages an attribute value or expression.
	/// </summary>
	/// <typeparam name="T">Type of attribute</typeparam>
	public abstract class Attribute<T>
	{
		private T evaluatedValue = default;
		private bool hasEvaluated = false;
		private bool hasEvaluatedValue = false;
		private readonly T presetValue;
		private readonly bool hasPresetValue;
		private readonly Expression expression;
		private readonly string name;
		private readonly bool isEmpty;

		/// <summary>
		/// Manages an attribute value or expression.
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Value">Attribute value.</param>
		public Attribute(string AttributeName, T Value)
		{
			this.name = AttributeName;
			this.presetValue = Value;
			this.hasPresetValue = true;
			this.expression = null;
			this.isEmpty = false;
		}

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

				this.isEmpty = c == 0;

				if (this.TryParse(Value, out T ParsedValue))
				{
					this.presetValue = ParsedValue;
					this.hasPresetValue = true;
					this.expression = null;
				}
				else
					throw new XmlException("Invalid value of attribute " + this.name + ": " + Value);
			}
			else
			{
				this.presetValue = default;
				this.expression = null;
				this.hasPresetValue = false;
				this.isEmpty = true;
			}
		}

		/// <summary>
		/// Defines an undefined attribute.
		/// </summary>
		/// <param name="AttributeName">Attribute name.</param>
		/// <param name="Expression">Expression.</param>
		protected Attribute(string AttributeName, Expression Expression)
		{
			this.name = AttributeName;
			this.presetValue = default;
			this.hasPresetValue = false;
			this.expression = Expression;
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
		/// If the attribute is defined by an expression.
		/// </summary>
		public bool HasExpression => !(this.expression is null);

		/// <summary>
		/// If attribute is empty.
		/// </summary>
		public bool IsEmpty => this.isEmpty;

		/// <summary>
		/// If the attribute is undefined.
		/// </summary>
		public bool Undefined => !this.hasPresetValue && this.expression is null;

		/// <summary>
		/// If the attribute is defined.
		/// </summary>
		public bool Defined => this.hasPresetValue || !(this.expression is null);

		/// <summary>
		/// Tries to parse a string value
		/// </summary>
		/// <param name="StringValue">String value for attribute.</param>
		/// <param name="Value">Parsed value, if successful.</param>
		/// <returns>If the value could be parsed.</returns>
		public abstract bool TryParse(string StringValue, out T Value);

		/// <summary>
		/// Tries to convert script result to a value of type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="Result">Script result.</param>
		/// <param name="Value">Converted value.</param>
		/// <returns>If conversion was possible.</returns>
		public virtual bool TryConvert(object Result, out T Value)
		{
			if (Result is string s && this.TryParse(s, out Value))
				return true;
			else
			{
				Value = default;
				return false;
			}
		}

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

		/// <summary>
		/// Exports the state of the attribute.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public void ExportState(XmlWriter Output)
		{
			if (this.hasPresetValue)
				Output.WriteAttributeString(this.name, this.ToString(this.presetValue));
			else if (this.hasEvaluatedValue)
				Output.WriteAttributeString(this.name, this.ToString(this.evaluatedValue));
		}

		/// <summary>
		/// Evaluate the attribute value.
		/// </summary>
		/// <param name="Session">Current session.</param>
		/// <param name="DefaultValue">Default value if value or expression is missing or invalid.</param>
		/// <returns>Evaluation result, or default value if not possible to evaluate.</returns>
		public async Task<T> EvaluateAsync(Variables Session, T DefaultValue)
		{
			if (this.hasPresetValue)
				return this.presetValue;
			else if (this.hasEvaluatedValue)
				return this.evaluatedValue;
			else if (this.hasEvaluated)
				return DefaultValue;
			else if (this.expression is null)
				return DefaultValue;
			else 
			{
				try
				{
					object Value = await this.expression.EvaluateAsync(Session);
					if (Value is T Eval || this.TryConvert(Value, out Eval))
					{
						this.evaluatedValue = Eval;
						this.hasEvaluated = true;
						this.hasEvaluatedValue = true;

						return Eval;
					}
					else
					{
						this.hasEvaluated = true;
						return DefaultValue;
					}
				}
				catch (Exception)
				{
					this.hasEvaluated = true;
					return DefaultValue;
				}
			}
		}

		/// <summary>
		/// Tries to evaluate the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <returns>Evaluation result.</returns>
		public static async Task<EvaluationResult<T>> TryEvaluate(Attribute<T> Attribute, Variables Session)
		{
			if (Attribute is null)
				return EvaluationResult<T>.Empty;
			else if (Attribute.hasPresetValue)
				return new EvaluationResult<T>(Attribute.presetValue);
			else if (Attribute.hasEvaluatedValue)
				return new EvaluationResult<T>(Attribute.evaluatedValue);
			else if (Attribute.hasEvaluated)
				return EvaluationResult<T>.Empty;
			else if (!(Attribute.expression is null))
			{
				try
				{
					object Value = await Attribute.expression.EvaluateAsync(Session);
					if (Value is T Eval || Attribute.TryConvert(Value, out Eval))
					{
						Attribute.evaluatedValue = Eval;
						Attribute.hasEvaluated = true;
						Attribute.hasEvaluatedValue = true;

						return new EvaluationResult<T>(Eval);
					}
					else
					{
						Attribute.hasEvaluated = true;
						return EvaluationResult<T>.Empty;
					}
				}
				catch (Exception)
				{
					Attribute.hasEvaluated = true;
					return EvaluationResult<T>.Empty;
				}
			}
			else
				return EvaluationResult<T>.Empty;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			if (this.hasPresetValue)
				return this.presetValue?.ToString();
			else if (this.hasEvaluatedValue)
				return this.evaluatedValue?.ToString();
			else if (this.hasEvaluated)
				return "<Evaluation error>";
			else if (!(this.expression is null))
				return this.expression.Script;
			else
				return "<Not set>";
		}
	}
}
