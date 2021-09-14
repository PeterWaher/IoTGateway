using System;
using System.Threading.Tasks;

namespace Waher.Networking.XMPP.DataForms
{
	/// <summary>
	/// Class used to convert callback methods of type <see cref="DataFormResultEventHandler"/> to callback methods of
	/// type <see cref="DataFormEventHandler"/>.
	/// </summary>
	public class FormResultToFormCallback
	{
		private readonly DataFormEventHandler callback;

		/// <summary>
		/// Class used to convert callback methods of type <see cref="DataFormResultEventHandler"/> to callback methods of
		/// type <see cref="DataFormEventHandler"/>.
		/// </summary>
		/// <param name="FormCallback">Final callback method.</param>
		public FormResultToFormCallback(DataFormEventHandler FormCallback)
		{
			this.callback = FormCallback;
		}

		/// <summary>
		/// Callback method of type <see cref="DataFormResultEventHandler"/>
		/// </summary>
		/// <param name="Sender">Sender of event.</param>
		/// <param name="e">Event arguments.</param>
		public Task Callback(object Sender, DataFormEventArgs e)
		{
			if (e.Ok && !(e.Form is null))
				return this.callback(Sender, e.Form);
			else
				return Task.CompletedTask;
		}
	}
}
