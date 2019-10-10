using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// C# export state
	/// </summary>
	public class CSharpExportState
	{
		/// <summary>
		/// C# export settings.
		/// </summary>
		public CSharpExportSettings Settings;

		internal int ExportingValuesIndent = 0;
		internal bool ExportingValues = false;

		/// <summary>
		/// C# export state
		/// </summary>
		/// <param name="Settings">Export settings.</param>
		public CSharpExportState(CSharpExportSettings Settings)
		{
			this.Settings = Settings;
		}

		/// <summary>
		/// Close pending actions
		/// </summary>
		/// <param name="Output">C# Output</param>
		public void ClosePending(StringBuilder Output)
		{
			this.ClosePendingValues(Output);
		}

		/// <summary>
		/// Close pending value actions
		/// </summary>
		/// <param name="Output">C# Output</param>
		public void ClosePendingValues(StringBuilder Output)
		{
			if (this.ExportingValues)
			{
				this.ExportingValues = false;

				Output.Append(Model.Asn1Node.Tabs(this.ExportingValuesIndent - 1));
				Output.AppendLine("}");
				Output.AppendLine();
			}
		}
	}
}
