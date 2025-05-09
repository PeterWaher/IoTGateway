using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Script;

namespace Waher.Networking.XMPP.Contracts
{
	/// <summary>
	/// Attachment-valued contractual parameter
	/// </summary>
	public class AttachmentParameter : Parameter
	{
		private string value;
		private string contentType;
		private Regex contentTypeRegEx = null;
		private bool required = false;
		private int? minSize = null;
		private int? maxSize = null;
		private int? minWidth = null;
		private int? maxWidth = null;
		private int? minHeight = null;
		private int? maxHeight = null;

		/// <summary>
		/// Parameter value
		/// </summary>
		public string Value
		{
			get => this.@value;
			set
			{
				this.@value = value;
				this.ProtectedValue = null;
			}
		}

		/// <summary>
		/// Optional Content-Type.
		/// </summary>
		[DefaultValueNull]
		public string ContentType
		{
			get => this.contentType;
			set
			{
				this.contentType = value;
				this.contentTypeRegEx = null;
			}
		}

		/// <summary>
		/// If the attachment is required
		/// </summary>
		[DefaultValue(false)]
		public bool Required
		{
			get => this.required;
			set => this.required = value;
		}

		/// <summary>
		/// Optional minimum size of attachment.
		/// </summary>
		[DefaultValueNull]
		public int? MinSize
		{
			get => this.minSize;
			set => this.minSize = value;
		}

		/// <summary>
		/// Optional maximum size of attachment.
		/// </summary>
		[DefaultValueNull]
		public int? MaxSize
		{
			get => this.maxSize;
			set => this.maxSize = value;
		}

		/// <summary>
		/// Optional minimum width of attachment, if relevant.
		/// </summary>
		[DefaultValueNull]
		public int? MinWidth
		{
			get => this.minWidth;
			set => this.minWidth = value;
		}

		/// <summary>
		/// Optional maximum width of attachment, if relevant.
		/// </summary>
		[DefaultValueNull]
		public int? MaxWidth
		{
			get => this.maxWidth;
			set => this.maxWidth = value;
		}

		/// <summary>
		/// Optional minimum height of attachment, if relevant.
		/// </summary>
		[DefaultValueNull]
		public int? MinHeight
		{
			get => this.minHeight;
			set => this.minHeight = value;
		}

		/// <summary>
		/// Optional maximum height of attachment, if relevant.
		/// </summary>
		[DefaultValueNull]
		public int? MaxHeight
		{
			get => this.maxHeight;
			set => this.maxHeight = value;
		}

		/// <summary>
		/// Parameter value.
		/// </summary>
		public override object ObjectValue => this.@value;

		/// <summary>
		/// Attachment representation of value.
		/// </summary>
		public override string StringValue
		{
			get => this.Value ?? string.Empty;
			set => this.Value = value;
		}

		/// <summary>
		/// Parameter type name, corresponding to the local name of the parameter element in XML.
		/// </summary>
		public override string ParameterType => "attachmentParameter";

