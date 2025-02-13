using System;
using System.Threading.Tasks;
using Waher.Content.Xml;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Networking.XMPP.DataForms.ValidationMethods;
using Waher.Runtime.Language;
using Waher.Script;

namespace Waher.Reports.Files.Model.Parameters
{
    /// <summary>
    /// Represents a Date-valued parameter.
    /// </summary>
    public class DateParameter : ReportParameterWithOptions
	{
		/// <summary>
		/// Represents a Date-valued parameter.
		/// </summary>
		/// <param name="Page">Parameter Page</param>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Label">Parameter label.</param>
		/// <param name="Description">Parameter description.</param>
		/// <param name="Required">If parameter is required.</param>
		/// <param name="RestrictToOptions">If only values defined in options are valid values.</param>
		/// <param name="Options">Available options</param>
		/// <param name="DefaultValue">Default value of parameter.</param>
        /// <param name="Min">Optional minimum value.</param>
        /// <param name="Max">Optional maximum value.</param>
		public DateParameter(string Page, string Name, string Label, string Description,
			bool Required, bool RestrictToOptions, ParameterOption[] Options,
			DateTime? DefaultValue, DateTime? Min, DateTime? Max)
			: base(Page, Name, Label, Description, Required, RestrictToOptions, Options)
		{
			this.DefaultValue = DefaultValue;
			this.Min = Min;
			this.Max = Max;
		}

		/// <summary>
		/// Default parameter value.
		/// </summary>
		public DateTime? DefaultValue { get; }

        /// <summary>
        /// Optional minimum value allowed.
        /// </summary>
        public DateTime? Min { get; }

        /// <summary>
        /// Optional maximum value allowed.
        /// </summary>
        public DateTime? Max { get; }

        /// <summary>
        /// Populates a data form with parameters for the object.
        /// </summary>
        /// <param name="Parameters">Data form to host all editable parameters.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="Value">Value for parameter.</param>
        public override Task PopulateForm(DataForm Parameters, Language Language, object Value)
        {
            ValidationMethod Validation;
            Field Field;

            if (this.Min.HasValue || this.Max.HasValue)
            {
                Validation = new RangeValidation(
                    this.Min.HasValue ? XML.Encode(this.Min.Value, true) : null,
                    this.Max.HasValue ? XML.Encode(this.Max.Value, true) : null);
            }
            else
                Validation = new BasicValidation();

            if (this.RestrictToOptions)
            {
                Field = new ListSingleField(Parameters, this.Name, this.Label, this.Required,
                    new string[] { this.DefaultValue.HasValue ? XML.Encode(this.DefaultValue.Value, true) : string.Empty },
                    this.GetOptionTags(), this.Description, DateDataType.Instance, Validation, string.Empty, false, false, false);
            }
            else
            {
                Field = new TextSingleField(Parameters, this.Name, this.Label, this.Required,
                    new string[] { this.DefaultValue.HasValue ? XML.Encode(this.DefaultValue.Value, true) : string.Empty },
                    this.GetOptionTags(), this.Description, DateDataType.Instance, Validation, string.Empty, false, false, false);
            }

            Parameters.Add(Field);

            Page Page = Parameters.GetPage(this.Page);
            Page.Add(Field);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the parameters of the object, based on contents in the data form.
        /// </summary>
        /// <param name="Parameters">Data form with parameter values.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
        /// <param name="Values">Collection of parameter values.</param>
        /// <param name="Result">Result set to return to caller.</param>
        /// <returns>Any errors encountered, or null if parameters was set properly.</returns>
        public override async Task SetParameter(DataForm Parameters, Language Language, bool OnlySetChanged, Variables Values,
            SetEditableFormResult Result)
        {
            Field Field = Parameters[this.Name];
            if (Field is null)
            {
                if (this.Required)
                    Result.AddError(this.Name, await Language.GetStringAsync(typeof(ReportFileNode), 1, "Required parameter."));

                Values[this.Name] = null;
            }
            else
            {
                string s = Field.ValueString;

                if (string.IsNullOrEmpty(s))
                {
                    if (this.Required)
                        Result.AddError(this.Name, await Language.GetStringAsync(typeof(ReportFileNode), 1, "Required parameter."));

                    Values[this.Name] = null;
                }
                else if (XML.TryParse(s, out DateTime Parsed))
                {
                    Values[this.Name] = Parsed.Date;

                    if (Parsed.TimeOfDay != TimeSpan.Zero)
                        Result.AddError(this.Name, await Language.GetStringAsync(typeof(ReportFileNode), 3, "Only date acceptable."));
                }
                else
                    Result.AddError(this.Name, await Language.GetStringAsync(typeof(ReportFileNode), 2, "Invalid value."));
            }
        }

    }
}