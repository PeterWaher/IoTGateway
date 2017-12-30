using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Language;
using Waher.Things.ControlParameters;

namespace Waher.Things.Gpio
{
	public class OutputPin : Pin, IActuator
	{
		public OutputPin()
			: base()
		{
		}

		public ControlParameter[] GetControlParameters()
		{
			throw new NotImplementedException();
		}

		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(Controller), 6, "Output Pin");
		}
	}
}
