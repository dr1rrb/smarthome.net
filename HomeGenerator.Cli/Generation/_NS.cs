using System;
using System.Linq;

namespace Mavri.Ha.Generation;

public static class NS
{
	// Where we have the entities (ButtonEntity, LightEntity)
	public static readonly string Entities = "global::" + typeof(Mavri.Ha.Entities.Entity<>).Namespace;
}