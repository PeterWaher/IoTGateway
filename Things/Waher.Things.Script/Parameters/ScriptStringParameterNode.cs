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
using Waher.Things.Attributes;

namespace Waher.Things.Script.Parameters
{
    /// <summary>
    /// Represents a string-valued script parameter.
    /// </summary>
    public class ScriptStringParameterNode : ScriptParameterNode
    {
        private string pattern = string.Empty;
        private Regex parsed = null;

        /// <summary>
        /// Represents a string-valued script parameter.
        /// </summary>
        public ScriptStringParameterNode()
            : base()
        {
        }

        /// <summary>
        /// Default parameter value.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(29, "Default value:")]
        [ToolTip(30, "Default value presented to user.")]
        public string DefaultValue { get; set; }

        /// <summary>
        /// Optional regular expression used for validating user input.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(31, "Pattern:")]
        [ToolTip(32, "Regular expression used for validating user input.")]
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
        /// Gets the type name of the node.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        /// <returns>Localized type node.</returns>
        public override Task<string> GetTypeNameAsync(Language Language)
        {
            return Language.GetStringAsync(typeof(ScriptNode), 35, "String-valued parameter");
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

            if (string.IsNullOrEmpty(this.Pattern))
                Validation = new OpenValidation();
            else
                Validation = new RegexValidation(this.Pattern);

            TextSingleField Field = new TextSingleField(Parameters, this.ParameterName, this.Label, this.Required,
                new string[] { this.DefaultValue }, null, this.Description, StringDataType.Instance,
                Validation, string.Empty, false, false, false);

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
            Field Field = Parameters[this.ParameterName];
            if (Field is null)
            {
                if (this.Required)
                    Result.AddError(this.ParameterName, await Language.GetStringAsync(typeof(ScriptNode), 42, "Required parameter."));

                Values[this.ParameterName] = null;
            }
            else
            {
                string s = Field.ValueString;
                Values[this.ParameterName] = s;

                if (!(this.parsed is null))
                {
                    Match M = this.parsed.Match(s);

                    if (!M.Success || M.Index > 0 || M.Length != s.Length)
                        Result.AddError(this.ParameterName, await Language.GetStringAsync(typeof(ScriptNode), 43, "Value does not match expected pattern."));
                }
            }
        }

    }
}