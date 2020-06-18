using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things.ControlParameters;
using Waher.Things;

namespace Waher.Networking.XMPP.Control.ControlOperations
{
	/// <summary>
	/// Enum control operation.
	/// </summary>
	public class EnumControlOperation : ControlOperation
	{
		private readonly EnumControlParameter parameter;
		private readonly Enum value;

		/// <summary>
		/// Enum control operation.
		/// </summary>
		/// <param name="Node">Node on which operation is to be performed.</param>
		/// <param name="Parameter">Control parameter.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="request">Original request.</param>
		public EnumControlOperation(IThingReference Node, EnumControlParameter Parameter, Enum Value, IqEventArgs request)
			: base(Node, request, Parameter)
		{
			this.parameter = Parameter;
			this.value = Value;
		}

		/// <summary>
		/// Control parameter
		/// </summary>
		public EnumControlParameter Parameter
		{
			get { return this.parameter; }
		}

		/// <summary>
		/// Value to set.
		/// </summary>
		public Enum Value
		{
			get { return this.value; }
		}

		/// <summary>
		/// Performs the control operation.
		/// </summary>
		/// <returns>If the operation was successful or not.</returns>
		public override async Task<bool> Set()
		{
			bool Result = await this.parameter.Set(this.Node, this.value);

			if (!Result)
				ControlServer.ParameterValueInvalid(this.parameter.Name, this.Request);

			return Result;
		}
	}
}
