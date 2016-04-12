using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Networking.XMPP.Control.ParameterTypes;
using Waher.Things;

namespace Waher.Networking.XMPP.Control.ControlOperations
{
	/// <summary>
	/// String control operation.
	/// </summary>
	public class StringControlOperation : ControlOperation
	{
		private StringControlParameter parameter;
		private string value;

		/// <summary>
		/// String control operation.
		/// </summary>
		/// <param name="Node">Node on which operation is to be performed.</param>
		/// <param name="Parameter">Control parameter.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="request">Original request.</param>
		public StringControlOperation(ThingReference Node, StringControlParameter Parameter, string Value, IqEventArgs request)
			: base(Node, request, Parameter)
		{
			this.parameter = Parameter;
			this.value = Value;
		}

		/// <summary>
		/// Control parameter
		/// </summary>
		public StringControlParameter Parameter
		{
			get { return this.parameter; }
		}

		/// <summary>
		/// Value to set.
		/// </summary>
		public string Value
		{
			get { return this.value; }
		}

		/// <summary>
		/// Performs the control operation.
		/// </summary>
		/// <returns>If the operation was successful or not.</returns>
		public override bool Set()
		{
			bool Result = this.parameter.Set(this.Node, this.value);

			if (!Result)
				ControlServer.ParameterValueInvalid(this.parameter.Name, this.Request);

			return Result;
		}
	}
}
