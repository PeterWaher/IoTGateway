using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things.ControlParameters;
using Waher.Things;

namespace Waher.Networking.XMPP.Control.ControlOperations
{
	/// <summary>
	/// Color control operation.
	/// </summary>
	public class ColorControlOperation : ControlOperation
	{
		private readonly ColorControlParameter parameter;
		private readonly string value;

		/// <summary>
		/// Color control operation.
		/// </summary>
		/// <param name="Node">Node on which operation is to be performed.</param>
		/// <param name="Parameter">Control parameter.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="request">Original request.</param>
		public ColorControlOperation(IThingReference Node, ColorControlParameter Parameter, string Value, IqEventArgs request)
			: base(Node, request, Parameter)
		{
			this.parameter = Parameter;
			this.value = Value;
		}

		/// <summary>
		/// Control parameter
		/// </summary>
		public ColorControlParameter Parameter
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
		public override async Task<bool> Set()
		{
			bool Result = await this.parameter.SetStringValue(this.Node, this.value);

			if (!Result)
				ControlServer.ParameterSyntaxError(this.parameter.Name, this.Request);

			return Result;
		}
	}
}
