using System.Collections.Generic;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using BlockElements = Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements;
using InlineElements = Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements;
using Waher.Runtime.Geo;
using Waher.Content.Markdown.Model.BlockElements;

namespace Waher.Mcp.Identity.Resources
{
	/// <summary>
	/// Contract resource extensions.
	/// </summary>
	public static class JsonExtensions
	{
		/// <summary>
		/// Converts a legal identity to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="Identity">Legal Identity</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this LegalIdentity Identity)
		{
			if (Identity is null)
				return null;

			return new Dictionary<string, object?>()
			{
				{ "Id", Identity.Id },
				{ "Provider", Identity.Provider },
				{ "State", Identity.State },
				{ "Created", Identity.Created },
				{ "Updated", Identity.Updated },
				{ "From", Identity.From },
				{ "To", Identity.To },
				{ "Properties", Identity.Properties.ToJson() },
				{ "Attachments", Identity.Attachments.ToJson() },
				{ "ClientKeyName", Identity.ClientKeyName },
				{ "ClientPubKey", Identity.ClientPubKey },
				{ "ClientSignature", Identity.ClientSignature }
			};
		}

		/// <summary>
		/// Converts a property to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="Property">Property</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this Property Property)
		{
			if (Property is null)
				return null;

			return new Dictionary<string, object?>()
			{
				{ "Name", Property.Name },
				{ "Value", Property.Value }
			};
		}

		/// <summary>
		/// Converts an array of properties to an array of dictionaries for JSON encoding.
		/// </summary>
		/// <param name="Properties">Properties</param>
		/// <returns>Dictionaries</returns>
		public static Dictionary<string, object?>?[] ToJson(this Property[] Properties)
		{
			int i, c = Properties?.Length ?? 0;
			Dictionary<string, object?>?[] Result = new Dictionary<string, object?>?[c];

			for (i = 0; i < c; i++)
				Result[i] = Properties![i].ToJson();

			return Result;
		}

		/// <summary>
		/// Converts a attachment to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="Attachment">Attachment</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this Attachment Attachment)
		{
			if (Attachment is null)
				return null;

			return new Dictionary<string, object?>()
			{
				{ "Id", Attachment.Id },
				{ "LegalId", Attachment.LegalId },
				{ "ContentType", Attachment.ContentType },
				{ "FileName", Attachment.FileName },
				{ "Url", Attachment.Url },
				{ "Signature", Attachment.Signature },
				{ "Timestamp", Attachment.Timestamp }
			};
		}

		/// <summary>
		/// Converts an array of attachments to an array of dictionaries for JSON encoding.
		/// </summary>
		/// <param name="Attachments">Attachments</param>
		/// <returns>Dictionaries</returns>
		public static Dictionary<string, object?>?[] ToJson(this Attachment[] Attachments)
		{
			int i, c = Attachments?.Length ?? 0;
			Dictionary<string, object?>?[] Result = new Dictionary<string, object?>?[c];

			for (i = 0; i < c; i++)
				Result[i] = Attachments![i].ToJson();

			return Result;
		}

		/// <summary>
		/// Converts a contract to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="Contract">Contract</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this Contract Contract)
		{
			if (Contract is null)
				return null;

			return new Dictionary<string, object?>()
			{
				{ "Id", Contract.ContractId },
				{ "TemplateId", Contract.TemplateId },
				{ "Provider", Contract.Provider },
				{ "ForMachinesLocalName", Contract.ForMachinesLocalName },
				{ "ForMachinesNamespace", Contract.ForMachinesNamespace },
				{ "ContentSchemaDigest", Contract.ContentSchemaDigest },
				{ "ContentSchemaHashFunction", Contract.ContentSchemaHashFunction },
				{ "Nonce", Contract.Nonce },
				{ "ForMachines", Contract.ForMachines.OuterXml },
				{ "State", Contract.State },
				{ "Visibility", Contract.Visibility },
				{ "Created", Contract.Created },
				{ "Updated", Contract.Updated },
				{ "From", Contract.From },
				{ "To", Contract.To },
				{ "SignAfter", Contract.SignAfter },
				{ "SignBefore", Contract.SignBefore },
				{ "Duration", Contract.Duration },
				{ "ArchiveRequired", Contract.ArchiveRequired },
				{ "ArchiveOptional", Contract.ArchiveOptional },
				{ "CanActAsTemplate", Contract.CanActAsTemplate },
				{ "Roles", Contract.Roles.ToJson() },
				{ "Parts", Contract.Parts.ToJson() },
				{ "Parameters", Contract.Parameters.ToJson() },
				{ "ForHumans", Contract.ForHumans.ToJson() },
				{ "ClientSignatures", Contract.ClientSignatures.ToJson() },
				{ "ServerSignature", Contract.ServerSignature.ToJson() }
			};
		}

