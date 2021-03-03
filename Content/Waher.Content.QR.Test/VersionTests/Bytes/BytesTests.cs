using System;
using System.Text;
using Waher.Content.QR.Encoding;

namespace Waher.Content.QR.Test.VersionTests.Bytes
{
	public abstract class BytesTests : VersionTests
	{
		public override EncodingMode Mode => EncodingMode.Byte;
		public override string Folder => "Bytes";

		private const string LoremIpsum = 
			"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed eget fringilla quam, condimentum finibus enim. Aenean facilisis quam cursus tortor venenatis, quis luctus diam efficitur. Etiam et dolor vitae nisi congue mattis. In facilisis risus ac velit fringilla tristique. Aliquam non eleifend lectus. Aenean vehicula, nisi lobortis consectetur aliquet, diam ipsum vulputate dolor, eu varius lectus tellus non mauris. Mauris hendrerit, ante in malesuada luctus, ante neque tincidunt lectus, non aliquet mi ante quis risus. Aenean lacinia quam eget odio varius, vitae luctus urna fringilla. Nam vel mi at ante tincidunt hendrerit sit amet a lorem. Quisque semper in ante a mattis.\r\n" +
			"\r\n" +
			"Suspendisse potenti. Nunc tempus vitae velit et consectetur. Curabitur at nunc at sapien tempor maximus non a neque. Quisque in eleifend nunc. Phasellus nec quam accumsan, cursus arcu at, vestibulum purus. Nunc sed vehicula libero. Nullam in ligula a ante elementum lobortis. Praesent euismod nisi vitae ex consectetur viverra.\r\n" +
			"\r\n" +
			"Ut luctus, dui ut pharetra gravida, orci est tempus elit, a sagittis dui libero eget velit. Vestibulum consequat orci eu nibh rutrum facilisis. Vivamus imperdiet facilisis pulvinar. Nulla et vestibulum orci. Nam elementum tempor cursus. Orci varius natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Aenean quis ex sodales, consectetur nisl at, placerat nunc. In iaculis, arcu vel facilisis ultrices, est diam consequat nibh, id dapibus tellus ipsum in mauris. Phasellus dictum dapibus neque ut consequat. Donec vel mauris vel nisl imperdiet vestibulum a ut nibh. Sed luctus nulla id augue ornare, a malesuada urna vehicula.\r\n" +
			"\r\n" +
			"Nam imperdiet scelerisque leo, et sodales urna aliquam sed. Phasellus ultricies nulla semper risus ullamcorper suscipit. Vestibulum id laoreet nisl, sit amet efficitur quam. In dictum erat id ante eleifend, at consectetur felis mollis. Morbi tincidunt hendrerit sodales. Proin sed egestas arcu. Integer dignissim posuere varius. Cras massa nisl, mollis eu viverra sit amet, vestibulum eget lacus.\r\n" +
			"\r\n" +
			"Quisque ac vehicula massa, sed interdum eros. Ut finibus erat eros, ut accumsan elit euismod vitae. Etiam mollis, erat in porta dapibus, metus est dignissim erat, nec dictum justo dui ut est. Proin urna ex, condimentum quis varius ac, interdum posuere mi. Donec augue arcu, laoreet non justo sed, commodo dignissim mauris. Curabitur sit amet vulputate dui, a suscipit quam. Aliquam sem est, pretium eget massa vel, mollis vestibulum augue. Morbi hendrerit, tellus eu viverra cursus, lectus nisi rutrum leo, eget porttitor velit purus a nibh. Donec tempus posuere magna sed mollis. Interdum et malesuada fames ac ante ipsum primis in faucibus. Sed egestas finibus augue, et feugiat justo posuere ac. Vivamus feugiat in ligula cursus egestas. Aliquam sagittis elit non vehicula placerat. Duis egestas, magna vel tincidunt ornare, tortor erat pretium metus egestas.";

		public override string GetMessage(int ForVersion)
		{
			VersionInfo Version = Versions.FindVersionInfo(ForVersion, this.Level);
			int NrBytes = Version.TotalDataBytes - 5;
			return LoremIpsum.Substring(0, NrBytes);
		}
	}
}
