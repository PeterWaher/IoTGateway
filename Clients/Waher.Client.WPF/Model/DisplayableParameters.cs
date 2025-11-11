using System.Collections;
using System.Collections.Generic;
using Waher.Things.DisplayableParameters;

namespace Waher.Client.WPF.Model
{
	public class DisplayableParameters : IEnumerable<Parameter>
	{
		private readonly Dictionary<string, Parameter> parameters = [];
		private Parameter[] ordered;

		public DisplayableParameters(Parameter[] Parameters)
		{
			this.ordered = Parameters;
			this.AddRange(Parameters);
		}

		public void Add(Parameter Parameter)
		{
			this.parameters[Parameter.Id] = Parameter;
			this.ordered = null;
		}

		public void AddRange(IEnumerable<Parameter> Parameters)
		{
			foreach (Parameter P in Parameters)
				this.parameters[P.Id] = P;

			this.ordered = null;
		}

		public void Clear()
		{
			this.parameters.Clear();
			this.ordered = null;
		}

		public IEnumerator<Parameter> GetEnumerator()
		{
			return this.parameters.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.parameters.Values.GetEnumerator();
		}

		public Parameter[] Ordered
		{
			get
			{
				if (this.ordered is null)
				{
					Parameter[] Ordered = new Parameter[this.parameters.Count];
					this.parameters.Values.CopyTo(Ordered, 0);
					this.ordered = Ordered;
				}

				return this.ordered;
			}
		}

		public string this[string ParameterId]
		{
			get
			{
				if (this.parameters.TryGetValue(ParameterId, out Parameter Parameter))
					return Parameter.UntypedValue?.ToString() ?? string.Empty;
				else
					return string.Empty;
			}
		}
	}
}
