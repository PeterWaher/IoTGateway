using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Script;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// String-valued contractual parameter
	/// </summary>
	public class StringParameter : Parameter
	{
		private string value;
		private string regEx;
		private string min = null;
		private string max = null;
		private bool minIncluded = true;
		private bool maxIncluded = true;
		private int? minLength = null;
		private int? maxLength = null;
		private Regex parsed = null;
		private Match match = null;

		/// <summary>
		/// Parameter value
		/// </summary>
		public string Value
		{
			get => this.@value;
			set
			{
				this.@value = value;
				this.match = null;
				this.ProtectedValue = null;
			}
		}

		/// <summary>
		/// Optional regular expression to validate the value of the string parameter.
		/// </summary>
		public string RegEx
		{
			get => this.regEx;
			set
			{
				this.regEx = value;
				this.parsed = null;
				this.match = null;
			}
		}

		/// <summary>
		/// Optional minimum value.
		/// </summary>
		public string Min
		{
			get => this.min;
			set => this.min = value;
		}

		/// <summary>
		/// Optional maximum value.
		/// </summary>
		public string Max
		{
			get => this.max;
			set => this.max = value;
		}

		/// <summary>
		/// If the optional minimum value is included in the allowed range.
		/// </summary>
		public bool MinIncluded
		{
			get => this.minIncluded;
			set => this.minIncluded = value;
		}

		/// <summary>
		/// If the optional maximum value is included in the allowed range.
		/// </summary>
		public bool MaxIncluded
		{
			get => this.maxIncluded;
			set => this.maxIncluded = value;
		}

		/// <summary>
		/// Optional minimum length of value.
		/// </summary>
		public int? MinLength
		{
			get => this.minLength;
			set => this.minLength = value;
		}

		/// <summary>
		/// Optional maximum length of value.
		/// </summary>
		public int? MaxLength
		{
			get => this.maxLength;
			set => this.maxLength = value;
		}

		/// <summary>
		/// Parameter value.
		/// </summary>
		public override object ObjectValue => this.@value;

		/// <summary>
		/// String representation of value.
		/// </summary>
		public override string StringValue
		{
			get => this.Value ?? string.Empty;
			set => this.Value = value;
		}

		/// <summary>
		/// Parameter type name, corresponding to the local name of the parameter element in XML.
		/// </summary>
		public override string ParameterType => "stringParameter";

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="UsingTemplate">If the XML is for creating a contract using a template.</param>
		public override void Serialize(StringBuilder Xml, bool UsingTemplate)
		{
			Xml.Append("<stringParameter");

			if (!UsingTemplate)
			{
				if (!string.IsNullOrEmpty(this.Expression))
				{
					Xml.Append(" exp=\"");
					Xml.Append(XML.Encode(this.Expression.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}

				if (!string.IsNullOrEmpty(this.Guide))
				{
					Xml.Append(" guide=\"");
					Xml.Append(XML.Encode(this.Guide.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}

				if (!(this.max is null))
				{
					Xml.Append(" max=\"");
					Xml.Append(XML.Encode(this.max));
					Xml.Append("\" maxIncluded=\"");
					Xml.Append(XML.Encode(CommonTypes.Encode(this.maxIncluded)));
					Xml.Append('"');
				}

				if (this.maxLength.HasValue)
				{
					Xml.Append(" maxLength=\"");
					Xml.Append(this.maxLength.Value.ToString());
					Xml.Append('"');
				}

				if (!(this.min is null))
				{
					Xml.Append(" min=\"");
					Xml.Append(XML.Encode(this.min));
					Xml.Append("\" minIncluded=\"");
					Xml.Append(XML.Encode(CommonTypes.Encode(this.minIncluded)));
					Xml.Append('"');
				}

				if (this.minLength.HasValue)
				{
					Xml.Append(" minLength=\"");
					Xml.Append(this.minLength.Value.ToString());
					Xml.Append('"');
				}
			}

			Xml.Append(" name=\"");
			Xml.Append(XML.Encode(this.Name));
			Xml.Append('"');

			if (this.CanSerializeProtectedValue)
			{
				Xml.Append(" protected=\"");
				Xml.Append(Convert.ToBase64String(this.ProtectedValue));
				Xml.Append('"');
			}

			if (!UsingTemplate)
			{
				if (this.Protection != ProtectionLevel.Normal)
				{
					Xml.Append(" protection=\"");
					Xml.Append(this.Protection.ToString());
					Xml.Append('"');
				}

				if (!string.IsNullOrEmpty(this.regEx))
				{
					Xml.Append(" regEx=\"");
					Xml.Append(XML.Encode(this.regEx.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}
			}

			if (!(this.@value is null) && this.CanSerializeValue)
			{
				Xml.Append(" value=\"");
				Xml.Append(XML.Encode(this.@value));
				Xml.Append('"');
			}

			if (UsingTemplate || this.Descriptions is null || this.Descriptions.Length == 0)
				Xml.Append("/>");
			else
			{
				Xml.Append('>');

				foreach (HumanReadableText Description in this.Descriptions)
					Description.Serialize(Xml, "description", null);

				Xml.Append("</stringParameter>");
			}
		}

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <param name="Client">Connected contracts client. If offline or null, partial validation in certain cases will be performed.</param>
		/// <returns>If parameter value is valid.</returns>
		public override Task<bool> IsParameterValid(Variables Variables, ContractsClient Client)
		{
			int i;

			if (this.@value is null)
			{
				this.ErrorReason = ParameterErrorReason.LacksValue;
				this.ErrorText = null;

				return Task.FromResult(false);
			}

			if (!(this.min is null))
			{
				i = string.Compare(this.@value, this.min);

				if (i < 0 || (i == 0 && !this.minIncluded))
				{
					this.ErrorReason = ParameterErrorReason.BelowMin;
					this.ErrorText = null;

					return Task.FromResult(false);
				}
			}

			if (!(this.max is null))
			{
				i = string.Compare(this.@value, this.max);

				if (i > 0 || (i == 0 && !this.maxIncluded))
				{
					this.ErrorReason = ParameterErrorReason.AboveMax;
					this.ErrorText = null;

					return Task.FromResult(false);
				}
			}

			if (this.minLength.HasValue && this.@value.Length < this.minLength.Value)
			{
				this.ErrorReason = ParameterErrorReason.TooShort;
				this.ErrorText = null;

				return Task.FromResult(false);
			}

			if (this.maxLength.HasValue && this.@value.Length > this.maxLength.Value)
			{
				this.ErrorReason = ParameterErrorReason.TooLong;
				this.ErrorText = null;

				return Task.FromResult(false);
			}

			if (!string.IsNullOrEmpty(this.regEx))
			{
				try
				{
					this.parsed ??= new Regex(this.regEx, RegexOptions.Singleline);
					this.match ??= this.parsed.Match(this.@value);

					if (!this.match.Success || this.match.Index > 0 || this.match.Length < this.@value.Length)
					{
						this.ErrorReason = ParameterErrorReason.RegularExpressionRejection;
						this.ErrorText = null;

						return Task.FromResult(false);
					}
				}
				catch (Exception)
				{
					// Ignore. Leniant expression handling: Servers handle implementation-specific expression syntaxes.
				}
			}

			return base.IsParameterValid(Variables, Client);
		}

		/// <summary>
		/// Populates a variable collection with the value of the parameter.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public override void Populate(Variables Variables)
		{
			Variables[this.Name] = this.@value;

			if (!string.IsNullOrEmpty(this.regEx) && !(this.@value is null))
			{
				try
				{
					this.parsed ??= new Regex(this.regEx, RegexOptions.Singleline);
					this.match ??= this.parsed.Match(this.@value);

					if (this.match.Success && this.match.Index == 0 && this.match.Length == this.@value.Length)
					{
						foreach (string Name in this.parsed.GetGroupNames())
						{
							if (!int.TryParse(Name, out int _))
							{
								Group G = this.match.Groups[Name];
								if (G.Success)
								{
									string Value = G.Value;

									if (Script.Expression.TryParse(Value, out double d))
										Variables[Name] = d;
									else
										Variables[Name] = Value;

									Variables[Name + "_STR"] = Value;
									Variables[Name + "_POS"] = G.Index;
									Variables[Name + "_LEN"] = G.Length;
								}
							}
						}
					}
				}
				catch (Exception)
				{
					// Ignore
				}
			}
		}

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public override void SetValue(object Value)
		{
			this.Value = Value?.ToString();
		}

		/// <summary>
		/// Sets the minimum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Minimum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMinValue(object Value, bool? Inclusive)
		{
			this.Min = Value.ToString();

			if (Inclusive.HasValue)
				this.MinIncluded = Inclusive.Value;
		}

		/// <summary>
		/// Sets the maximum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Maximum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMaxValue(object Value, bool? Inclusive)
		{
			this.Max = Value.ToString();

			if (Inclusive.HasValue)
				this.MaxIncluded = Inclusive.Value;
		}

		/// <summary>
		/// Imports parameter values from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>If import was successful.</returns>
		public override Task<bool> Import(XmlElement Xml)
		{
			this.@value = Xml.HasAttribute("value") ? XML.Attribute(Xml, "value") : null;
			this.regEx = XML.Attribute(Xml, "regEx");
			this.min = Xml.HasAttribute("min") ? XML.Attribute(Xml, "min") : null;
			this.minIncluded = XML.Attribute(Xml, "minIncluded", true);
			this.max = Xml.HasAttribute("max") ? XML.Attribute(Xml, "max") : null;
			this.maxIncluded = XML.Attribute(Xml, "maxIncluded", true);
			this.minLength = Xml.HasAttribute("minLength") ? XML.Attribute(Xml, "minLength", 0) : (int?)null;
			this.maxLength = Xml.HasAttribute("maxLength") ? XML.Attribute(Xml, "maxLength", 0) : (int?)null;

			return base.Import(Xml);
		}

	}
}
