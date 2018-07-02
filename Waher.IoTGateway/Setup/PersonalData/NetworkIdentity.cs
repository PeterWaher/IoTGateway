using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.IoTGateway.Setup.PersonalData
{
	/// <summary>
	/// Information about the connection to the Internet
	/// </summary>
	public class NetworkIdentity : IProcessingActivity
	{
		/// <summary>
		/// Priority of the processing activity. When the transparent information about all processing activities is assembled,
		/// they are presented in ascending priority order.
		/// </summary>
		public int Priority => 200;

		/// <summary>
		/// Filename of transparent information markdown for the processing activity.
		/// </summary>
		public string TransparentInformationMarkdownFileName => "NetworkIdentity.md";
	}
}