		/// <summary>
		/// Serializes the parameter, in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output</param>
		/// <param name="UsingTemplate">If the XML is for creating a contract using a template.</param>
		public override void Serialize(StringBuilder Xml, bool UsingTemplate)
		{
			Xml.Append("<attachmentParameter");

			if (!UsingTemplate)
			{
				if (!string.IsNullOrEmpty(this.Expression))
				{
					Xml.Append(" contentType=\"");
					Xml.Append(XML.Encode(this.contentType.Normalize(NormalizationForm.FormC)));
					Xml.Append('"');
				}

				if (!string.IsNullOrEmpty(this.contentType))
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

				if (this.maxHeight.HasValue)
				{
					Xml.Append(" maxHeight=\"");
					Xml.Append(this.maxHeight.Value.ToString());
					Xml.Append('"');
				}

				if (this.maxSize.HasValue)
				{
					Xml.Append(" maxSize=\"");
					Xml.Append(this.maxSize.Value.ToString());
					Xml.Append('"');
				}

				if (this.maxWidth.HasValue)
				{
					Xml.Append(" maxWidth=\"");
					Xml.Append(this.maxWidth.Value.ToString());
					Xml.Append('"');
				}

				if (this.minHeight.HasValue)
				{
					Xml.Append(" minHeight=\"");
					Xml.Append(this.minHeight.Value.ToString());
					Xml.Append('"');
				}

				if (this.minSize.HasValue)
				{
					Xml.Append(" minSize=\"");
					Xml.Append(this.minSize.Value.ToString());
					Xml.Append('"');
				}

				if (this.minWidth.HasValue)
				{
					Xml.Append(" minWidth=\"");
					Xml.Append(this.minWidth.Value.ToString());
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

				if (this.Required)
					Xml.Append(" required=\"true\"");
			}

			if (!(this.@value is null) && this.CanSerializeValue)
			{
				Xml.Append(" value=\"");
				Xml.Append(XML.Encode(this.@value));
				Xml.Append('"');
			}

			if (this.Descriptions is null || this.Descriptions.Length == 0)
				Xml.Append("/>");
			else
			{
				Xml.Append('>');

				foreach (HumanReadableText Description in this.Descriptions)
					Description.Serialize(Xml, "description", null);

				Xml.Append("</attachmentParameter>");
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
			if (string.IsNullOrEmpty(this.@value))
			{
				if (this.required)
				{
					this.ErrorReason = ParameterErrorReason.LacksValue;
					this.ErrorText = null;

					return Task.FromResult(false);
				}
			}
			else if (!string.IsNullOrEmpty(this.contentType))
			{
				if (!InternetContent.TryGetContentType(Path.GetExtension(this.value), out string ContentType))
				{
					this.ErrorReason = ParameterErrorReason.Outside;
					this.ErrorText = null;

					return Task.FromResult(false);
				}

				if (this.contentType.Contains('*'))
				{
					this.contentTypeRegEx ??= new Regex(Database.WildcardToRegex(this.contentType, "*"), RegexOptions.Singleline);

					Match M = this.contentTypeRegEx.Match(ContentType);

					if (!M.Success || M.Index > 0 || M.Length < ContentType.Length)
					{
						this.ErrorReason = ParameterErrorReason.Outside;
						this.ErrorText = null;

						return Task.FromResult(false);
					}
				}
				else if (string.Compare(this.contentType, ContentType, true) != 0)
				{
					this.ErrorReason = ParameterErrorReason.Outside;
					this.ErrorText = null;

					return Task.FromResult(false);
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
			// Populated in the validation phase.
		}

		/// <summary>
		/// Sets the parameter value.
		/// </summary>
		/// <param name="Value">Value</param>
		/// <exception cref="ArgumentException">If <paramref name="Value"/> is not of the correct type.</exception>
		public override void SetValue(object Value)
		{
			this.Value = Value.ToString();
		}

		/// <summary>
		/// Sets the minimum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Minimum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMinValue(object Value, bool? Inclusive)
		{
			throw new InvalidOperationException("Minimum value for Attachment parameter types not supported.");
		}

		/// <summary>
		/// Sets the maximum value allowed by the parameter.
		/// </summary>
		/// <param name="Value">Maximum value.</param>
		/// <param name="Inclusive">If the value is included in the range. If null, keeps the original value.</param>
		public override void SetMaxValue(object Value, bool? Inclusive)
		{
			throw new InvalidOperationException("Maximum value for Attachment parameter types not supported.");
		}

		/// <summary>
		/// Imports parameter values from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <returns>If import was successful.</returns>
		public override Task<bool> Import(XmlElement Xml)
		{
			this.@value = Xml.HasAttribute("value") ? XML.Attribute(Xml, "value") : null;
			this.required = XML.Attribute(Xml, "required", false);
			this.contentType = XML.Attribute(Xml, "contentType");
			this.minSize = Xml.HasAttribute("minSize") ? XML.Attribute(Xml, "minSize", 0) : (int?)null;
			this.maxSize = Xml.HasAttribute("maxSize") ? XML.Attribute(Xml, "maxSize", 0) : (int?)null;
			this.minWidth = Xml.HasAttribute("minWidth") ? XML.Attribute(Xml, "minWidth", 0) : (int?)null;
			this.maxWidth = Xml.HasAttribute("maxWidth") ? XML.Attribute(Xml, "maxWidth", 0) : (int?)null;
			this.minHeight = Xml.HasAttribute("minHeight") ? XML.Attribute(Xml, "minHeight", 0) : (int?)null;
			this.maxHeight = Xml.HasAttribute("maxHeight") ? XML.Attribute(Xml, "maxHeight", 0) : (int?)null;

			return base.Import(Xml);
		}

	}
}