		/// <summary>
		/// Converts a role to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="Role">Role</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this Role Role)
		{
			if (Role is null)
				return null;

			return new Dictionary<string, object?>()
			{
				{ "Name", Role.Name },
				{ "MinCount", Role.MinCount },
				{ "MaxCount", Role.MaxCount },
				{ "CanRevoke", Role.CanRevoke }
			};
		}

		/// <summary>
		/// Converts an array of roles to an array of dictionaries for JSON encoding.
		/// </summary>
		/// <param name="Roles">Roles</param>
		/// <returns>Dictionaries</returns>
		public static Dictionary<string, object?>?[] ToJson(this Role[] Roles)
		{
			int i, c = Roles?.Length ?? 0;
			Dictionary<string, object?>?[] Result = new Dictionary<string, object?>?[c];

			for (i = 0; i < c; i++)
				Result[i] = Roles![i].ToJson();

			return Result;
		}

		/// <summary>
		/// Converts a part to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="Part">Part</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this Part Part)
		{
			if (Part is null)
				return null;

			return new Dictionary<string, object?>()
			{
				{ "LegalId", Part.LegalId },
				{ "LegalIdUri", Part.LegalIdUri },
				{ "Role", Part.Role }
			};
		}

		/// <summary>
		/// Converts an array of parts to an array of dictionaries for JSON encoding.
		/// </summary>
		/// <param name="Parts">Parts</param>
		/// <returns>Dictionaries</returns>
		public static Dictionary<string, object?>?[] ToJson(this Part[] Parts)
		{
			int i, c = Parts?.Length ?? 0;
			Dictionary<string, object?>?[] Result = new Dictionary<string, object?>?[c];

			for (i = 0; i < c; i++)
				Result[i] = Parts![i].ToJson();

			return Result;
		}

