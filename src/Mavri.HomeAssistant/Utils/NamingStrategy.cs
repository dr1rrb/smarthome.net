using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mavri.HomeAssistant.Utils;

internal static class NamingStrategy
{
	public static string ToCsharpName(string prefix, string name)
	{
		name = ToCSharpCamel(name);
		return name is null or { Length: 0 } || char.IsNumber(name[0]) ? prefix + name : name;
	}

	public static string ToCSharpCamel(string name, bool canIgnoreSomeChars = true)
	{
		if (name.Length == 0)
		{
			return name;
		}

		var result = new StringBuilder((int)(name.Length * 1.2));
		var nextIsUpper = true;
		var lastWasDigit = false;

		for (var i = 0; i < name.Length; i++)
		{
			var c = name[i];
			if (canIgnoreSomeChars && !(lastWasDigit && IsDigit(i + 1)) && c is '_' or ' ' or '-' or '\'')
			{
				nextIsUpper = true;
			}
			else if (c is '&')
			{
				nextIsUpper = true;
				lastWasDigit = false;
				result.Append("And");
			}
			else if (!char.IsLetterOrDigit(c))
			{
				nextIsUpper = true;
				lastWasDigit = false;
				result.Append('_');
			}
			else if (nextIsUpper)
			{
				nextIsUpper = false;
				lastWasDigit = char.IsDigit(c);
				result.Append(char.ToUpperInvariant(c));
			}
			else
			{
				// nextIsUpper = false; // Already false, no needs to update it
				lastWasDigit = char.IsDigit(c);
				result.Append(c);
			}
		}

		return result.ToString();

		bool IsDigit(int i)
			=> i < name.Length && char.IsDigit(name[i]);
	}
}