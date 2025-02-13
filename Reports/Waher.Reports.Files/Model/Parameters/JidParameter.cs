using System;
using System.Threading.Tasks;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Networking.XMPP.DataForms.ValidationMethods;
using Waher.Runtime.Language;
using Waher.Script;

namespace Waher.Reports.Files.Model.Parameters
{
    /// <summary>
    /// Represents a JID-valued parameter.
    /// </summary>
    public class JidParameter : ReportParameterWithOptions
	{
        private string defaultValue;

		/// <summary>
		/// Represents a JID-valued parameter.
		/// </summary>
		/// <param name="Page">Parameter Page</param>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Label">Parameter label.</param>
		/// <param name="Description">Parameter description.</param>
		/// <param name="Required">If parameter is required.</param>
		/// <param name="RestrictToOptions">If only values defined in options are valid values.</param>
		/// <param name="Options">Available options</param>
		/// <param name="DefaultValue">Default value of parameter.</param>
		public JidParameter(string Page, string Name, string Label, string Description,
			bool Required, bool RestrictToOptions, ParameterOption[] Options,
			string DefaultValue)
			: base(Page, Name, Label, Description, Required, RestrictToOptions, Options)
		{
			this.DefaultValue = DefaultValue;
		}

        /// <summary>
        /// Default parameter value.
        /// </summary>
        public string DefaultValue
        {
            get => this.defaultValue;
            set
            {
                if (!string.IsNullOrEmpty(value) && !XmppClient.BareJidRegEx.IsMatch(value))
                    throw new NotSupportedException("Invalid JID.");

                this.defaultValue = value;
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
            Field Field;

            if (this.RestrictToOptions)
            {
                Field = new ListSingleField(Parameters, this.Name, this.Label, this.Required,
                    new string[] { this.DefaultValue }, this.GetOptionTags(), this.Description, null, new BasicValidation(),
                    string.Empty, false, false, false);
            }
            else
            {
                Field = new JidSingleField(Parameters, this.Name, this.Label, this.Required,
                    new string[] { this.DefaultValue }, this.GetOptionTags(), this.Description, null, new BasicValidation(),
                    string.Empty, false, false, false);
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
                Values[this.Name] = s;

                if (string.IsNullOrEmpty(s))
                {
                    if (this.Required)
                        Result.AddError(this.Name, await Language.GetStringAsync(typeof(ReportFileNode), 1, "Required parameter."));

                    Values[this.Name] = null;
                }
                else if (!XmppClient.BareJidRegEx.IsMatch(s))
                    Result.AddError(this.Name, await Language.GetStringAsync(typeof(ReportFileNode), 4, "Invalid JID."));
            }
        }

    }
}