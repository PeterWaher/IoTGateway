using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Text;
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
    /// Represents a text-valued parameter.
    /// </summary>
    public class TextParameter : ReportParameterWithOptions
	{
		private readonly ReportStringAttribute[] defaultValue;
		private readonly ReportStringAttribute contentType;
		private readonly ReportUInt16Attribute minCount;
		private readonly ReportUInt16Attribute maxCount;
		private readonly int nrDefaultValues;

		/// <summary>
		/// Represents a text-valued parameter.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public TextParameter(XmlElement Xml)
			: base(Xml)
		{
			this.contentType = new ReportStringAttribute(Xml, "contentType");
			this.minCount = new ReportUInt16Attribute(Xml, "minCount");
			this.maxCount = new ReportUInt16Attribute(Xml, "maxCount");

			List<ReportStringAttribute> DefaultValues = new List<ReportStringAttribute>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "DefaultValue")
					DefaultValues.Add(new ReportStringAttribute(E, null));
			}

			this.defaultValue = DefaultValues.ToArray();
			this.nrDefaultValues = this.defaultValue.Length;
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
            string ContentType = await this.contentType.Evaluate(Variables, PlainTextCodec.DefaultContentType);
			ushort? MinCount = this.minCount.IsEmpty ? null : (ushort?)await this.minCount.Evaluate(Variables);
			ushort? MaxCount = this.maxCount.IsEmpty ? null : (ushort?)await this.maxCount.Evaluate(Variables);
			ValidationMethod Validation = new BasicValidation();
            Field Field;

            if (MinCount.HasValue || MaxCount.HasValue)
                Validation = new ListRangeValidation(Validation, MinCount ?? 0, MaxCount ?? ushort.MaxValue);

            if (Attributes.RestrictToOptions)
            {
                Field = new ListMultiField(Parameters, Attributes.Name, Attributes.Label, Attributes.Required,
					await this.GetDefaultValue(Variables), Attributes.Options, Attributes.Description, 
					StringDataType.Instance, Validation, string.Empty, false, false, false);
            }
            else
            {
                Field = new TextMultiField(Parameters, Attributes.Name, Attributes.Label, Attributes.Required,
					await this.GetDefaultValue(Variables), Attributes.Options, Attributes.Description, 
					StringDataType.Instance, Validation, string.Empty, false, false, false, ContentType);
            }

            Parameters.Add(Field);

            Page Page = Parameters.GetPage(Attributes.Page);
            Page.Add(Field);
        }

		private async Task<string[]> GetDefaultValue(Variables Variables)
		{
			string[] DefaultValue = new string[this.nrDefaultValues];
			int i;

			for (i = 0; i < this.nrDefaultValues; i++)
				DefaultValue[i] = await this.defaultValue[i].Evaluate(Variables);

			return DefaultValue;
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
			ushort? MinCount = this.minCount.IsEmpty ? null : (ushort?)await this.minCount.Evaluate(Variables);
			ushort? MaxCount = this.maxCount.IsEmpty ? null : (ushort?)await this.maxCount.Evaluate(Variables);
			Field Field = Parameters[Name];

            if (Field is null)
            {
                if (Required)
                    Result.AddError(Name, await Language.GetStringAsync(typeof(ReportFileNode), 1, "Required parameter."));

                Variables[Name] = null;
            }
            else
            {
                string[] s = Field.ValueStrings;

                if (s is null || s.Length == 0)
                {
                    if (Required)
                        Result.AddError(Name, await Language.GetStringAsync(typeof(ReportFileNode), 1, "Required parameter."));

                    Variables[Name] = null;
                }
                else
                {
                    Variables[Name] = s;

					if (MinCount.HasValue && s.Length < MinCount.Value)
						Result.AddError(Name, await Language.GetStringAsync(typeof(ReportFileNode), 5, "Too few rows."));
					else if (MaxCount.HasValue && s.Length > MaxCount.Value)
						Result.AddError(Name, await Language.GetStringAsync(typeof(ReportFileNode), 6, "Too many rows."));
				}
			}
        }

    }
}