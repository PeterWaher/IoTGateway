﻿using System.Threading.Tasks;
using Waher.Things.ControlParameters;
using Waher.Things;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Control.ControlOperations
{
	/// <summary>
	/// String control operation.
	/// </summary>
	public class StringControlOperation : ControlOperation
	{
		private readonly StringControlParameter parameter;
		private readonly string value;

		/// <summary>
		/// String control operation.
		/// </summary>
		/// <param name="Node">Node on which operation is to be performed.</param>
		/// <param name="Parameter">Control parameter.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="request">Original request.</param>
		public StringControlOperation(IThingReference Node, StringControlParameter Parameter, string Value, IqEventArgs request)
			: base(Node, request, Parameter)
		{
			this.parameter = Parameter;
			this.value = Value;
		}

		/// <summary>
		/// Control parameter
		/// </summary>
		public StringControlParameter Parameter => this.parameter;

		/// <summary>
		/// Value to set.
		/// </summary>
		public string Value => this.value;

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