		/// <summary>
		/// Converts a parameter to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="Parameter">Parameter</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this Parameter Parameter)
		{
			if (Parameter is null)
				return null;

			Dictionary<string, object?> Result = new Dictionary<string, object?>()
			{
				{ "Name", Parameter.Name },
				{ "Guide", Parameter.Guide },
				{ "Expression", Parameter.Expression },
				{ "Protection", Parameter.Protection },
				{ "HasError", Parameter.ErrorReason.HasValue },
				{ "ParameterType", Parameter.ParameterType }
			};

			if (Parameter.ErrorReason.HasValue)
			{
				Result["ErrorReason"] = Parameter.ErrorReason.Value;
				Result["ErrorText"] = Parameter.ErrorText;
			}

			if (Parameter.CanSerializeValue)
				Result["Value"] = Parameter.ObjectValue;

			if (Parameter.CanSerializeProtectedValue)
				Result["ProtectedValue"] = Parameter.ProtectedValue;

			if (Parameter is AttachmentParameter AttachmentParameter)
			{
				Result["ContentType"] = AttachmentParameter.ContentType;
				Result["Required"] = AttachmentParameter.Required;

				if (AttachmentParameter.MinSize.HasValue)
					Result["MinSize"] = AttachmentParameter.MinSize;

				if (AttachmentParameter.MaxSize.HasValue)
					Result["MaxSize"] = AttachmentParameter.MaxSize;

				if (AttachmentParameter.MinWidth.HasValue)
					Result["MinWidth"] = AttachmentParameter.MinWidth;

				if (AttachmentParameter.MaxWidth.HasValue)
					Result["MaxWidth"] = AttachmentParameter.MaxWidth;

				if (AttachmentParameter.MinHeight.HasValue)
					Result["MinHeight"] = AttachmentParameter.MinHeight;

				if (AttachmentParameter.MaxHeight.HasValue)
					Result["MaxHeight"] = AttachmentParameter.MaxHeight;
			}
			else if (Parameter is ContractReferenceParameter ContractReferenceParameter)
			{
				Result["Labels"] = ContractReferenceParameter.Labels.ToJson();
				Result["LocalName"] = ContractReferenceParameter.LocalName;
				Result["Namespace"] = ContractReferenceParameter.Namespace;
				Result["TemplateId"] = ContractReferenceParameter.TemplateId;
				Result["Provider"] = ContractReferenceParameter.Provider;
				Result["CreatorRole"] = ContractReferenceParameter.CreatorRole;
				Result["Required"] = ContractReferenceParameter.Required;
				Result["Reference"] = ContractReferenceParameter.Reference.ToJson();
			}
			else if (Parameter is DateParameter DateParameter)
			{
				if (DateParameter.Min.HasValue)
				{
					Result["Min"] = DateParameter.Min.Value;
					Result["MinIncluded"] = DateParameter.MinIncluded;
				}

				if (DateParameter.Max.HasValue)
				{
					Result["Max"] = DateParameter.Max.Value;
					Result["MaxIncluded"] = DateParameter.MaxIncluded;
				}
			}
			else if (Parameter is DateTimeParameter DateTimeParameter)
			{
				if (DateTimeParameter.Min.HasValue)
				{
					Result["Min"] = DateTimeParameter.Min.Value;
					Result["MinIncluded"] = DateTimeParameter.MinIncluded;
				}

				if (DateTimeParameter.Max.HasValue)
				{
					Result["Max"] = DateTimeParameter.Max.Value;
					Result["MaxIncluded"] = DateTimeParameter.MaxIncluded;
				}
			}
			else if (Parameter is DurationParameter DurationParameter)
			{
				if (DurationParameter.Min.HasValue)
				{
					Result["Min"] = DurationParameter.Min.Value;
					Result["MinIncluded"] = DurationParameter.MinIncluded;
				}

				if (DurationParameter.Max.HasValue)
				{
					Result["Max"] = DurationParameter.Max.Value;
					Result["MaxIncluded"] = DurationParameter.MaxIncluded;
				}
			}
			else if (Parameter is GeoParameter GeoParameter)
			{
				Result["Value"] = GeoParameter.Value.ToJson();
				Result["ContractLocation"] = GeoParameter.ContractLocation;
				Result["Min"] = GeoParameter.Min.ToJson();
				Result["MinIncluded"] = GeoParameter.MinIncluded;
				Result["Max"] = GeoParameter.Max.ToJson();
				Result["MaxIncluded"] = GeoParameter.MaxIncluded;
				Result["Altitude"] = GeoParameter.Altitude;
			}
			else if (Parameter is NumericalParameter NumericalParameter)
			{
				if (NumericalParameter.Min.HasValue)
				{
					Result["Min"] = NumericalParameter.Min.Value;
					Result["MinIncluded"] = NumericalParameter.MinIncluded;
				}

				if (NumericalParameter.Max.HasValue)
				{
					Result["Max"] = NumericalParameter.Max.Value;
					Result["MaxIncluded"] = NumericalParameter.MaxIncluded;
				}
			}
			else if (Parameter is RoleParameter RoleParameter)
			{
				Result["Role"] = RoleParameter.Role;
				Result["Index"] = RoleParameter.Index;
				Result["Property"] = RoleParameter.Property;
				Result["Required"] = RoleParameter.Required;
				Result["ContentType"] = RoleParameter.ContentType;

				if (RoleParameter.HasAttachmentValue)
				{
					Result["AttachmentUrl"] = RoleParameter.AttachmentUrl;
					Result["AttachmentId"] = RoleParameter.AttachmentId.Value;
					Result["AttachmentLegalId"] = RoleParameter.AttachmentLegalId.Value;
					Result["AttachmentContentType"] = RoleParameter.AttachmentContentType;
					Result["AttachmentFileName"] = RoleParameter.AttachmentFileName;
					Result["AttachmentSignature"] = RoleParameter.AttachmentSignature;
					Result["AttachmentTimestamp"] = RoleParameter.AttachmentTimestamp;
					Result["AttachmentTimestamp"] = RoleParameter.AttachmentTimestamp;
				}
			}
			else if (Parameter is StringParameter StringParameter)
			{
				Result["RegEx"] = StringParameter.RegEx;

				if (!(StringParameter.Min is null))
				{
					Result["Min"] = StringParameter.Min;
					Result["MinIncluded"] = StringParameter.MinIncluded;
				}

				if (!(StringParameter.Max is null))
				{
					Result["Max"] = StringParameter.Max;
					Result["MaxIncluded"] = StringParameter.MaxIncluded;
				}

				if (StringParameter.MinLength.HasValue)
					Result["MinLength"] = StringParameter.MinLength;

				if (StringParameter.MaxLength.HasValue)
					Result["MaxLength"] = StringParameter.MaxLength;
			}
			else if (Parameter is TimeParameter TimeParameter)
			{
				if (TimeParameter.Min.HasValue)
				{
					Result["Min"] = TimeParameter.Min.Value;
					Result["MinIncluded"] = TimeParameter.MinIncluded;
				}

				if (TimeParameter.Max.HasValue)
				{
					Result["Max"] = TimeParameter.Max.Value;
					Result["MaxIncluded"] = TimeParameter.MaxIncluded;
				}
			}

			return Result;
		}

