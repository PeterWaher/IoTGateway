﻿using System.Threading.Tasks;
using Waher.Content;
using Waher.Things.ControlParameters;
using Waher.Things;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Control.ControlOperations
{
	/// <summary>
	/// Duration control operation.
	/// </summary>
	public class DurationControlOperation : ControlOperation
	{
		private readonly DurationControlParameter parameter;
		private readonly Duration value;

		/// <summary>
		/// Duration control operation.
		/// </summary>
		/// <param name="Node">Node on which operation is to be performed.</param>
		/// <param name="Parameter">Control parameter.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="request">Original request.</param>
		public DurationControlOperation(IThingReference Node, DurationControlParameter Parameter, Duration Value, IqEventArgs request)
			: base(Node, request, Parameter)
		{
			this.parameter = Parameter;
			this.value = Value;
		}

		/// <summary>
		/// Control parameter
		/// </summary>
		public DurationControlParameter Parameter => this.parameter;

		/// <summary>
		/// Value to set.
		/// </summary>
		public Duration Value => this.value;

		/// <summary>
		/// Performs the control operation.
		/// </summary>
		/// <returns>If the operation was successful or not.</returns>
		public override async Task<bool> Set()
		{
			bool Result = await this.parameter.Set(this.Node, this.value);

			if (!Result)
				await ControlServer.ParameterValueInvalid(this.parameter.Name, this.Request);

			return Result;
		}
	}
}
