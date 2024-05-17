using System.Text.RegularExpressions;
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
using Waher.Things.Attributes;

namespace Waher.Things.Script.Parameters
{
    /// <summary>
    /// Represents a color-valued script parameter.
    /// </summary>
    public class ScriptColorParameterNode : ScriptParameterNodeWithOptions
    {
        /// <summary>
        /// Represents a color-valued script parameter.
        /// </summary>
        public ScriptColorParameterNode()
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
        /// If editing alpha-channel is enabled.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(61, "Alpha channel")]
        [ToolTip(62, "If alpha channel is enabled.")]
        public bool AlphaChannel { get; set; }

        /// <summary>
        /// Gets the type name of the node.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        /// <returns>Localized type node.</returns>
        public override Task<string> GetTypeNameAsync(Language Language)
        {
            return Language.GetStringAsync(typeof(ScriptNode), 60, "Color-valued parameter");
        }

        /// <summary>
        /// Populates a data form with parameters for the object.
        /// </summary>
        /// <param name="Parameters">Data form to host all editable parameters.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="Value">Value for parameter.</param>
        public override async Task PopulateForm(DataForm Parameters, Language Language, object Value)
        {
            DataType DataType = this.AlphaChannel ? (DataType)ColorAlphaDataType.Instance : (DataType)ColorDataType.Instance;

            TextSingleField Field = new TextSingleField(Parameters, this.ParameterName, this.Label, this.Required,
                new string[] { this.DefaultValue }, await this.GetOptions(), this.Description, DataType, new BasicValidation(), 
                string.Empty, false, false, false);

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
                else if (this.AlphaChannel && ColorAlphaDataType.TryParse(s, out ColorReference Parsed))
                    Values[this.ParameterName] = Parsed;
                else if (!this.AlphaChannel && ColorDataType.TryParse(s, out Parsed))
                    Values[this.ParameterName] = Parsed;
                else
                    Result.AddError(this.ParameterName, await Language.GetStringAsync(typeof(ScriptNode), 49, "Invalid value."));
            }
        }

    }
}