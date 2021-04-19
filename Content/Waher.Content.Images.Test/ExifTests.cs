using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content.Images.Exif;

namespace Waher.Content.Images.Test
{
	[TestClass]
	public class ExifTests
	{
		private static string[] jpgFiles = new string[]
		{
			@"jpg\WWL_(Polaroid)_ION230.jpg",
			@"jpg\Canon_40D.jpg",
			@"jpg\Canon_40D_photoshop_import.jpg",
			@"jpg\Canon_DIGITAL_IXUS_400.jpg",
			@"jpg\Canon_PowerShot_S40.jpg",
			@"jpg\corrupted.jpg",
			@"jpg\Fujifilm_FinePix_E500.jpg",
			@"jpg\Fujifilm_FinePix6900ZOOM.jpg",
			@"jpg\Kodak_CX7530.jpg",
			@"jpg\Konica_Minolta_DiMAGE_Z3.jpg",
			@"jpg\long_description.jpg",
			@"jpg\Nikon_COOLPIX_P1.jpg",
			@"jpg\Nikon_D70.jpg",
			@"jpg\Olympus_C8080WZ.jpg",
			@"jpg\PaintTool_sample.jpg",
			@"jpg\Panasonic_DMC-FZ30.jpg",
			@"jpg\Pentax_K10D.jpg",
			@"jpg\Reconyx_HC500_Hyperfire.jpg",
			@"jpg\Ricoh_Caplio_RR330.jpg",
			@"jpg\Samsung_Digimax_i50_MP3.jpg",
			@"jpg\Sony_HDR-HC3.jpg",
			@"jpg\exif-org\fujifilm-dx10.jpg",
			@"jpg\exif-org\fujifilm-finepix40i.jpg",
			@"jpg\exif-org\fujifilm-mx1700.jpg",
			@"jpg\exif-org\kodak-dc210.jpg",
			@"jpg\exif-org\kodak-dc240.jpg",
			@"jpg\exif-org\nikon-e950.jpg",
			@"jpg\exif-org\olympus-c960.jpg",
			@"jpg\exif-org\olympus-d320l.jpg",
			@"jpg\exif-org\ricoh-rdc5300.jpg",
			@"jpg\exif-org\sanyo-vpcg250.jpg",
			@"jpg\exif-org\sanyo-vpcsx550.jpg",
			@"jpg\exif-org\sony-cybershot.jpg",
			@"jpg\exif-org\sony-d700.jpg",
			@"jpg\exif-org\sony-powershota5.jpg",
			@"jpg\exif-org\canon-ixus.jpg",
			@"jpg\gps\DSCN0010.jpg",
			@"jpg\gps\DSCN0012.jpg",
			@"jpg\gps\DSCN0021.jpg",
			@"jpg\gps\DSCN0025.jpg",
			@"jpg\gps\DSCN0027.jpg",
			@"jpg\gps\DSCN0029.jpg",
			@"jpg\gps\DSCN0038.jpg",
			@"jpg\gps\DSCN0040.jpg",
			@"jpg\gps\DSCN0042.jpg",
			@"jpg\hdr\canon_hdr_YES.jpg",
			@"jpg\hdr\iphone_hdr_NO.jpg",
			@"jpg\hdr\iphone_hdr_YES.jpg",
			@"jpg\hdr\canon_hdr_NO.jpg",
			@"jpg\invalid\image01088.jpg",
			@"jpg\invalid\image01137.jpg",
			@"jpg\invalid\image01551.jpg",
			@"jpg\invalid\image01713.jpg",
			@"jpg\invalid\image01980.jpg",
			@"jpg\invalid\image02206.jpg",
			@"jpg\invalid\image00971.jpg",
			@"jpg\mobile\jolla.jpg",
			@"jpg\orientation\portrait_5.jpg",
			@"jpg\orientation\portrait_6.jpg",
			@"jpg\orientation\portrait_7.jpg",
			@"jpg\orientation\portrait_8.jpg",
			@"jpg\orientation\landscape_1.jpg",
			@"jpg\orientation\landscape_2.jpg",
			@"jpg\orientation\landscape_3.jpg",
			@"jpg\orientation\landscape_4.jpg",
			@"jpg\orientation\landscape_5.jpg",
			@"jpg\orientation\landscape_6.jpg",
			@"jpg\orientation\landscape_7.jpg",
			@"jpg\orientation\landscape_8.jpg",
			@"jpg\orientation\portrait_1.jpg",
			@"jpg\orientation\portrait_2.jpg",
			@"jpg\orientation\portrait_3.jpg",
			@"jpg\orientation\portrait_4.jpg",
			@"jpg\tests\87_OSError.jpg",
			@"jpg\tests\11-tests.jpg",
			@"jpg\tests\22-canon_tags.jpg",
			@"jpg\tests\28-hex_value.jpg",
			@"jpg\tests\30-type_error.jpg",
			@"jpg\tests\32-lens_data.jpeg",
			@"jpg\tests\33-type_error.jpg",
			@"jpg\tests\35-empty.jpg",
			@"jpg\tests\36-memory_error.jpg",
			@"jpg\tests\42_IndexError.jpg",
			@"jpg\tests\45-gps_ifd.jpg",
			@"jpg\tests\46_UnicodeEncodeError.jpg",
			@"jpg\tests\67-0_length_string.jpg",
			@"jpg\xmp\no_exif.jpg",
			@"jpg\xmp\BlueSquare.jpg",
		};

