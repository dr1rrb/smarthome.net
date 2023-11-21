using System;
using System.Linq;

namespace Mavri.Ha.Data;

public record EntityStateUpdate(string EntityId, EntityState OldState, EntityState NewState);