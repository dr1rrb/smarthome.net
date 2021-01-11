using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartHomeDotNet.Computer
{
	internal class PowerHelper
	{
		public static async Task Sleep(CancellationToken ct)
			=> Process.Start("shutdown", "/h /f /t 0");

		public static async Task Off(CancellationToken ct)
			=> Process.Start("shutdown", "/s /f /t 0");
	}
}