		// Sample images used from this repository, downloaded in parallel to the IoT Gateway repository:
		// https://github.com/ianare/exif-samples

		[TestMethod]
		public void Test_JPEG_01()
		{
			this.TestImage(jpgFiles, 01);
		}

		[TestMethod]
		public void Test_JPEG_02()
		{
			this.TestImage(jpgFiles, 02);
		}

		[TestMethod]
		public void Test_JPEG_03()
		{
			this.TestImage(jpgFiles, 03);
		}

		[TestMethod]
		public void Test_JPEG_04()
		{
			this.TestImage(jpgFiles, 04);
		}

		[TestMethod]
		public void Test_JPEG_05()
		{
			this.TestImage(jpgFiles, 05);
		}

		[TestMethod]
		public void Test_JPEG_06()
		{
			this.TestImage(jpgFiles, 06);
		}

		[TestMethod]
		public void Test_JPEG_07()
		{
			this.TestImage(jpgFiles, 07);
		}

		[TestMethod]
		public void Test_JPEG_08()
		{
			this.TestImage(jpgFiles, 08);
		}

		[TestMethod]
		public void Test_JPEG_09()
		{
			this.TestImage(jpgFiles, 09);
		}

		[TestMethod]
		public void Test_JPEG_10()
		{
			this.TestImage(jpgFiles, 10);
		}

		[TestMethod]
		public void Test_JPEG_11()
		{
			this.TestImage(jpgFiles, 11);
		}

		[TestMethod]
		public void Test_JPEG_12()
		{
			this.TestImage(jpgFiles, 12);
		}

		[TestMethod]
		public void Test_JPEG_13()
		{
			this.TestImage(jpgFiles, 13);
		}

		[TestMethod]
		public void Test_JPEG_14()
		{
			this.TestImage(jpgFiles, 14);
		}

		[TestMethod]
		public void Test_JPEG_15()
		{
			this.TestImage(jpgFiles, 15);
		}

		[TestMethod]
		public void Test_JPEG_16()
		{
			this.TestImage(jpgFiles, 16);
		}

		[TestMethod]
		public void Test_JPEG_17()
		{
			this.TestImage(jpgFiles, 17);
		}

		[TestMethod]
		public void Test_JPEG_18()
		{
			this.TestImage(jpgFiles, 18);
		}

		[TestMethod]
		public void Test_JPEG_19()
		{
			this.TestImage(jpgFiles, 19);
		}

		[TestMethod]
		public void Test_JPEG_20()
		{
			this.TestImage(jpgFiles, 20);
		}

		[TestMethod]
		public void Test_JPEG_21()
		{
			this.TestImage(jpgFiles, 21);
		}

		[TestMethod]
		public void Test_JPEG_22()
		{
			this.TestImage(jpgFiles, 22);
		}

		[TestMethod]
		public void Test_JPEG_23()
		{
			this.TestImage(jpgFiles, 23);
		}

		[TestMethod]
		public void Test_JPEG_24()
		{
			this.TestImage(jpgFiles, 24);
		}

