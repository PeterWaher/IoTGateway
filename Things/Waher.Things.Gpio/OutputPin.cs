using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Language;

namespace Waher.Things.Gpio
{
	public class OutputPin : Pin
	{
		public OutputPin()
			: base()
		{
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Controller), 6, "Output Pin");
		}
	}
}