		/// <summary>
		/// Converts an array of parameters to an array of dictionaries for JSON encoding.
		/// </summary>
		/// <param name="Parameters">Parameters</param>
		/// <returns>Dictionaries</returns>
		public static Dictionary<string, object?>?[] ToJson(this Parameter[] Parameters)
		{
			int i, c = Parameters?.Length ?? 0;
			Dictionary<string, object?>?[] Result = new Dictionary<string, object?>?[c];

			for (i = 0; i < c; i++)
				Result[i] = Parameters![i].ToJson();

			return Result;
		}

		/// <summary>
		/// Converts a client signature to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="ClientSignature">Client Signature</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this ClientSignature ClientSignature)
		{
			if (ClientSignature is null)
				return null;

			return new Dictionary<string, object?>()
			{
				{ "LegalId", ClientSignature.LegalId },
				{ "LegalIdUri", ClientSignature.LegalIdUri },
				{ "BareJid", ClientSignature.BareJid },
				{ "Role", ClientSignature.Role },
				{ "Transferable", ClientSignature.Transferable },
				{ "Timestamp", ClientSignature.Timestamp },
				{ "Signature", ClientSignature.DigitalSignature }
			};
		}

		/// <summary>
		/// Converts an array of client signatures to an array of dictionaries for JSON encoding.
		/// </summary>
		/// <param name="ClientSignatures">Client Signatures</param>
		/// <returns>Dictionaries</returns>
		public static Dictionary<string, object?>?[] ToJson(this ClientSignature[] ClientSignatures)
		{
			int i, c = ClientSignatures?.Length ?? 0;
			Dictionary<string, object?>?[] Result = new Dictionary<string, object?>?[c];

			for (i = 0; i < c; i++)
				Result[i] = ClientSignatures![i].ToJson();

			return Result;
		}

		/// <summary>
		/// Converts a server signature to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="ServerSignature">Server Signature</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this ServerSignature ServerSignature)
		{
			if (ServerSignature is null)
				return null;

			return new Dictionary<string, object?>()
			{
				{ "Timestamp", ServerSignature.Timestamp },
				{ "Signature", ServerSignature.DigitalSignature }
			};
		}

		/// <summary>
		/// Converts a server signature to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="GeoPosition">Geo-position</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this GeoPosition GeoPosition)
		{
			if (GeoPosition is null)
				return null;

			Dictionary<string, object?> Result = new Dictionary<string, object?>()
			{
				{ "Latitude", GeoPosition.Latitude },
				{ "Longitude", GeoPosition.Longitude },
				{ "NormalizedLatitude", GeoPosition.NormalizedLatitude },
				{ "NormalizedLongitude", GeoPosition.NormalizedLongitude },
				{ "HumanReadable", GeoPosition.HumanReadable }
			};

			if (GeoPosition.Altitude.HasValue)
				Result["Altitude"] = GeoPosition.Altitude.Value;

			return Result;
		}

		/// <summary>
		/// Converts a human-readable text to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="HumanReadableText">Human-readable text</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this HumanReadableText HumanReadableText)
		{
			if (HumanReadableText is null)
				return null;

			return new Dictionary<string, object?>()
			{
				{ "Language", HumanReadableText.Language },
				{ "Body", HumanReadableText.Body.ToJson() }
			};
		}

		/// <summary>
		/// Converts an array of human-readable texts to an array of dictionaries for JSON encoding.
		/// </summary>
		/// <param name="HumanReadableTexts">Human-readable texts</param>
		/// <returns>Dictionaries</returns>
		public static Dictionary<string, object?>?[] ToJson(this HumanReadableText[] HumanReadableTexts)
		{
			int i, c = HumanReadableTexts?.Length ?? 0;
			Dictionary<string, object?>?[] Result = new Dictionary<string, object?>?[c];

			for (i = 0; i < c; i++)
				Result[i] = HumanReadableTexts![i].ToJson();

			return Result;
		}

