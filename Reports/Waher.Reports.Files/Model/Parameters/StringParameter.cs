using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Networking.XMPP.DataForms.ValidationMethods;
using Waher.Reports.Model.Attributes;
using Waher.Runtime.Language;
using Waher.Script;

namespace Waher.Reports.Files.Model.Parameters
{
	/// <summary>
	/// Represents a String-valued parameter.
	/// </summary>
	public class StringParameter : ReportParameterWithOptions
	{
		private readonly ReportStringAttribute defaultValue;
		private readonly ReportStringAttribute pattern;

		/// <summary>
		/// Represents a String-valued parameter.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public StringParameter(XmlElement Xml)
			: base(Xml)
		{
			this.defaultValue = new ReportStringAttribute(Xml, "default");
			this.pattern = new ReportStringAttribute(Xml, "pattern");
		}

		/// <summary>
		/// Populates a data form with parameters for the object.
		/// </summary>
		/// <param name="Parameters">Data form to host all editable parameters.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="Variables">Report variables.</param>
		public override async Task PopulateForm(DataForm Parameters, Language Language, Variables Variables)
		{
			ReportParameterWithOptionsAttributes Attributes = await this.GetReportParameterWithOptionsAttributes(Variables);
			string Default = this.defaultValue.IsEmpty ? null : await this.defaultValue.Evaluate(Variables);
			string Pattern = this.pattern.IsEmpty ? null : await this.pattern.Evaluate(Variables);
			ValidationMethod Validation;
			Field Field;
			string[] DefaultValue;

			if (!(Default is null))
				DefaultValue = new string[] { Default };
			else
				DefaultValue = Array.Empty<string>();

			if (string.IsNullOrEmpty(Pattern))
				Validation = new OpenValidation();
			else
				Validation = new RegexValidation(Pattern);

			if (Attributes.RestrictToOptions)
			{
				Field = new ListSingleField(Parameters, Attributes.Name, Attributes.Label, Attributes.Required,
					DefaultValue, Attributes.Options, Attributes.Description, StringDataType.Instance, Validation,
					string.Empty, false, false, false);
			}
			else
			{
				Field = new TextSingleField(Parameters, Attributes.Name, Attributes.Label, Attributes.Required,
					DefaultValue, Attributes.Options, Attributes.Description, StringDataType.Instance, Validation,
					string.Empty, false, false, false);
			}

			Parameters.Add(Field);

			Page Page = Parameters.GetPage(Attributes.Page);
			Page.Add(Field);
		}

		/// <summary>
		/// Sets the parameters of the object, based on contents in the data form.
		/// </summary>
		/// <param name="Parameters">Data form with parameter values.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <param name="Variables">Report variables.</param>
		/// <param name="Result">Result set to return to caller.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public override async Task SetParameter(DataForm Parameters, Language Language, bool OnlySetChanged, Variables Variables,
			SetEditableFormResult Result)
		{
			string Name = await this.GetName(Variables);
			bool Required = await this.IsRequired(Variables);
			Field Field = Parameters[Name];

			if (Field is null)
			{
				if (Required)
					Result.AddError(Name, await Language.GetStringAsync(typeof(ReportFileNode), 1, "Required parameter."));

				Variables[Name] = null;
			}
			else
			{
				string s = Field.ValueString;

				if (string.IsNullOrEmpty(s))
				{
					if (Required)
						Result.AddError(Name, await Language.GetStringAsync(typeof(ReportFileNode), 1, "Required parameter."));

					Variables[Name] = null;
				}
				else
				{
					Variables[Name] = s;

					string Pattern = await this.pattern.Evaluate(Variables);
					if (!string.IsNullOrEmpty(Pattern))
					{
						Regex Parsed = new Regex(Pattern);
						Match M = Parsed.Match(s);

						if (!M.Success || M.Index > 0 || M.Length != s.Length)
							Result.AddError(Name, await Language.GetStringAsync(typeof(ReportFileNode), 8, "Value does not match expected pattern."));
					}
				}
			}
		}

	}
}