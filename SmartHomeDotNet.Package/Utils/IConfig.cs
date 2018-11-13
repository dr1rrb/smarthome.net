using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartHomeDotNet.Utils
{
	public interface IConfig
	{
		IEnumerable<string> Validate();
	}
}