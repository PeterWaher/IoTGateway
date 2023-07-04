using System;
using System.Collections.Generic;

namespace Waher.IoTGateway.Setup
{
	public partial class XmppConfiguration
	{
		private readonly static Dictionary<string, KeyValuePair<string, string>> clp = new Dictionary<string, KeyValuePair<string, string>>(StringComparer.CurrentCultureIgnoreCase)
		{
			{ "af.id.tagroot.io", new KeyValuePair<string, string>("9659f902acbfba4c2260c44e3002c5b1bb1a9d52598ccea89a8ce934380c672a", "66cdbf350d62032a1691805abad22701bb4ca4f50aaccd1b9c85675d8dd51039") },
			{ "build.tagroot.io", new KeyValuePair<string, string>("2e767cc2f694b90c7b02a6f1e944e49e6ba8946e5b29dd6e5dc0df7c45cf8035", "f995e67ab81c14474b381146264edb062ea7d950b4ca9c20394ba25d3dfa6125") },
			{ "cybercity.online", new KeyValuePair<string, string>("8f8a822b7303652994aa605bfe728f597c7e50bba8f58664a4f388e26ff5a95c", "5dd8657843c99e6542f5dacc0981429f5d420f092bfbfff1bd7115621fa0b9d6") },
			{ "eu.id.tagroot.io", new KeyValuePair<string, string>("e15d672ca49ca0834a11755f0dc85557396ed7d4f2e56445bba90f4cba073f45", "90849e59431e4d4a8fd573be5491dd1fcb250cf79b73c4bb2935348813728e8c") },
			{ "fi.tagroot.io", new KeyValuePair<string, string>("4dddd944f55505c094ee2cc36b9ace3305020698b1b21f259efd149ef8f390e7", "9b1ba73058d68ed4ac35f7c4bab975e446f9bf842cf39d2e46a43ee99bbd5c98") },
			{ "folketsrost.online", new KeyValuePair<string, string>("bcbb78b2b1790b447f47baab53342a26171db2ac6c4ba21d848066caafe6464d", "c2747fe30cc792768b8714b0b6dc35d1f99e4fe1bd407a6766098d90d7b9f0ea") },
			{ "id.tagroot.io", new KeyValuePair<string, string>("acaca0df44d8e8a239149145e435a2cf0f7ca0668b581ae216a7838e038d45bb", "cf947069d81dc8732c09cf4270176d4294b3599793147f7f2ce49d49113280d1") },
			{ "lab.tagroot.io", new KeyValuePair<string, string>("c654a5b4f421a6e591daeddb6d0cf09e7fda1163574095bd5ae7a02e5e431e6d", "27f689cd03404c0f4a2b8b36d6ac0e1c0cf3d828d8f2be73a0282b0fa0a33d18") },
			{ "neuron.itudigital.sp.gov.br", new KeyValuePair<string, string>("e6a917204cfc9f0c02cb83f07d3a142e5b0ffe6bc4ba1f71017d3e79b5550234", "45341791dca2d6bce2cc59100868caba5e63f823ab24b0467137a80a5e08fb7f") },
			{ "paiwise.tagroot.io", new KeyValuePair<string, string>("72006bac116fcb17f0a5f05c07ce6de405eb0a5c6ad2a85f06ae822a5d556b75", "03771e78821e0d0c9f9a9cd868d32a89c92d1434d628a05e66099f79e7a16781") },
			{ "sa.id.tagroot.io", new KeyValuePair<string, string>("3d8d23cc92ef31b0b256be937bc9b0aecb33a2b5c84be3d5447df350a77a9f48", "f6a0e2ec3980dc1def9e6eecff146dbd7194b8b5281396241c26acca129a5f5c") },
			{ "tagroot.io", new KeyValuePair<string, string>("ccfea335817e2ba3002ce4fa849a0bbc4646cb1eed8709f5841e6d64b6adeed5", "3ecaaf06af86d808a978ea75a0b6ce58ffd176d9b5da979d5691d176a48cb95f") },
			{ "quickhub.app", new KeyValuePair<string, string>("4cdf919ec16d7b134a2a4dd21f1c76738cab16afbea66d80a0ba9f98123ad9df", "d00b3b048f1e2dca18ed49c032938218506d77727cc648bfc144181d7c792ab4") },
			{ "techpronobis.com", new KeyValuePair<string, string>("9111c1fb80542c017211d30eef22652c6d65a93a4bbc558b048e12e2382d75fb", "c2bc0f1d5798585ab1ef78d03237960dbd3a49e5897fd4ff6b556beb20ff3290") },
			{ "waher.se", new KeyValuePair<string, string>("732cdb097eebe75d9bbde0ed6e3cc56aef9fa16d14f0b35d559ed17c52a350af", "f163bf86936ab8787215682f7962b04cffec15880250abd927057cf088fd83e4") }
		};

		/// <summary>
		/// Date when solution was built.
		/// </summary>
		public static readonly string BuildDate = "2023-07-04";

		/// <summary>
		/// Time when solution was built.
		/// </summary>
		public static readonly string BuildTime = "18:08:38";
	}
}
