using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SmartHomeDotNet.Utils
{
	internal class SnakeCaseNamingPolicy : JsonNamingPolicy
	{
		public override string ConvertName(string name)
		{
			if (name.Length == 0)
			{
				return name;
			}

			var result = new StringBuilder();

			result.Append(Char.ToLowerInvariant(name[0]));
			for (var i = 1; i < name.Length; i++)
			{
				var c = name[i];
				if (Char.IsUpper(c))
				{
					result.Append('_');
					result.Append(Char.ToLowerInvariant(c));
				}
				else
				{
					result.Append(c);
				}
			}
			return result.ToString();
		}
	}
}