using System;
using System.Collections.Generic;
using System.Text;
using Waher.IoTGateway.Setup.PersonalData;

namespace Waher.IoTGateway.Setup.PersonalData
{
	/// <summary>
	/// Information about backups.
	/// </summary>
	public class Backups : IProcessingActivity
	{
		/// <summary>
		/// Priority of the processing activity. When the transparent information about all processing activities is assembled,
		/// they are presented in ascending priority order.
		/// </summary>
		public int Priority => 600;

		/// <summary>
		/// Filename of transparent information markdown for the processing activity.
		/// </summary>
		public string TransparentInformationMarkdownFileName => "Backups.md";
	}
}
