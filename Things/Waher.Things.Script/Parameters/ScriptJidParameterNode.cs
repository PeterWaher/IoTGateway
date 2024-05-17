using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Networking.XMPP;
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
    /// Represents a JID-valued script parameter.
    /// </summary>
    public class ScriptJidParameterNode : ScriptParameterNode
    {
        private string defaultValue;

        /// <summary>
        /// Represents a JID-valued script parameter.
        /// </summary>
        public ScriptJidParameterNode()
            : base()
        {
        }

        /// <summary>
        /// Default parameter value.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(29, "Default value:")]
        [ToolTip(30, "Default value presented to user.")]
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
        /// Gets the type name of the node.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        /// <returns>Localized type node.</returns>
        public override Task<string> GetTypeNameAsync(Language Language)
        {
            return Language.GetStringAsync(typeof(ScriptNode), 66, "JID-valued parameter");
        }

        /// <summary>
        /// Populates a data form with parameters for the object.
        /// </summary>
        /// <param name="Parameters">Data form to host all editable parameters.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="Value">Value for parameter.</param>
        public override Task PopulateForm(DataForm Parameters, Language Language, object Value)
        {
            JidSingleField Field = new JidSingleField(Parameters, this.ParameterName, this.Label, this.Required,
                new string[] { this.DefaultValue }, null, this.Description, null, new BasicValidation(), string.Empty, false, false, false);

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

                if (string.IsNullOrEmpty(s))
                {
                    if (this.Required)
                        Result.AddError(this.ParameterName, await Language.GetStringAsync(typeof(ScriptNode), 42, "Required parameter."));

                    Values[this.ParameterName] = null;
                }
                else if (!XmppClient.BareJidRegEx.IsMatch(s))
                    Result.AddError(this.ParameterName, "Invalid JID.");
            }
        }

    }
}