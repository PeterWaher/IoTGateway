using System.Collections.Generic;
using System.Xml;

namespace Waher.Reports.Files.Model.Actions
{
	/// <summary>
	/// Defines conditional execution in a report.
	/// </summary>
	public class Conditional : ReportAction
	{
		private readonly Else otherwise;
		private readonly If[] conditions;

		/// <summary>
		/// Defines conditional execution in a report.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Report">Report</param>
		public Conditional(XmlElement Xml, ReportFile Report)
			: base(Report)
		{
			List<If> Conditions = new List<If>();

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				switch (E.LocalName)
				{
					case "If":
						Conditions.Add(new If(E, Report));
						break;

					case "Else":
						this.otherwise = new Else(E, Report);
						break;

				}
			}

			this.conditions = Conditions.ToArray();
		}
	}
}
