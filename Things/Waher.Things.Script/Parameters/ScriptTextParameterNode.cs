using System;
using System.Threading.Tasks;
using Waher.Content;
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
    /// Represents a text-valued script parameter.
    /// </summary>
    public class ScriptTextParameterNode : ScriptParameterNodeWithOptions
    {
        private string contentType;

        /// <summary>
        /// Represents a text-valued script parameter.
        /// </summary>
        public ScriptTextParameterNode()
            : base()
        {
        }

        /// <summary>
        /// Internet Content-Type of text value.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(63, "Content-Type:")]
        [ToolTip(64, "Content-Type of text.")]
        [Required]
        public string ContentType
        {
            get => this.contentType;
            set
            {
                if (!string.IsNullOrEmpty(value) && (
                    !InternetContent.IsAccepted(value, InternetContent.CanDecodeContentTypes) ||
                    !InternetContent.IsAccepted(value, InternetContent.CanEncodeContentTypes)))
                {
                    throw new NotSupportedException("Content-Type not supported.");
                }

                this.contentType = value;
            }
        }

        /// <summary>
        /// Default parameter value.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(29, "Default value:")]
        [ToolTip(30, "Default value presented to user.")]
        [DynamicContentType("GetContentType")]
        public string[] DefaultValue { get; set; }

        /// <summary>
        /// Minimum amount of JIDs expected.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(68, "Minimum Count:")]
        [ToolTip(69, "The smallest amount of items accepted.")]
        public ushort? MinCount { get; set; }

        /// <summary>
        /// Maximum amount of JIDs expected.
        /// </summary>
        [Page(2, "Script", 100)]
        [Header(70, "Maximum Count:")]
        [ToolTip(71, "The largest amount of items accepted.")]
        public ushort? MaxCount { get; set; }

        /// <summary>
        /// Gets the Content-Type of the text value of the parameter.
        /// </summary>
        /// <returns>Internet Content-Type.</returns>
        public string GetContentType()
        {
            return string.IsNullOrEmpty(this.contentType) ? PlainTextCodec.DefaultContentType : this.contentType;
        }

        /// <summary>
        /// Gets the type name of the node.
        /// </summary>
        /// <param name="Language">Language to use.</param>
        /// <returns>Localized type node.</returns>
        public override Task<string> GetTypeNameAsync(Language Language)
        {
            return Language.GetStringAsync(typeof(ScriptNode), 65, "Text-valued parameter");
        }

        /// <summary>
        /// Populates a data form with parameters for the object.
        /// </summary>
        /// <param name="Parameters">Data form to host all editable parameters.</param>
        /// <param name="Language">Current language.</param>
        /// <param name="Value">Value for parameter.</param>
        public override async Task PopulateForm(DataForm Parameters, Language Language, object Value)
        {
            ValidationMethod Validation = new BasicValidation();
            Field Field;

            if (this.MinCount.HasValue || this.MaxCount.HasValue)
                Validation = new ListRangeValidation(Validation, this.MinCount ?? 0, this.MaxCount ?? ushort.MaxValue);

            if (this.RestrictToOptions)
            {
                Field = new ListMultiField(Parameters, this.ParameterName, this.Label, this.Required,
                    this.DefaultValue, await this.GetOptions(), this.Description, StringDataType.Instance, Validation, string.Empty,
                    false, false, false);
            }
            else
            {
                Field = new TextMultiField(Parameters, this.ParameterName, this.Label, this.Required,
                    this.DefaultValue, await this.GetOptions(), this.Description, StringDataType.Instance, Validation, string.Empty,
                    false, false, false, this.GetContentType());
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
                string[] s = Field.ValueStrings;

                if (s is null || s.Length == 0)
                {
                    if (this.Required)
                        Result.AddError(this.ParameterName, await Language.GetStringAsync(typeof(ScriptNode), 42, "Required parameter."));

                    Values[this.ParameterName] = null;
                }
                else
                {
                    Values[this.ParameterName] = s;

                    if (this.MinCount.HasValue && s.Length < this.MinCount.Value)
                        Result.AddError(this.ParameterName, await Language.GetStringAsync(typeof(ScriptNode), 72, "Too few rows."));
                    else if (this.MaxCount.HasValue && s.Length > this.MaxCount.Value)
                        Result.AddError(this.ParameterName, await Language.GetStringAsync(typeof(ScriptNode), 73, "Too many rows."));
                }
            }
        }

    }
}