﻿using System.Text;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements.ValueRendering;
using Waher.Persistence;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.InlineElements
{
	/// <summary>
	/// Is replaced by parameter value
	/// </summary>
	public class Parameter : InlineElement
	{
		private string name;

		/// <summary>
		/// Name of parameter
		/// </summary>
		public string Name
		{
			get => this.name;
			set => this.name = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		/// <returns>Returns first failing element, if found.</returns>
		public override Task<HumanReadableElement> IsWellDefined()
		{
			return Task.FromResult<HumanReadableElement>(string.IsNullOrEmpty(this.name) ? this : null);
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<parameter name=\"");
			Xml.Append(XML.Encode(this.name));
			Xml.Append("\"/>");
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public override async Task GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			switch (Settings.Type)
			{
				case MarkdownType.ForEditing:
					Markdown.Append("[%");
					Markdown.Append(this.name);
					Markdown.Append(']');
					break;

				case MarkdownType.ForRendering:
					object Value;

					if (Settings.Contract is null)
						Value = null;
					else
					{
						int i;

						if (Settings.Contract.TryGetParameter(this.name, out Contracts.Parameter Parameter))
						{
							if (Parameter is ContractReferenceParameter ContractReferenceParameter
								&& !(ContractReferenceParameter.Labels is null) &&
								ContractReferenceParameter.Labels.Length > 0)
							{
								bool HasReference = !CaseInsensitiveString.IsNullOrEmpty(ContractReferenceParameter.Value);

								if (HasReference)
									Markdown.Append('[');

								foreach (Label Label in ContractReferenceParameter.Labels)
									await Label.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings);

								if (HasReference)
								{
									Markdown.Append("](iotsc:");
									Markdown.Append(ContractReferenceParameter.Value.Value);
									Markdown.Append(')');
								}

								break;
							}

							Value = Parameter.ObjectValue;

							if (Parameter is AttachmentParameter)
							{
								// TODO: Use attachment as value.
							}
						}
						else if ((i = this.name.IndexOf('.')) > 0 &&
							Settings.Contract.TryGetParameter(this.name[..i], out Parameter) &&
							Parameter is ContractReferenceParameter ContractReferenceParameter)
						{
							if (ContractReferenceParameter.Reference is null ||
								!ContractReferenceParameter.Reference.TryGetParameter(this.name[(i + 1)..], out Parameter))
							{
								Value = null;
							}
							else
								Value = Parameter.ObjectValue;
						}
						else
							Value = null;

						if (!(Value is null))
							Value = await Settings.Contract.FormatParameterValue(this.name, Value);
					}

					if (Value is null)
					{
						string Guide = null;

						foreach (Contracts.Parameter P in Settings.Contract.Parameters)
						{
							if (P.Name == this.name)
							{
								Guide = P.Guide;
								break;
							}
						}

						if (string.IsNullOrEmpty(Guide))
							Guide = this.name;

						Markdown.Append('`');
						Markdown.Append(Guide);
						Markdown.Append('`');
					}
					else
					{
						IParameterValueRenderer ValueRenderer =
							Types.FindBest<IParameterValueRenderer, object>(Value);

						string s;

						if (ValueRenderer is null)
							s = MarkdownEncode(Value.ToString(), Settings.SimpleEscape);
						else
						{
							s = this.GetLanguage(Settings.Contract);
							if (string.IsNullOrEmpty(s))
								s = Translator.DefaultLanguageCode;

							s = await ValueRenderer.ToString(Value, s, Settings);

							if (!ValueRenderer.IsMarkdownOutput)
								s = MarkdownEncode(s, Settings.SimpleEscape);
						}

						Markdown.Append(s);
					}
					break;
			}
		}
	}
}
