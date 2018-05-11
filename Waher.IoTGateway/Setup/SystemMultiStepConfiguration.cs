using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Abstract base class for multi-step system configurations.
	/// </summary>
	public abstract class SystemMultiStepConfiguration : SystemConfiguration
	{
		private int step = 0;

		/// <summary>
		/// Configuration step.
		/// </summary>
		[DefaultValue(0)]
		public int Step
		{
			get { return this.step; }
			set { this.step = value; }
		}
	}
}
