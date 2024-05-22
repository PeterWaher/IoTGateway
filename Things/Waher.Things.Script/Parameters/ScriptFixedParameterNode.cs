using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Content.Text;
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
    /// Represents a fixed-valued script parameter.
    /// </summary>
    public class ScriptFixedParameterNode : ScriptParameterNode
    {
        /// <summary>
        /// Represents a fixed-valued script parameter.
        /// </summary>
        public ScriptFixedParameterNode()
            : base()
        {
        }

        /// <summary>
        /// Parameter value.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(75, "Value:")]
        [ToolTip(76, "Value presented to user.")]
        [ContentType(PlainTextCodec.DefaultContentType)]
        public string[] Value { get; set; }

        /// <summary>
        /// Gets the type name of the node.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        /// <returns>Localized type node.</returns>
        public override Task<string> GetTypeNameAsync(Language Language)
        {
            return Language.GetStringAsync(typeof(ScriptNode), 74, "Fixed parameter");
        }

        /// <summary>
        /// Populates a data form with parameters for the object.
        /// </summary>
        /// <param name="Parameters">Data form to host all editable parameters.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="Value">Value for parameter.</param>
        public override Task PopulateForm(DataForm Parameters, Language Language, object Value)
        {
            FixedField Field = new FixedField(Parameters, this.ParameterName, this.Label, this.Required,
                this.Value, null, this.Description, null, null, string.Empty, false, false, false);

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
        public override Task SetParameter(DataForm Parameters, Language Language, bool OnlySetChanged, Variables Values,
            SetEditableFormResult Result)
        {
            Values[this.ParameterName] = this.Value;
            return Task.CompletedTask;
        }

    }
}