﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Persistence;
using Waher.Script;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Contract-reference parameter
	/// </summary>
	public class ContractReferenceParameter : Parameter
	{
		private HumanReadableText[] labels = null;
		private CaseInsensitiveString value = CaseInsensitiveString.Empty;
		private string localName = string.Empty;
		private string @namespace = string.Empty;
		private string templateId = string.Empty;
		private string provider = string.Empty;
		private string creatorRole = string.Empty;
		private bool required = false;

		/// <summary>
		/// Parameter value.
		/// </summary>
		public override object ObjectValue => this.value;

		/// <summary>
		/// Human-readable label to be shown in stead of a reference, in contract text, in different languages.
		/// </summary>
		public HumanReadableText[] Labels
		{
			get => this.labels;
			set => this.labels = value;
		}

		/// <summary>
		/// The value of the parameter, containing the ID of the contract that is being referenced.
		/// </summary>
		public CaseInsensitiveString Value
		{
			get => this.value;
			set => this.value = value;
		}

		/// <summary>
		/// Restriction on the local name of the machine-readable part of the referenced contract.
		/// </summary>
		public string LocalName
		{
			get => this.value;
			set => this.value = value;
		}

		/// <summary>
		/// Restriction on the namespace of the machine-readable part of the referenced contract.
		/// </summary>
		public string Namespace
		{
			get => this.@namespace;
			set => this.@namespace = value;
		}

		/// <summary>
		/// Restriction on the Template ID of the referenced contract.
		/// </summary>
		public string TemplateId
		{
			get => this.templateId;
			set => this.templateId = value;
		}

		/// <summary>
		/// Restriction on the provider of the referenced contract.
		/// </summary>
		public string Provider
		{
			get => this.provider;
			set => this.provider = value;
		}

		/// <summary>
		/// Restriction on the role the creator must have in the referenced contract.
		/// </summary>
		public string CreatorRole
		{
			get => this.creatorRole;
			set => this.creatorRole = value;
		}

		/// <summary>
		/// If the reference parameter is required or not.
		/// </summary>
		public bool Required
		{
			get => this.required;
			set => this.required = value;
		}

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="UsingTemplate">If the XML is for creating a contract using a template.</param>
		public override void Serialize(StringBuilder Xml, bool UsingTemplate)
		{
			Xml.Append("<contractReferenceParameter name=\"");
			Xml.Append(XML.Encode(this.Name));

			if (!CaseInsensitiveString.IsNullOrEmpty(this.value))
			{
				Xml.Append("\" value=\"");
				Xml.Append(XML.Encode(this.value.Value));
			}

			if (UsingTemplate)
				Xml.Append("\"/>");
			else
			{
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

				if (!string.IsNullOrEmpty(this.localName))
				{
					Xml.Append("\" localName=\"");
					Xml.Append(XML.Encode(this.localName.Normalize(NormalizationForm.FormC)));
				}

				if (!string.IsNullOrEmpty(this.@namespace))
				{
					Xml.Append("\" namespace=\"");
					Xml.Append(XML.Encode(this.@namespace.Normalize(NormalizationForm.FormC)));
				}

				if (!string.IsNullOrEmpty(this.templateId))
				{
					Xml.Append("\" templateId=\"");
					Xml.Append(XML.Encode(this.templateId.Normalize(NormalizationForm.FormC)));
				}

				if (!string.IsNullOrEmpty(this.provider))
				{
					Xml.Append("\" provider=\"");
					Xml.Append(XML.Encode(this.provider.Normalize(NormalizationForm.FormC)));
				}

				if (!string.IsNullOrEmpty(this.creatorRole))
				{
					Xml.Append("\" creatorRole=\"");
					Xml.Append(XML.Encode(this.creatorRole.Normalize(NormalizationForm.FormC)));
				}

				if (this.Required)
					Xml.Append("\" required=\"true");

				if (this.Transient)
					Xml.Append("\" transient=\"true");

				if ((!(this.Descriptions is null) && this.Descriptions.Length > 0) ||
					(!(this.labels is null) && this.labels.Length > 0))
				{
					Xml.Append("\">");

					if (!(this.Descriptions is null))
					{
						foreach (HumanReadableText Description in this.Descriptions)
							Description.Serialize(Xml, "description", false);
					}

					if (!(this.labels is null))
					{
						foreach (HumanReadableText Label in this.labels)
							Label.Serialize(Xml, "label", false);
					}

					Xml.Append("</contractReferenceParameter>");
				}
				else
					Xml.Append("\"/>");
			}
		}

		/// <summary>
		/// Checks if the parameter value is valid.
		/// </summary>
		/// <param name="Variables">Collection of parameter values.</param>
		/// <param name="Client">Connected contracts client. If offline or null, partial validation in certain cases will be performed.</param>
		/// <returns>If parameter value is valid.</returns>
		public override async Task<bool> IsParameterValid(Variables Variables, ContractsClient Client)
		{
			if (CaseInsensitiveString.IsNullOrEmpty(this.value))
			{
				if (this.required)
					return false;
				else
					return true;
			}

			if (!XmppClient.BareJidRegEx.IsMatch(this.value))
				return false;

			if (!(Client is null) && Client.Client.State == XmppState.Connected)
			{
				Contract Contract;

				try
				{
					Contract = await Client.GetContractAsync(this.value);
					if (Contract is null)
						return false;

					if (!string.IsNullOrEmpty(this.localName) &&
						this.localName != Contract.ForMachinesLocalName)
					{
						return false;
					}

					if (!string.IsNullOrEmpty(this.@namespace) &&
						this.@namespace != Contract.ForMachinesNamespace)
					{
						return false;
					}

					if (!string.IsNullOrEmpty(this.templateId) &&
						this.templateId != Contract.TemplateId)
					{
						return false;
					}

					if (!string.IsNullOrEmpty(this.provider) &&
						this.provider != Contract.Provider)
					{
						return false;
					}

					if (!string.IsNullOrEmpty(this.creatorRole))
					{
						// TODO
					}

					ContractStatus Status = await Client.ValidateAsync(Contract, true);
					if (Status != ContractStatus.Valid)
						return false;
				}
				catch (Exception)
				{
					return false;
				}

				Dictionary<string, object> ContractParameters = new Dictionary<string, object>();

				foreach (Parameter P in Contract.Parameters)
					ContractParameters[P.Name] = P.ObjectValue;

				Variables[this.Name] = ContractParameters;
			}

			return await base.IsParameterValid(Variables, Client);
		}

		/// <summary>
		/// Populates a variable collection with the value of the parameter.
		/// </summary>
		/// <param name="Variables">Variable collection.</param>
		public override void Populate(Variables Variables)
		{
			// Populated in the validation phase.
		}

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public override void SetValue(object Value)
		{
			this.value = Value.ToString();
		}

		/// <summary>
		/// Sets the minimum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Minimum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMinValue(object Value, bool? Inclusive)
		{
			throw new InvalidOperationException("Minimum value for Contract Reference parameter types not supported.");
		}

		/// <summary>
		/// Sets the maximum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Maximum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMaxValue(object Value, bool? Inclusive)
		{
			throw new InvalidOperationException("Maximum value for Contract Reference parameter types not supported.");
		}

		/// <summary>
		/// Imports parameter values from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>If import was successful.</returns>
		public override async Task<bool> Import(XmlElement Xml)
		{
			if (!await base.Import(Xml))
				return false;

			List<HumanReadableText> Labels = new List<HumanReadableText>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "label": // Smart contract
							HumanReadableText Text = HumanReadableText.Parse(E);
							if (Text is null || !await Text.IsWellDefined())
								return false;

							Labels.Add(Text);
							break;

						case "Label": // Simplified (ex. state machine note command).
							Text = HumanReadableText.ParseSimplified(E);
							if (Text is null || !await Text.IsWellDefined())
								return false;

							Labels.Add(Text);
							break;
					}
				}
			}

			this.labels = Labels.ToArray();

			this.value = Xml.HasAttribute("value") ? XML.Attribute(Xml, "value") : null;
			this.required = XML.Attribute(Xml, "required", false);
			this.localName = Xml.HasAttribute("localName") ? XML.Attribute(Xml, "localName") : null;
			this.@namespace = Xml.HasAttribute("namespace") ? XML.Attribute(Xml, "namespace") : null;
			this.templateId = Xml.HasAttribute("templateId") ? XML.Attribute(Xml, "templateId") : null;
			this.provider = Xml.HasAttribute("provider") ? XML.Attribute(Xml, "provider") : null;
			this.creatorRole = Xml.HasAttribute("creatorRole") ? XML.Attribute(Xml, "creatorRole") : null;

			return true;
		}

		/// <summary>
		/// If a local name of a child XML element is recognized.
		/// </summary>
		/// <param name="LocalName">Local name.</param>
		/// <returns>If recognized.</returns>
		protected override bool IsChildLocalNameRecognized(string LocalName)
		{
			switch (LocalName)
			{
				case "label": // Smart contract
				case "Label": // Simplified (ex. state machine note command).
					return true;

				default:
					return base.IsChildLocalNameRecognized(LocalName);
			}
		}
	}
}