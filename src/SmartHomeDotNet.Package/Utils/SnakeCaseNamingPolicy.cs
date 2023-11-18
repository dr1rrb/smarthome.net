using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SmartHomeDotNet.Utils
{
	public class SnakeCaseNamingPolicy : JsonNamingPolicy
	{
		public override string ConvertName(string name)
			=> FromCamel(name);

		public static string FromCamel(string name)
		{
			if (name.Length == 0)
			{
				return name;
			}

			var result = new StringBuilder((int)(name.Length * 1.2));

			result.Append(char.ToLowerInvariant(name[0]));
			for (var i = 1; i < name.Length; i++)
			{
				var c = name[i];
				if (char.IsUpper(c))
				{
					result.Append('_');
					result.Append(char.ToLowerInvariant(c));
				}
				else
				{
					result.Append(c);
				}
			}
			return result.ToString();
		}

		public static string ToCamel(string name)
		{
			if (name.Length == 0)
			{
				return name;
			}

			var result = new StringBuilder(name.Length);
			var nextIsUpper = true;

			for (var i = 0; i < name.Length; i++)
			{
				var c = name[i];
				if (c is '_')
				{
					nextIsUpper = true;
				}
				else if (nextIsUpper)
				{
					nextIsUpper = false;
					result.Append(char.ToUpperInvariant(c));
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