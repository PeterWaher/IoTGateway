using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things.ControlParameters;
using Waher.Things;

namespace Waher.Networking.XMPP.Control.ControlOperations
{
	/// <summary>
	/// Date control operation.
	/// </summary>
	public class DateControlOperation : ControlOperation
	{
		private DateControlParameter parameter;
		private DateTime value;

		/// <summary>
		/// Date control operation.
		/// </summary>
		/// <param name="Node">Node on which operation is to be performed.</param>
		/// <param name="Parameter">Control parameter.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="request">Original request.</param>
		public DateControlOperation(ThingReference Node, DateControlParameter Parameter, DateTime Value, IqEventArgs request)
			: base(Node, request, Parameter)
		{
			this.parameter = Parameter;
			this.value = Value;
		}

		/// <summary>
		/// Control parameter
		/// </summary>
		public DateControlParameter Parameter
		{
			get { return this.parameter; }
		}

		/// <summary>
		/// Value to set.
		/// </summary>
		public DateTime Value
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
