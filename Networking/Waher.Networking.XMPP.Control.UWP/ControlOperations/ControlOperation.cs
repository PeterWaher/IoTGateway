using System;
using System.Threading.Tasks;
using Waher.Things;
using Waher.Things.ControlParameters;

namespace Waher.Networking.XMPP.Control.ControlOperations
{
	/// <summary>
	/// Abstract base class for control operations.
	/// </summary>
	public abstract class ControlOperation
	{
		private readonly IThingReference node;
		private readonly IqEventArgs request;
		private readonly string parameterName;

		/// <summary>
		/// Abstract base class for control operations.
		/// </summary>
		/// <param name="Node">Node on which operation is to be performed.</param>
		/// <param name="Request">Original request.</param>
		/// <param name="Parameter">Control parameter.</param>
		public ControlOperation(IThingReference Node, IqEventArgs Request, ControlParameter Parameter)
		{
			this.node = Node;
			this.request = Request;
			this.parameterName = Parameter.Name;
		}

		/// <summary>
		/// Node on which operation is to be performed.
		/// </summary>
		public IThingReference Node
		{
			get { return this.node; }
		}

		/// <summary>
		/// Original request.
		/// </summary>
		public IqEventArgs Request
		{
			get { return this.request; }
		}

		/// <summary>
		/// Control parameter name.
		/// </summary>
		public string ParameterName
		{
			get { return this.parameterName; }
		}

		/// <summary>
		/// Performs the control operation.
		/// </summary>
		/// <returns>If the operation was successful or not.</returns>
		public abstract Task<bool> Set();
	}
}
