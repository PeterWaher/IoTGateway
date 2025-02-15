using System;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
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
    /// Represents a color-valued parameter.
    /// </summary>
    public class ColorParameter : ReportParameterWithOptions
	{
		private readonly ReportStringAttribute defaultValue;
        private readonly ReportBooleanAttribute alphaChannel;

		/// <summary>
		/// Represents a color-valued parameter.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public ColorParameter(XmlElement Xml)
			: base(Xml)
		{
            this.defaultValue = new ReportStringAttribute(Xml, "default");
			this.alphaChannel = new ReportBooleanAttribute(Xml, "alphaChannel");
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
            bool AlphaChannel = await this.alphaChannel.Evaluate(Variables, false);
			string[] DefaultValue;

			if (this.defaultValue.IsEmpty)
				DefaultValue = Array.Empty<string>();
			else
				DefaultValue = new string[] { await this.defaultValue.Evaluate(Variables) };

			DataType DataType = AlphaChannel ? (DataType)ColorAlphaDataType.Instance : ColorDataType.Instance;
            Field Field;

            if (Attributes.RestrictToOptions)
            {
                Field = new ListSingleField(Parameters, Attributes.Name, Attributes.Label, Attributes.Required,
                    DefaultValue, Attributes.Options, Attributes.Description, DataType, new BasicValidation(),
                    string.Empty, false, false, false);
            }
            else
            {
                Field = new TextSingleField(Parameters, Attributes.Name, Attributes.Label, Attributes.Required,
                    DefaultValue, Attributes.Options, Attributes.Description, DataType, new BasicValidation(),
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
			bool AlphaChannel = await this.alphaChannel.Evaluate(Variables);
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
                else if (AlphaChannel && ColorAlphaDataType.TryParse(s, out ColorReference Parsed))
                    Variables[Name] = Parsed;
                else if (!AlphaChannel && ColorDataType.TryParse(s, out Parsed))
                    Variables[Name] = Parsed;
                else
					Result.AddError(Name, await Language.GetStringAsync(typeof(ReportFileNode), 2, "Invalid value."));
			}
		}

    }
}