using System;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.Matter.Controller;
using Waher.Things.Matter.Onboarding;
using Waher.Things.Queries;

namespace Waher.Things.Matter.Commands
{
	/// <summary>
	/// Opens a commissioning window on a device, allowing another Matter administrator (ecosystem) to commission
	/// the device into its fabric (multi-admin). The pairing codes to use are presented in a table.
	/// </summary>
	public class ShareDeviceCommand : MatterCommand
	{
		private readonly MatterDeviceNode device;
		private int minutes = 5;

		/// <summary>
		/// Opens a commissioning window on a device, allowing another Matter administrator (ecosystem) to commission
		/// the device into its fabric (multi-admin).
		/// </summary>
		/// <param name="Device">Device node.</param>
		public ShareDeviceCommand(MatterDeviceNode Device)
			: base(Device, "Matter.Share", CommandType.Query, "7")
		{
			this.device = Device;
		}

		/// <summary>
		/// Number of minutes the commissioning window should be open.
		/// </summary>
		[Page(16, "Commissioning Window")]
		[Header(17, "Minutes:")]
		[ToolTip(18, "Number of minutes the commissioning window should be open (3-15 minutes).")]
		[Range(3, 15)]
		[Required]
		public int Minutes
		{
			get => this.minutes;
			set => this.minutes = value;
		}

		/// <inheritdoc/>
		public override ICommand Copy()
		{
			return new ShareDeviceCommand(this.device)
			{
				minutes = this.minutes
			};
		}

		/// <inheritdoc/>
		public override Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 354, "Share with another ecosystem...");
		}

		/// <inheritdoc/>
		public override Task<string> GetFailureStringAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(MatterControllerNode), 355, "Unable to open commissioning window.");
		}

		/// <inheritdoc/>
		public override Task StartQueryExecutionAsync(Query Query, Language Language)
		{
			int Minutes = this.minutes;
			Task.Run(() => this.Execute(Query, Language, Minutes));
			return Task.CompletedTask;
		}

		/// <summary>
		/// Executes the query.
		/// </summary>
		/// <param name="Query">Query.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Minutes">Number of minutes the window should be open.</param>
		public async Task Execute(Query Query, Language Language, int Minutes)
		{
			try
			{
				await Query.Start();
				await Query.SetTitle(await Language.GetStringAsync(typeof(MatterControllerNode), 356, "Commissioning window"));
				await Query.SetStatus(await Language.GetStringAsync(typeof(MatterControllerNode), 357, "Opening commissioning window..."));

				Minutes = Math.Max(3, Math.Min(15, Minutes));

				MatterNode Node = await this.device.GetMatterNodeAsync();
				SetupPayload Payload = await Node.OpenCommissioningWindowAsync(TimeSpan.FromMinutes(Minutes));
				DateTime Expires = DateTime.Now.AddMinutes(Minutes);

				await Query.NewTable("Window", await Language.GetStringAsync(typeof(MatterControllerNode), 356, "Commissioning window"),
					new Column("Parameter", await Language.GetStringAsync(typeof(MatterControllerNode), 358, "Parameter"), null, null, null, null, ColumnAlignment.Left, null),
					new Column("Value", await Language.GetStringAsync(typeof(MatterControllerNode), 343, "Value"), null, null, null, null, ColumnAlignment.Left, null));

				await Query.NewRecords("Window",
					new Record(await Language.GetStringAsync(typeof(MatterControllerNode), 359, "Manual pairing code"), SetupPayload.FormatManualCode(Payload.ToManualCode())),
					new Record(await Language.GetStringAsync(typeof(MatterControllerNode), 360, "QR code payload"), Payload.ToQrCode()),
					new Record(await Language.GetStringAsync(typeof(MatterControllerNode), 315, "Discriminator"), Payload.Discriminator.ToString()),
					new Record(await Language.GetStringAsync(typeof(MatterControllerNode), 361, "Valid until"), Expires));

				await Query.TableDone("Window");
				await Query.SetStatus(string.Empty);

				await this.device.LogInformationAsync("Commissioning", "Commissioning window opened for " + Minutes.ToString() + " minutes.");
			}
			catch (Exception ex)
			{
				await Query.LogMessage(ex);
				Log.Exception(ex);
			}
			finally
			{
				await Query.Done();
			}
		}
	}
}
