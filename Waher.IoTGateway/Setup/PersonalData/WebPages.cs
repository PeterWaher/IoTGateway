using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.IoTGateway.Setup.PersonalData
{
	public class WebPages : IProcessingActivity
	{
		/// <summary>
		/// Priority of the processing activity. When the transparent information about all processing activities is assembled,
		/// they are presented in ascending priority order.
		/// </summary>
		public int Priority => 400;

		/// <summary>
		/// Filename of transparent information markdown for the processing activity.
		/// </summary>
		public string TransparentInformationMarkdownFileName => "WebPages.md";
	}
}
