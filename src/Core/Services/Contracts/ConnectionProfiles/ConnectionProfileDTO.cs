using ConnectionManager.Core.Models;

namespace ConnectionManager.Core.Services.Contracts.ConnectionProfiles;

public sealed record ConnectionProfileDTO(Guid Id, string Name, ConnectionType ConnectionType);
