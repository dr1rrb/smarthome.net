using System;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Mavri.Utils;

/// <summary>
/// A naming policy to support snake_case
/// </summary>
public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
	/// <inheritdoc />
	public override string ConvertName(string name)
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
}