using System;
using System.Text;
using System.Text.RegularExpressions;
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
			get => this.value;
			set
			{
				this.value = value;
				this.match = null;
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
		public override object ObjectValue => this.value;

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<stringParameter name=\"");
			Xml.Append(XML.Encode(this.Name));
			Xml.Append("\" value=\"");
			Xml.Append(XML.Encode(this.value));

			if (!string.IsNullOrEmpty(this.Guide))
			{
				Xml.Append("\" guide=\"");
				Xml.Append(XML.Encode(this.Guide.Normalize(NormalizationForm.FormC)));
			}

			if (!string.IsNullOrEmpty(this.Expression))
			{
				Xml.Append("\" exp=\"");
				Xml.Append(XML.Encode(this.Expression.Normalize(NormalizationForm.FormC)));
			}

			if (!string.IsNullOrEmpty(this.regEx))
			{
				Xml.Append("\" regEx=\"");
				Xml.Append(XML.Encode(this.regEx.Normalize(NormalizationForm.FormC)));
			}

			if (!(this.min is null))
			{
				Xml.Append("\" min=\"");
				Xml.Append(XML.Encode(this.min));
				Xml.Append("\" minIncluded=\"");
				Xml.Append(XML.Encode(CommonTypes.Encode(this.minIncluded)));
			}

			if (!(this.max is null))
			{
				Xml.Append("\" max=\"");
				Xml.Append(XML.Encode(this.max));
				Xml.Append("\" maxIncluded=\"");
				Xml.Append(XML.Encode(CommonTypes.Encode(this.maxIncluded)));
			}

			if (this.minLength.HasValue)
			{
				Xml.Append("\" minLength=\"");
				Xml.Append(this.minLength.Value.ToString());
			}

			if (this.maxLength.HasValue)
			{
				Xml.Append("\" maxLength=\"");
				Xml.Append(this.maxLength.Value.ToString());
			}

			if (this.Descriptions is null || this.Descriptions.Length == 0)
				Xml.Append("\"/>");
			else
			{
				Xml.Append("\">");

				foreach (HumanReadableText Description in this.Descriptions)
					Description.Serialize(Xml, "description", false);

				Xml.Append("</stringParameter>");
			}
		}

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <returns>If parameter value is valid.</returns>
		public override bool IsParameterValid(Variables Variables)
		{
			int i;

			if (!(this.min is null))
			{
				i = string.Compare(this.value, this.min);

				if (i < 0 || (i == 0 && !this.minIncluded))
					return false;
			}

			if (!(this.max is null))
			{
				i = string.Compare(this.value, this.max);

				if (i > 0 || (i == 0 && !this.maxIncluded))
					return false;
			}

			if (this.minLength.HasValue && this.value.Length < this.minLength.Value)
				return false;

			if (this.maxLength.HasValue && this.value.Length > this.maxLength.Value)
				return false;

			if (!string.IsNullOrEmpty(this.regEx))
			{
				try
				{
					if (this.parsed is null)
						this.parsed = new Regex(this.regEx, RegexOptions.Singleline);

					if (this.match is null)
						this.match = this.parsed.Match(this.value);

					if (!this.match.Success || this.match.Index > 0 || this.match.Length < this.value.Length)
						return false;
				}
				catch (Exception)
				{
					return false;
				}
			}

			return base.IsParameterValid(Variables);
		}

		/// <summary>
		/// Populates a variable collection with the value of the parameter.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public override void Populate(Variables Variables)
		{
			Variables[this.Name] = this.value;

			if (!string.IsNullOrEmpty(this.regEx))
			{
				try
				{
					if (this.parsed is null)
						this.parsed = new Regex(this.regEx, RegexOptions.Singleline);

					if (this.match is null)
						this.match = this.parsed.Match(this.value);

					if (this.match.Success && this.match.Index == 0 && this.match.Length == this.value.Length)
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

	}
}
