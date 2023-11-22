using System;
using System.Collections.Immutable;
using System.Linq;
using Mavri.Ha.Data;

namespace Mavri.Ha.Entities;

public record struct Zone(double Longitude, double Latitude, IImmutableList<EntityId> Persons);

public sealed record ZoneEntity(EntityId Id, IHomeAssistantHub Hub) : Entity<Zone>(Id, Hub)
{
	/// <inheritdoc />
	protected override Zone Parse(EntityState raw)
	{
		var longitude = raw.Attributes.Get<double>("longitude", Id);
		var latitude = raw.Attributes.Get<double>("latitude", Id);
		var persons = raw.Attributes.GetArray<string>("persons", Id);

		return new Zone(longitude, latitude, persons.Select(id => (EntityId)id).ToImmutableArray());
	}
}