		/// <summary>
		/// Converts a block element to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="BlockElement">Block element</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this BlockElements.BlockElement BlockElement)
		{
			if (BlockElement is null)
				return null;

			Dictionary<string, object?> Result = new Dictionary<string, object?>()
			{
				{ "Type", BlockElement.GetType().Name }
			};

			if (BlockElement is BlockElements.Blocks Blocks)
			{
				Result["Body"] = Blocks.Body.ToJson();

				if (Blocks is BlockElements.Section Section)
					Result["Header"] = Section.Header.ToJson();
			}
			else if (BlockElement is BlockElements.InlineBlock InlineBlock)
			{
				Result["Elements"] = InlineBlock.Elements.ToJson();

				if (InlineBlock is BlockElements.Image Image)
				{
					Result["Data"] = Image.Data;
					Result["ContentType"] = Image.ContentType;
					Result["Width"] = Image.Width;
					Result["Height"] = Image.Height;
				}
			}
			else if (BlockElement is BlockElements.ItemList ItemList)
				Result["Items"] = ItemList.Items.ToJson();
			else if (BlockElement is BlockElements.Item Item)
			{
				if ((Item.InlineElements?.Length??0)>0)
					Result["InlineElements"] = Item.InlineElements!.ToJson();

				if ((Item.BlockElements?.Length ?? 0) > 0)
					Result["BlockElements"] = Item.BlockElements!.ToJson();

				if (Item is BlockElements.Cell Cell)
				{
					Result["Header"] = Cell.Header;
					Result["ColumnSpan"] = Cell.ColumnSpan;
					Result["Alignment"] = Cell.Alignment;
				}
			}
			else if (BlockElement is BlockElements.Row Row)
				Result["Cells"] = Row.Cells.ToJson();
			else if (BlockElement is BlockElements.Table Table)
				Result["Rows"] = Table.Rows.ToJson();

			return Result;
		}

		/// <summary>
		/// Converts an array of block elements to an array of dictionaries for JSON encoding.
		/// </summary>
		/// <param name="BlockElements">Block Elements</param>
		/// <returns>Dictionaries</returns>
		public static Dictionary<string, object?>?[] ToJson(this BlockElements.BlockElement[] BlockElements)
		{
			int i, c = BlockElements?.Length ?? 0;
			Dictionary<string, object?>?[] Result = new Dictionary<string, object?>?[c];

			for (i = 0; i < c; i++)
				Result[i] = BlockElements![i].ToJson();

			return Result;
		}

		/// <summary>
		/// Converts a inline element to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="InlineElement">Inline element</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this InlineElements.InlineElement InlineElement)
		{
			if (InlineElement is null)
				return null;

			Dictionary<string, object?> Result = new Dictionary<string, object?>()
			{
				{ "Type", InlineElement.GetType().Name }
			};

			if (InlineElement is InlineElements.Text Text)
				Result["Text"] = Text.Value;
			else if (InlineElement is InlineElements.Parameter Parameter)
				Result["Name"] = Parameter.Name;
			else if (InlineElement is InlineElements.Formatting Formatting)
			{
				Result["Elements"] = Formatting.Elements.ToJson();

				if (Formatting is Label Label)
					Result["Language"] = Label.Language;
			}
			else if (InlineElement is InlineElements.Image Image)
			{
				Result["Data"] = Image.Data;
				Result["ContentType"] = Image.ContentType;
				Result["Width"] = Image.Width;
				Result["Height"] = Image.Height;
			}

			return Result;
		}

		/// <summary>
		/// Converts an array of inline elements to an array of dictionaries for JSON encoding.
		/// </summary>
		/// <param name="InlineElements">Inline Elements</param>
		/// <returns>Dictionaries</returns>
		public static Dictionary<string, object?>?[] ToJson(this InlineElements.InlineElement[] InlineElements)
		{
			int i, c = InlineElements?.Length ?? 0;
			Dictionary<string, object?>?[] Result = new Dictionary<string, object?>?[c];

			for (i = 0; i < c; i++)
				Result[i] = InlineElements![i].ToJson();

			return Result;
		}

		/// <summary>
		/// Converts a human-readable element to a dictionary for JSON encoding.
		/// </summary>
		/// <param name="HumanReadableElement">Human-readable element</param>
		/// <returns>Dictionary</returns>
		public static Dictionary<string, object?>? ToJson(this HumanReadableElement HumanReadableElement)
		{
			if (HumanReadableElement is BlockElements.BlockElement BlockElement)
				return BlockElement.ToJson();
			else if (HumanReadableElement is InlineElements.InlineElement InlineElement)
				return InlineElement.ToJson();
			else
				return null;
		}

		/// <summary>
		/// Converts an array of human-readable elements to an array of dictionaries for JSON encoding.
		/// </summary>
		/// <param name="HumanReadableElements">HumanReadableElements</param>
		/// <returns>Dictionaries</returns>
		public static Dictionary<string, object?>?[] ToJson(this HumanReadableElement[] HumanReadableElements)
		{
			int i, c = HumanReadableElements?.Length ?? 0;
			Dictionary<string, object?>?[] Result = new Dictionary<string, object?>?[c];

			for (i = 0; i < c; i++)
				Result[i] = HumanReadableElements![i].ToJson();

			return Result;
		}

	}
}
