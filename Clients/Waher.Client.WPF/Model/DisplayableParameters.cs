using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Things.DisplayableParameters;

namespace Waher.Client.WPF.Model
{
	public class DisplayableParameters
	{
		private Dictionary<string, object> parameters = new Dictionary<string, object>();
		private Parameter[] ordered;

		public DisplayableParameters(Parameter[] Parameters)
		{
			this.ordered = Parameters;

			foreach (Parameter P in Parameters)
				this.parameters[P.Id] = P.UntypedValue;
		}

		public Parameter[] Ordered
		{
			get { return this.ordered; }
		}

		public string this[string ParameterId]
		{
			get
			{
				if (this.parameters.TryGetValue(ParameterId, out object Value))
				{
					if (Value == null)
						return string.Empty;
					else
						return Value.ToString();
				}
				else
					return string.Empty;
			}
		}
	}
}
