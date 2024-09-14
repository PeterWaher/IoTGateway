using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Persistence.Attributes;
using Waher.Script;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Abstract base class for contractual parameters
	/// </summary>
	public abstract class Parameter : LocalizableDescription
	{
		private string name;
		private string guide = string.Empty;
		private string exp = string.Empty;
		private byte[] protectedValue = null;
		private string errorText = null;
		private ParameterErrorReason? errorReason = null;
		private Expression parsed = null;
		private ProtectionLevel protection = ProtectionLevel.Normal;

		/// <summary>
		/// Parameter name
		/// </summary>
		public string Name
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Parameter guide text
		/// </summary>
		public string Guide
		{
			get => this.guide;
			set => this.guide = value;
		}

		/// <summary>
		/// Parameter validation script expression.
		/// </summary>
		public string Expression
		{
			get => this.exp;
			set
			{
				this.exp = value;
				this.parsed = null;
			}
		}

		/// <summary>
		/// Level of confidentiality of the information provided by the parameter.
		/// </summary>
		public ProtectionLevel Protection
		{
			get => this.protection;
			set => this.protection = value;
		}

		/// <summary>
		/// Protected value, in case <see cref="Protection"/> is not equal to <see cref="ProtectionLevel.Normal"/>.
		/// </summary>
		[DefaultValueNull]
		public byte[] ProtectedValue
		{
			get => this.protectedValue;
			set => this.protectedValue = value;
		}

		/// <summary>
		/// If the value can be serialized in the clear.
		/// </summary>
		public bool CanSerializeValue
		{
			get
			{
				return
					!(this.ObjectValue is null) &&
					(this.protection == ProtectionLevel.Normal || this.protection == ProtectionLevel.Obfuscated);
			}
		}

		/// <summary>
		/// If the protected value van be serialized.
		/// </summary>
		public bool CanSerializeProtectedValue
		{
			get
			{
				return
					!(this.protectedValue is null) &&
					(this.protection == ProtectionLevel.Encrypted || this.protection == ProtectionLevel.Transient);
			}
		}

		/// <summary>
		/// Parameter value.
		/// </summary>
		public abstract object ObjectValue { get; }

		/// <summary>
		/// String representation of value.
		/// </summary>
		public abstract string StringValue
		{
			get;
			set;
		}

		/// <summary>
		/// After <see cref="IsParameterValid(Variables)"/> or <see cref="IsParameterValid(Variables, ContractsClient)"/> has been
		/// execited, this property contains the reason why the validation failed.
		/// </summary>
		public ParameterErrorReason? ErrorReason
		{
			get => this.errorReason;
			protected set => this.errorReason = value;
		}

		/// <summary>
		/// After <see cref="IsParameterValid(Variables)"/> or <see cref="IsParameterValid(Variables, ContractsClient)"/> has been
		/// execited, this property may contain (depending on the value of <see cref="ErrorReason"/>) textual information related
		/// to why the validation failed.
		/// </summary>
		public string ErrorText
		{
			get => this.errorText;
			protected set => this.errorText = value;
		}

		/// <summary>
		/// Parameter type name, corresponding to the local name of the parameter element in XML.
		/// </summary>
		public abstract string ParameterType { get; }

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		[Obsolete("Use the Serialize(StringBuilder, bool) overload.")]
		public void Serialize(StringBuilder Xml)
		{
			this.Serialize(Xml, false);
		}

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="UsingTemplate">If the XML is for creating a contract using a template.</param>
		public abstract void Serialize(StringBuilder Xml, bool UsingTemplate);

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <returns>If parameter value is valid.</returns>
		[Obsolete("Use the IsParameterValid(Variables, ContractsClient) overload for full validation.")]
		public Task<bool> IsParameterValid(Variables Variables)
		{
			return this.IsParameterValid(Variables, null);
		}

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <param name="Client">Connected contracts client. If offline or null, partial validation in certain cases will be performed.</param>
		/// <returns>If parameter value is valid.</returns>
		public virtual async Task<bool> IsParameterValid(Variables Variables, ContractsClient Client)
		{
			this.errorReason = null;
			this.errorText = null;

			if (!string.IsNullOrEmpty(this.exp))
			{
				try
				{
					object Result = await this.Parsed.EvaluateAsync(Variables);

					if (Result is bool b && !b)
					{
						this.errorReason = ParameterErrorReason.ScriptExpressionRejection;
						return false;
					}
				}
				catch (Exception)
				{
					// Ignore. Leniant expression handling: Servers handle implementation-specific expression syntaxes.
				}
			}

			return true;
		}

		/// <summary>
		/// Parsed expression.
		/// </summary>
		public Expression Parsed
		{
			get
			{
				if (this.parsed is null)
					this.parsed = new Expression(this.exp);

				return this.parsed;
			}
		}

		/// <summary>
		/// Populates a variable collection with the value of the parameter.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public abstract void Populate(Variables Variables);

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public abstract void SetValue(object Value);

		/// <summary>
		/// Sets the minimum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Minimum value.</param>
		public void SetMinValue(object Value)
		{
			this.SetMinValue(Value, null);
		}

		/// <summary>
		/// Sets the minimum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Minimum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public abstract void SetMinValue(object Value, bool? Inclusive);

		/// <summary>
		/// Sets the maximum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Maximum value.</param>
		public void SetMaxValue(object Value)
		{
			this.SetMaxValue(Value, null);
		}

		/// <summary>
		/// Sets the maximum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Maximum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public abstract void SetMaxValue(object Value, bool? Inclusive);

		/// <summary>
		/// Imports parameter values from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>If import was successful.</returns>
		public virtual async Task<bool> Import(XmlElement Xml)
		{
			this.Name = XML.Attribute(Xml, "name");
			if (string.IsNullOrEmpty(this.Name))
				return false;

			this.guide = XML.Attribute(Xml, "guide");
			this.exp = XML.Attribute(Xml, "exp");
			this.protection = XML.Attribute(Xml, "protection", ProtectionLevel.Normal);
			this.parsed = null;

			if (Xml.HasAttribute("protected"))
			{
				try
				{
					this.protectedValue = Convert.FromBase64String(XML.Attribute(Xml, "protected"));
				}
				catch (Exception)
				{
					return false;
				}
			}
			else
				this.protectedValue = null;

			List<HumanReadableText> Descriptions = new List<HumanReadableText>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					if (E.NamespaceURI != Xml.NamespaceURI)
						return false;

					switch (E.LocalName)
					{
						case "description": // Smart contract
							HumanReadableText Text = HumanReadableText.Parse(E);
							if (Text is null || !(await Text.IsWellDefined() is null))
								return false;

							Descriptions.Add(Text);
							break;

						case "Description": // Simplified (ex. state machine note command).
							Text = HumanReadableText.ParseSimplified(E);
							if (Text is null || !(await Text.IsWellDefined() is null))
								return false;

							Descriptions.Add(Text);
							break;

						default:
							if (!this.IsChildLocalNameRecognized(E.LocalName))
								return false;
							break;
					}
				}
			}

			this.Descriptions = Descriptions.ToArray();

			return true;
		}

		/// <summary>
		/// If a local name of a child XML element is recognized.
		/// </summary>
		/// <param name="LocalName">Local name.</param>
		/// <returns>If recognized.</returns>
		protected virtual bool IsChildLocalNameRecognized(string LocalName)
		{
			switch (LocalName)
			{
				case "description": // Smart contract
				case "Description": // Simplified (ex. state machine note command).
					return true;

				default:
					return false;
			}
		}
	}
}
