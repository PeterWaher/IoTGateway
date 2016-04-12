using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Things;

namespace Waher.Networking.XMPP.Control.ControlOperations
{
	/// <summary>
	/// Abstract base class for control operations.
	/// </summary>
	public abstract class ControlOperation
	{
		private ThingReference node;
		private IqEventArgs request;
		private string parameterName;

		/// <summary>
		/// Abstract base class for control operations.
		/// </summary>
		/// <param name="Node">Node on which operation is to be performed.</param>
		/// <param name="Request">Original request.</param>
		/// <param name="Parameter">Control parameter.</param>
		public ControlOperation(ThingReference Node, IqEventArgs Request, ControlParameter Parameter)
		{
			this.node = Node;
			this.request = Request;
			this.parameterName = Parameter.Name;
		}

		/// <summary>
		/// Node on which operation is to be performed.
		/// </summary>
		public ThingReference Node
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
		public abstract bool Set();
	}
}
