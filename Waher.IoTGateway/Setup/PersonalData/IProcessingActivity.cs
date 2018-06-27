using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.IoTGateway.Setup.PersonalData
{
	/// <summary>
	/// Interface for Personal Data Processing Activities
	/// </summary>
    public interface IProcessingActivity
    {
		/// <summary>
		/// Priority of the processing activity. When the transparent information about all processing activities is assembled,
		/// they are presented in ascending priority order.
		/// </summary>
		int Priority
		{
			get;
		}

		/// <summary>
		/// Filename of transparent information markdown for the processing activity.
		/// </summary>
		string TransparentInformationMarkdownFileName
		{
			get;
		}
    }
}
