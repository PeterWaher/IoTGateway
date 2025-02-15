using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Reports.Model.Attributes;
using Waher.Runtime.Language;
using Waher.Script;

namespace Waher.Reports.Files.Model.Parameters
{
    /// <summary>
    /// Represents a fixed-valued parameter.
    /// </summary>
    public class FixedParameter : ReportParameter
	{
        private readonly ReportStringAttribute[] value;
        private readonly int nrValues;

		/// <summary>
		/// Represents a fixed-valued parameter.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public FixedParameter(XmlElement Xml)
			: base(Xml)
		{
			List<ReportStringAttribute> Values = new List<ReportStringAttribute>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
                if (N is XmlElement E && E.LocalName == "Value")
                    Values.Add(new ReportStringAttribute(E, null));
			}

            this.value = Values.ToArray();
            this.nrValues = this.value.Length;
		}

		/// <summary>
		/// Populates a data form with parameters for the object.
		/// </summary>
		/// <param name="Parameters">Data form to host all editable parameters.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="Variables">Report variables.</param>
		public override async Task PopulateForm(DataForm Parameters, Language Language, Variables Variables)
		{
            ReportParameterAttributes Attributes = await this.GetReportParameterAttributes(Variables);

			FixedField Field = new FixedField(Parameters, Attributes.Name, Attributes.Label, Attributes.Required,
                await this.GetValue(Variables), null, Attributes.Description, null, null, string.Empty, 
				false, false, false);

            Parameters.Add(Field);

            Page Page = Parameters.GetPage(Attributes.Page);
            Page.Add(Field);
        }

		private async Task<string[]> GetValue(Variables Variables)
		{
			string[] Value = new string[this.nrValues];
			int i;

			for (i = 0; i < this.nrValues; i++)
				Value[i] = await this.value[i].Evaluate(Variables);

			return Value;
		}

		/// <summary>
		/// Sets the parameters of the object, based on contents in the data form.
		/// </summary>
		/// <param name="Parameters">Data form with parameter values.</param>
		/// <param name="Language">Current language.</param>
		/// <param name="OnlySetChanged">If only changed parameters are to be set.</param>
		/// <param name="Variables">Report variables.</param>
		/// <param name="Result">Result set to return to caller.</param>
		/// <returns>Any errors encountered, or null if parameters was set properly.</returns>
		public override async Task SetParameter(DataForm Parameters, Language Language, bool OnlySetChanged, Variables Variables,
			SetEditableFormResult Result)
		{
			string Name = await this.GetName(Variables);
			Variables[Name] = await this.GetValue(Variables);
        }

    }
}