		[TestMethod]
		public void Test_JPEG_25()
		{
			this.TestImage(jpgFiles, 25);
		}

		[TestMethod]
		public void Test_JPEG_26()
		{
			this.TestImage(jpgFiles, 26);
		}

		[TestMethod]
		public void Test_JPEG_27()
		{
			this.TestImage(jpgFiles, 27);
		}

		[TestMethod]
		public void Test_JPEG_28()
		{
			this.TestImage(jpgFiles, 28);
		}

		[TestMethod]
		public void Test_JPEG_29()
		{
			this.TestImage(jpgFiles, 29);
		}

		[TestMethod]
		public void Test_JPEG_30()
		{
			this.TestImage(jpgFiles, 30);
		}

		[TestMethod]
		public void Test_JPEG_31()
		{
			this.TestImage(jpgFiles, 31);
		}

		[TestMethod]
		public void Test_JPEG_32()
		{
			this.TestImage(jpgFiles, 32);
		}

		[TestMethod]
		public void Test_JPEG_33()
		{
			this.TestImage(jpgFiles, 33);
		}

		[TestMethod]
		public void Test_JPEG_34()
		{
			this.TestImage(jpgFiles, 34);
		}

		[TestMethod]
		public void Test_JPEG_35()
		{
			this.TestImage(jpgFiles, 35);
		}

		[TestMethod]
		public void Test_JPEG_36()
		{
			this.TestImage(jpgFiles, 36);
		}

		[TestMethod]
		public void Test_JPEG_37()
		{
			this.TestImage(jpgFiles, 37);
		}

		[TestMethod]
		public void Test_JPEG_38()
		{
			this.TestImage(jpgFiles, 38);
		}

		[TestMethod]
		public void Test_JPEG_39()
		{
			this.TestImage(jpgFiles, 39);
		}

		[TestMethod]
		public void Test_JPEG_40()
		{
			this.TestImage(jpgFiles, 40);
		}

		[TestMethod]
		public void Test_JPEG_41()
		{
			this.TestImage(jpgFiles, 41);
		}

		[TestMethod]
		public void Test_JPEG_42()
		{
			this.TestImage(jpgFiles, 42);
		}

		[TestMethod]
		public void Test_JPEG_43()
		{
			this.TestImage(jpgFiles, 43);
		}

		[TestMethod]
		public void Test_JPEG_44()
		{
			this.TestImage(jpgFiles, 44);
		}

		[TestMethod]
		public void Test_JPEG_45()
		{
			this.TestImage(jpgFiles, 45);
		}

		[TestMethod]
		public void Test_JPEG_46()
		{
			this.TestImage(jpgFiles, 46);
		}

		[TestMethod]
		public void Test_JPEG_47()
		{
			this.TestImage(jpgFiles, 47);
		}

		[TestMethod]
		public void Test_JPEG_48()
		{
			this.TestImage(jpgFiles, 48);
		}

		[TestMethod]
		public void Test_JPEG_49()
		{
			this.TestImage(jpgFiles, 49);
		}

		[TestMethod]
		public void Test_JPEG_50()
		{
			this.TestImage(jpgFiles, 50);
		}

		[TestMethod]
		public void Test_JPEG_51()
		{
			this.TestImage(jpgFiles, 51);
		}

		[TestMethod]
		public void Test_JPEG_52()
		{
			this.TestImage(jpgFiles, 52);
		}

		[TestMethod]
		public void Test_JPEG_53()
		{
			this.TestImage(jpgFiles, 53);
		}

		[TestMethod]
		public void Test_JPEG_54()
		{
			this.TestImage(jpgFiles, 54);
		}

		[TestMethod]
		public void Test_JPEG_55()
		{
			this.TestImage(jpgFiles, 55);
		}

		[TestMethod]
		public void Test_JPEG_56()
		{
			this.TestImage(jpgFiles, 56);
		}

		[TestMethod]
		public void Test_JPEG_57()
		{
			this.TestImage(jpgFiles, 57);
		}

		[TestMethod]
		public void Test_JPEG_58()
		{
			this.TestImage(jpgFiles, 58);
		}

		[TestMethod]
		public void Test_JPEG_59()
		{
			this.TestImage(jpgFiles, 59);
		}

