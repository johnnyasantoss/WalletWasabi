using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;
using WalletWasabi.Helpers;

namespace WalletWasabi.Hwi.Models
{
	public class HardwareWalletInfo
	{
		public HardwareWalletInfo(string masterFingerprint, string serialNumber, HardwareWalletType type, string path, string error)
		{
			try
			{
				Guard.NotNullOrEmptyOrWhitespace(nameof(masterFingerprint), masterFingerprint);
				var masterFingerPrintBytes = ByteHelpers.FromHex(masterFingerprint);
				MasterFingerprint = new HDFingerprint(masterFingerPrintBytes);
			}
			catch (ArgumentException)
			{
				MasterFingerprint = null;
			}

			SerialNumber = serialNumber;
			Type = type;
			Path = path;
			Error = error;

			if (Error != null && Error.Contains("Not initialized", StringComparison.InvariantCultureIgnoreCase))
			{
				Initialized = false;
			}
			else
			{
				Initialized = true;
			}
		}

		public HDFingerprint? MasterFingerprint { get; }
		public bool Initialized { get; }
		public string SerialNumber { get; }
		public HardwareWalletType Type { get; }
		public string Path { get; }
		public string Error { get; }
	}
}