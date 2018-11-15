using System;
using System.Linq;

namespace SmartHomeDotNet.Mqtt
{
	public enum QualityOfService : byte
	{
		/// <summary>
		/// This service level guarantees a best-effort delivery. There is no guarantee of delivery.
		/// The recipient does not acknowledge receipt of the message and the message is not stored and re-transmitted by the sender.
		/// QoS level 0 is often called “fire and forget” and provides the same guarantee as the underlying TCP protocol.
		/// </summary>
		AtMostOnce = 0,

		/// <summary>
		/// QoS level 1 guarantees that a message is delivered at least one time to the receiver.
		/// The sender stores the message until it gets a  PUBACK packet from the receiver that acknowledges receipt of the message.
		/// It is possible for a message to be sent or delivered multiple times.
		/// </summary>
		AtLeastOnce = 1,

		/// <summary>
		/// QoS 2 is the highest level of service in MQTT. This level guarantees that each message is received only once by the intended recipients.
		/// QoS 2 is the safest and slowest quality of service level. The guarantee is provided by at least two request/response flows
		/// (a four-part handshake) between the sender and the receiver. The sender and receiver use the packet identifier of the original
		/// PUBLISH message to coordinate delivery of the message.
		/// </summary>
		ExcatlyOnce = 2,
	}
}