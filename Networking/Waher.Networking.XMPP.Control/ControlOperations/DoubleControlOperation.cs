using System.Threading.Tasks;
using Waher.Things.ControlParameters;
using Waher.Things;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Control.ControlOperations
{
	/// <summary>
	/// Double control operation.
	/// </summary>
	public class DoubleControlOperation : ControlOperation
	{
		private readonly DoubleControlParameter parameter;
		private readonly double value;

		/// <summary>
		/// Double control operation.
		/// </summary>
		/// <param name="Node">Node on which operation is to be performed.</param>
		/// <param name="Parameter">Control parameter.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="request">Original request.</param>
		public DoubleControlOperation(IThingReference Node, DoubleControlParameter Parameter, double Value, IqEventArgs request)
			: base(Node, request, Parameter)
		{
			this.parameter = Parameter;
			this.value = Value;
		}

		/// <summary>
		/// Control parameter
		/// </summary>
		public DoubleControlParameter Parameter => this.parameter;

		/// <summary>
		/// Value to set.
		/// </summary>
		public double Value => this.value;

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