		[TestMethod]
		public void Test_JPEG_60()
		{
			this.TestImage(jpgFiles, 60);
		}

		[TestMethod]
		public void Test_JPEG_61()
		{
			this.TestImage(jpgFiles, 61);
		}

		[TestMethod]
		public void Test_JPEG_62()
		{
			this.TestImage(jpgFiles, 62);
		}

		[TestMethod]
		public void Test_JPEG_63()
		{
			this.TestImage(jpgFiles, 63);
		}

		[TestMethod]
		public void Test_JPEG_64()
		{
			this.TestImage(jpgFiles, 64);
		}

		[TestMethod]
		public void Test_JPEG_65()
		{
			this.TestImage(jpgFiles, 65);
		}

		[TestMethod]
		public void Test_JPEG_66()
		{
			this.TestImage(jpgFiles, 66);
		}

		[TestMethod]
		public void Test_JPEG_67()
		{
			this.TestImage(jpgFiles, 67);
		}

		[TestMethod]
		public void Test_JPEG_68()
		{
			this.TestImage(jpgFiles, 68);
		}

		[TestMethod]
		public void Test_JPEG_69()
		{
			this.TestImage(jpgFiles, 69);
		}

		[TestMethod]
		public void Test_JPEG_70()
		{
			this.TestImage(jpgFiles, 70);
		}

		[TestMethod]
		public void Test_JPEG_71()
		{
			this.TestImage(jpgFiles, 71);
		}

		[TestMethod]
		public void Test_JPEG_72()
		{
			this.TestImage(jpgFiles, 72);
		}

		[TestMethod]
		public void Test_JPEG_73()
		{
			this.TestImage(jpgFiles, 73);
		}

		[TestMethod]
		public void Test_JPEG_74()
		{
			this.TestImage(jpgFiles, 74);
		}

		[TestMethod]
		public void Test_JPEG_75()
		{
			this.TestImage(jpgFiles, 75);
		}

		[TestMethod]
		public void Test_JPEG_76()
		{
			this.TestImage(jpgFiles, 76);
		}

		[TestMethod]
		public void Test_JPEG_77()
		{
			this.TestImage(jpgFiles, 77);
		}

		[TestMethod]
		public void Test_JPEG_78()
		{
			this.TestImage(jpgFiles, 78);
		}

		[TestMethod]
		public void Test_JPEG_79()
		{
			this.TestImage(jpgFiles, 79);
		}

		[TestMethod]
		public void Test_JPEG_80()
		{
			this.TestImage(jpgFiles, 80);
		}

		[TestMethod]
		public void Test_JPEG_81()
		{
			this.TestImage(jpgFiles, 81);
		}

		[TestMethod]
		public void Test_JPEG_82()
		{
			this.TestImage(jpgFiles, 82);
		}

		[TestMethod]
		public void Test_JPEG_83()
		{
			this.TestImage(jpgFiles, 83);
		}

		[TestMethod]
		public void Test_JPEG_84()
		{
			this.TestImage(jpgFiles, 84);
		}

		[TestMethod]
		public void Test_JPEG_85()
		{
			this.TestImage(jpgFiles, 85);
		}

		[TestMethod]
		public void Test_JPEG_86()
		{
			this.TestImage(jpgFiles, 86);
		}

		[TestMethod]
		public void Test_JPEG_87()
		{
			this.TestImage(jpgFiles, 87);
		}

		[TestMethod]
		public void Test_JPEG_88()
		{
			this.TestImage(jpgFiles, 88);
		}

		private void TestImage(string[] Array, int Index)
		{
			string FileName = @"..\..\..\..\..\..\exif-samples-master\" + Array[Index - 1];
			this.TestImage(FileName);
		}

		private void TestImage(string FileName)
		{
			Assert.IsTrue(EXIF.TryExtractFromJPeg(FileName, out ExifTag[] Tags));
			this.Print(Tags);
		}

		private void Print(params ExifTag[] Tags)
		{
			foreach (ExifTag Tag in Tags)
			{
				string s = Tag.ToString();
				int i = s.IndexOf('=');

				if (i > 0)
					s = s.Substring(0, i) + "\t" + s.Substring(i + 1);

				Console.Out.WriteLine(s);
			}
		}
	}
}
