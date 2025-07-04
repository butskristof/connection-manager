using ErrorOr;

namespace ConnectionManager.Core.Services.ConnectionProfiles;

public interface IConnectionProfilesService
{
    Task<ErrorOr<List<ConnectionProfileDTO>>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task<ErrorOr<ConnectionProfileDTO>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );

    Task<ErrorOr<ConnectionProfileDTO>> CreateAsync(
        CreateConnectionProfileRequest request,
        CancellationToken cancellationToken = default
    );

    Task<ErrorOr<ConnectionProfileDTO>> UpdateAsync(
        UpdateConnectionProfileRequest request,
        CancellationToken cancellationToken = default
    );

    Task<ErrorOr<Deleted>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
