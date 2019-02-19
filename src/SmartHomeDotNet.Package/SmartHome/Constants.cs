using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHomeDotNet.SmartHome
{
	internal static class Constants
	{
		/// <summary>
		/// Gets the default delay to wait before retry if a remote connection failed
		/// </summary>
		public static TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(10);
	}
}
