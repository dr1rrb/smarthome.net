using System;
using System.Linq;

namespace Mavri.Ha.Api;

public class HomeAssistantApiException : Exception
{
	public HomeAssistantApiException(ErrorCode code, string message)
		: base(message)
		=> Code = code;

	public ErrorCode Code { get; }

	public enum ErrorCode
	{
		Unknown = 0, // Not an exception for HA

		Id = 1, // A non-increasing identifier has been supplied.

		//[JsonPropertyName("invalid_format")]
		Format = 2, // Received message is not in expected format (voluptuous validation error).
		NotFound = 3,   // Requested item cannot be found
	}
}