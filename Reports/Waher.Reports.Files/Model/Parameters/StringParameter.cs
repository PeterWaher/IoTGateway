using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    /// Represents a string-valued parameter.
    /// </summary>
    public class StringParameter : ReportParameterWithOptions
	{
        private string pattern = string.Empty;
        private Regex parsed = null;

		/// <summary>
		/// Represents a string-valued parameter.
		/// </summary>
		/// <param name="Page">Parameter Page</param>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Label">Parameter label.</param>
		/// <param name="Description">Parameter description.</param>
		/// <param name="Required">If parameter is required.</param>
		/// <param name="RestrictToOptions">If only values defined in options are valid values.</param>
		/// <param name="Options">Available options</param>
		/// <param name="DefaultValue">Default value of parameter.</param>
        /// <param name="Pattern">Regular expression pattern for values.</param>
		public StringParameter(string Page, string Name, string Label, string Description,
			bool Required, bool RestrictToOptions, ParameterOption[] Options,
			string DefaultValue, string Pattern)
			: base(Page, Name, Label, Description, Required, RestrictToOptions, Options)
		{
			this.DefaultValue = DefaultValue;
			this.Pattern = Pattern;
		}

        /// <summary>
        /// Default parameter value.
        /// </summary>
        public string DefaultValue { get; }

        /// <summary>
        /// Optional regular expression used for validating user input.
        /// </summary>
        public string Pattern
        {
            get => this.pattern;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.pattern = null;
                    this.parsed = null;
                }
                else
                {
                    this.parsed = new Regex(value);
                    this.pattern = value;
                }
            }
        }

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

            if (string.IsNullOrEmpty(this.Pattern))
                Validation = new OpenValidation();
            else
                Validation = new RegexValidation(this.Pattern);

            if (this.RestrictToOptions)
            {
                Field = new ListSingleField(Parameters, this.Name, this.Label, this.Required,
                    new string[] { this.DefaultValue }, this.GetOptionTags(), this.Description, StringDataType.Instance,
                    Validation, string.Empty, false, false, false);
            }
            else
            {
                Field = new TextSingleField(Parameters, this.Name, this.Label, this.Required,
                    new string[] { this.DefaultValue }, this.GetOptionTags(), this.Description, StringDataType.Instance,
                    Validation, string.Empty, false, false, false);
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
                else
                {
                    Values[this.Name] = s;

                    if (!(this.parsed is null))
                    {
                        Match M = this.parsed.Match(s);

                        if (!M.Success || M.Index > 0 || M.Length != s.Length)
                            Result.AddError(this.Name, await Language.GetStringAsync(typeof(ReportFileNode), 8, "Value does not match expected pattern."));
                    }
                }
            }
        }

    }
}