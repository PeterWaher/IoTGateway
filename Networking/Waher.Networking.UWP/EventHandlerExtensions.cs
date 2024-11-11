using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;

namespace Waher.Networking
{
	#region Delegates

	/// <summary>
	/// Asynchronous version of <see cref="EventArgs"/>.
	/// </summary>
	public delegate Task EventHandlerAsync(object Sender, EventArgs e);

	/// <summary>
	/// Asynchronous version of <see cref="EventArgs"/> with a typed event arguments.
	/// </summary>
	public delegate Task EventHandlerAsync<T>(object Sender, T e);

	/// <summary>
	/// Callback function with one argument.
	/// </summary>
	/// <typeparam name="ArgT">Argument type.</typeparam>
	/// <param name="Arg">Argument</param>
	public delegate void Callback<ArgT>(ArgT Arg);

	/// <summary>
	/// Asynchronous callback function with one argument.
	/// </summary>
	/// <typeparam name="ArgT">Argument type.</typeparam>
	/// <param name="Arg">Argument</param>
	public delegate Task CallbackAsync<ArgT>(ArgT Arg);

	#endregion

	/// <summary>
	/// Static class with method extensions simplifying raising events.
	/// </summary>
	public static class EventHandlerExtensions
	{
		#region Synchronous event handlers, non-communication-layers

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static bool Raise(this EventHandler EventHandler, object Sender)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty, false);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="Decoupled">If the event is decoupled, i.e. executed
		/// in parallel with the source that raised it.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static bool Raise(this EventHandler EventHandler, object Sender, bool Decoupled)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty, Decoupled);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static bool Raise(this EventHandler EventHandler, object Sender, EventArgs e)
		{
			return EventHandler.Raise(Sender, e, false);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <param name="Decoupled">If the event is decoupled, i.e. executed
		/// in parallel with the source that raised it.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static bool Raise(this EventHandler EventHandler, object Sender, EventArgs e, bool Decoupled)
		{
			if (!(EventHandler is null))
			{
				if (Decoupled)
				{
					Task.Run(() =>
					{
						try
						{
							EventHandler(Sender, e);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					});
				}
				else
				{
					try
					{
						EventHandler(Sender, e);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static bool Raise<T>(this EventHandler<T> EventHandler, object Sender, T e)
		{
			return EventHandler.Raise(Sender, e, false);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <param name="Decoupled">If the event is decoupled, i.e. executed
		/// in parallel with the source that raised it.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static bool Raise<T>(this EventHandler<T> EventHandler, object Sender, T e, bool Decoupled)
		{
			if (!(EventHandler is null))
			{
				if (Decoupled)
				{
					Task.Run(() =>
					{
						try
						{
							EventHandler(Sender, e);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					});
				}
				else
				{
					try
					{
						EventHandler(Sender, e);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
						return false;
					}
				}
			}

			return true;
		}

		#endregion

		#region Synchronous event handlers, communication layers

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandler EventHandler, ICommunicationLayer Sender)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty, Sender?.DecoupledEvents ?? false);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="Decoupled">If the event is decoupled, i.e. executed
		/// in parallel with the source that raised it.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandler EventHandler, ICommunicationLayer Sender, bool Decoupled)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty, Decoupled);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandler EventHandler, ICommunicationLayer Sender, EventArgs e)
		{
			return EventHandler.Raise(Sender, e, Sender?.DecoupledEvents ?? false);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <param name="Decoupled">If the event is decoupled, i.e. executed
		/// in parallel with the source that raised it.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise(this EventHandler EventHandler, ICommunicationLayer Sender, EventArgs e, bool Decoupled)
		{
			if (EventHandler is null)
				await Sender.NoEventHandlerWarning(e);
			else if (Decoupled)
			{
				Task _ = Task.Run(async () =>
				{
					try
					{
						EventHandler(Sender, e);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);

						if (!(Sender is null))
						{
							try
							{
								await Sender.Exception(ex);
							}
							catch (Exception ex2)
							{
								Log.Exception(ex2);
							}
						}
					}
				});

				return true;
			}
			else
			{
				try
				{
					EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);

					if (!(Sender is null))
						await Sender.Exception(ex);

					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Shows a warning in sniffers that an event handler is not registered.
		/// </summary>
		/// <param name="Sender">Sender of event.</param>
		/// <param name="e">Event arguments.</param>
		private static Task NoEventHandlerWarning(this ICommunicationLayer Sender, object e)
		{
			if (Sender is null)
				return Task.CompletedTask;

			StringBuilder sb = new StringBuilder();

			sb.Append("No event handler registered (");

			if (Sender is null)
				sb.Append("null sender");
			else
				sb.Append(Sender.GetType().FullName);

			sb.Append(", ");
			sb.Append(e.GetType().FullName);
			sb.Append(")");

			return Sender.Warning(sb.ToString());
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise<T>(this EventHandler<T> EventHandler, ICommunicationLayer Sender, T e)
		{
			return EventHandler.Raise(Sender, e, Sender?.DecoupledEvents ?? false);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <param name="Decoupled">If the event is decoupled, i.e. executed
		/// in parallel with the source that raised it.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise<T>(this EventHandler<T> EventHandler, ICommunicationLayer Sender, T e, bool Decoupled)
		{
			if (EventHandler is null)
				await Sender.NoEventHandlerWarning(e);
			else if (Decoupled)
			{
				Task _ = Task.Run(async () =>
				{
					try
					{
						EventHandler(Sender, e);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);

						if (!(Sender is null))
						{
							try
							{
								await Sender.Exception(ex);
							}
							catch (Exception ex2)
							{
								Log.Exception(ex2);
							}
						}
					}
				});

				return true;
			}
			else
			{
				try
				{
					EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);

					if (!(Sender is null))
						await Sender.Exception(ex);

					return false;
				}
			}

			return true;
		}

		#endregion

		#region Asynchronous event handlers, non-communication-layers

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandlerAsync EventHandler, object Sender)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty, false);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="Decoupled">If the event is decoupled, i.e. executed
		/// in parallel with the source that raised it.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandlerAsync EventHandler, object Sender, bool Decoupled)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty, Decoupled);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandlerAsync EventHandler, object Sender, EventArgs e)
		{
			return EventHandler.Raise(Sender, e, false);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <param name="Decoupled">If the event is decoupled, i.e. executed
		/// in parallel with the source that raised it.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise(this EventHandlerAsync EventHandler, object Sender, EventArgs e, bool Decoupled)
		{
			if (!(EventHandler is null))
			{
				if (Decoupled)
				{
					Task _ = Task.Run(async () =>
					{
						try
						{
							await EventHandler(Sender, e);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					});
				}
				else
				{
					try
					{
						await EventHandler(Sender, e);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise<T>(this EventHandlerAsync<T> EventHandler, object Sender, T e)
		{
			return EventHandler.Raise(Sender, e, false);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <param name="Decoupled">If the event is decoupled, i.e. executed
		/// in parallel with the source that raised it.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise<T>(this EventHandlerAsync<T> EventHandler, object Sender, T e, bool Decoupled)
		{
			if (!(EventHandler is null))
			{
				if (Decoupled)
				{
					Task _ = Task.Run(async () =>
					{
						try
						{
							await EventHandler(Sender, e);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
					});
				}
				else
				{
					try
					{
						await EventHandler(Sender, e);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
						return false;
					}
				}
			}

			return true;
		}

		#endregion

		#region Asynchronous event handlers, communication layers

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandlerAsync EventHandler, ICommunicationLayer Sender)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty, Sender?.DecoupledEvents ?? false);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="Decoupled">If the event is decoupled, i.e. executed
		/// in parallel with the source that raised it.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandlerAsync EventHandler, ICommunicationLayer Sender, bool Decoupled)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty, Decoupled);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandlerAsync EventHandler, ICommunicationLayer Sender, EventArgs e)
		{
			return EventHandler.Raise(Sender, e, Sender?.DecoupledEvents ?? false);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <param name="Decoupled">If the event is decoupled, i.e. executed
		/// in parallel with the source that raised it.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise(this EventHandlerAsync EventHandler, ICommunicationLayer Sender, EventArgs e, bool Decoupled)
		{
			if (EventHandler is null)
				await Sender.NoEventHandlerWarning(e);
			else if (Decoupled)
			{
				Task _ = Task.Run(async () =>
				{
					try
					{
						await EventHandler(Sender, e);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);

						if (!(Sender is null))
						{
							try
							{
								await Sender.Exception(ex);
							}
							catch (Exception ex2)
							{
								Log.Exception(ex2);
							}
						}
					}
				});

				return true;
			}
			else
			{
				try
				{
					await EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);

					if (!(Sender is null))
						await Sender.Exception(ex);

					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise<T>(this EventHandlerAsync<T> EventHandler, ICommunicationLayer Sender, T e)
		{
			return EventHandler.Raise(Sender, e, Sender?.DecoupledEvents ?? false);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <param name="Decoupled">If the event is decoupled, i.e. executed
		/// in parallel with the source that raised it.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise<T>(this EventHandlerAsync<T> EventHandler, ICommunicationLayer Sender, T e, bool Decoupled)
		{
			if (EventHandler is null)
				await Sender.NoEventHandlerWarning(e);
			else if (Decoupled)
			{
				Task _ = Task.Run(async () =>
				{
					try
					{
						await EventHandler(Sender, e);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);

						if (!(Sender is null))
						{
							try
							{
								await Sender.Exception(ex);
							}
							catch (Exception ex2)
							{
								Log.Exception(ex2);
							}
						}
					}
				});

				return true;
			}
			else
			{
				try
				{
					await EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);

					if (!(Sender is null))
						await Sender.Exception(ex);

					return false;
				}
			}

			return true;
		}

		#endregion

		#region Synchronous callback functions

		/// <summary>
		/// Calls a callback function. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="Callback">Callback function, or null if not defined.</param>
		/// <param name="Argument">Argument</param>
		public static void Call<ArgT>(this Callback<ArgT> Callback, ArgT Argument)
		{
			if (!(Callback is null))
			{
				try
				{
					Callback(Argument);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		#endregion

		#region Asynchronous callback functions

		/// <summary>
		/// Calls a callback function. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="Callback">Callback function, or null if not defined.</param>
		/// <param name="Argument">Argument</param>
		public static async Task Call<ArgT>(this CallbackAsync<ArgT> Callback, ArgT Argument)
		{
			if (!(Callback is null))
			{
				try
				{
					await Callback(Argument);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		#endregion
	}
}
