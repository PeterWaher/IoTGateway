using System.Threading.Tasks;
using Waher.Content;
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
    /// Represents a Boolean-valued parameter.
    /// </summary>
    public class BooleanParameter : ReportParameter
    {
		/// <summary>
		/// Represents a Boolean-valued parameter.
		/// </summary>
		/// <param name="Page">Parameter Page</param>
		/// <param name="Name">Parameter name.</param>
		/// <param name="Label">Parameter label.</param>
		/// <param name="Description">Parameter description.</param>
		/// <param name="Required">If parameter is required.</param>
        /// <param name="DefaultValue">Default value of parameter.</param>
		public BooleanParameter(string Page, string Name, string Label, string Description,
			bool Required, bool? DefaultValue)
            : base(Page, Name, Label, Description, Required)
        {
            this.DefaultValue = DefaultValue;
		}

        /// <summary>
        /// Default parameter value.
        /// </summary>
        public bool? DefaultValue { get; }

        /// <summary>
        /// Populates a data form with parameters for the object.
        /// </summary>
        /// <param name="Parameters">Data form to host all editable parameters.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="Value">Value for parameter.</param>
        public override Task PopulateForm(DataForm Parameters, Language Language, object Value)
        {
            BooleanField Field = new BooleanField(Parameters, this.Name, this.Label, this.Required,
                new string[] { this.DefaultValue.HasValue ? CommonTypes.Encode(this.DefaultValue.Value) : string.Empty }, 
                null, this.Description, BooleanDataType.Instance, new BasicValidation(), string.Empty, false, false, false);

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
                else if (CommonTypes.TryParse(s, out bool Parsed))
                    Values[this.Name] = Parsed;
                else
					Result.AddError(this.Name, await Language.GetStringAsync(typeof(ReportFileNode), 2, "Invalid value."));
            }
        }

    }
}