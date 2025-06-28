using ConnectionManager.Core.Services.Contracts.ConnectionProfiles;

namespace ConnectionManager.Core.Services.Interfaces;

public interface IConnectionProfilesService
{
    Task<List<ConnectionProfileDTO>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConnectionProfileDTO?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
    Task<ConnectionProfileDTO> CreateAsync(
        CreateConnectionProfileRequest request,
        CancellationToken cancellationToken = default
    );
    Task<ConnectionProfileDTO> UpdateAsync(
        UpdateConnectionProfileRequest request,
        CancellationToken cancellationToken = default
    );
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
