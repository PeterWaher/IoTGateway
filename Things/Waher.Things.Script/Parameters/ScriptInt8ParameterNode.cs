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
    /// Represents a 8-bit integer-valued script parameter.
    /// </summary>
    public class ScriptInt8ParameterNode : ScriptParameterNodeWithOptions
    {
        /// <summary>
        /// Represents a 8-bit integer-valued script parameter.
        /// </summary>
        public ScriptInt8ParameterNode()
            : base()
        {
        }

        /// <summary>
        /// Default parameter value.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(29, "Default value:")]
        [ToolTip(30, "Default value presented to user.")]
        public sbyte? DefaultValue { get; set; }

        /// <summary>
        /// Optional minimum value allowed.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(44, "Minimum Value:")]
        [ToolTip(45, "The smallest value allowed.")]
        public sbyte? Min { get; set; }

        /// <summary>
        /// Optional maximum value allowed.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(46, "Maximum Value:")]
        [ToolTip(47, "The largest value allowed.")]
        public sbyte? Max { get; set; }

        /// <summary>
        /// Gets the type name of the node.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        /// <returns>Localized type node.</returns>
        public override Task<string> GetTypeNameAsync(Language Language)
        {
            return Language.GetStringAsync(typeof(ScriptNode), 52, "8-bit integer-valued parameter");
        }

        /// <summary>
        /// Populates a data form with parameters for the object.
        /// </summary>
        /// <param name="Parameters">Data form to host all editable parameters.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="Value">Value for parameter.</param>
        public override async Task PopulateForm(DataForm Parameters, Language Language, object Value)
        {
            ValidationMethod Validation;
            Field Field;

            if (this.Min.HasValue || this.Max.HasValue)
                Validation = new RangeValidation(this.Min?.ToString(), this.Max?.ToString());
            else
                Validation = new BasicValidation();

            if (this.RestrictToOptions)
            {
                Field = new ListSingleField(Parameters, this.ParameterName, this.Label, this.Required,
                    new string[] { this.DefaultValue?.ToString() ?? string.Empty }, await this.GetOptions(), this.Description,
                    ByteDataType.Instance, Validation, string.Empty, false, false, false);
            }
            else
            {
                Field = new TextSingleField(Parameters, this.ParameterName, this.Label, this.Required,
                    new string[] { this.DefaultValue?.ToString() ?? string.Empty }, await this.GetOptions(), this.Description,
                    ByteDataType.Instance, Validation, string.Empty, false, false, false);
            }

            Parameters.Add(Field);

            Page Page = Parameters.GetPage(this.Page);
            Page.Add(Field);
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

                if (string.IsNullOrEmpty(s))
                {
                    if (this.Required)
                        Result.AddError(this.ParameterName, await Language.GetStringAsync(typeof(ScriptNode), 42, "Required parameter."));

                    Values[this.ParameterName] = null;
                }
                else if (sbyte.TryParse(s, out sbyte Parsed))
                    Values[this.ParameterName] = Parsed;
                else
                    Result.AddError(this.ParameterName, await Language.GetStringAsync(typeof(ScriptNode), 49, "Invalid value."));
            }
        }

    }
}