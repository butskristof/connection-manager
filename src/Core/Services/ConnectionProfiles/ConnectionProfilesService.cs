using ConnectionManager.Core.Common;
using ConnectionManager.Core.Common.Constants;
using ConnectionManager.Core.Common.Validation;
using ConnectionManager.Core.Data;
using ConnectionManager.Core.Models;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConnectionManager.Core.Services.ConnectionProfiles;

internal sealed class ConnectionProfilesService : IConnectionProfilesService
{
    #region construction

    private readonly ILogger<ConnectionProfilesService> _logger;
    private readonly AppDbContext _dbContext;
    private readonly IValidationService _validationService;

    public ConnectionProfilesService(
        ILogger<ConnectionProfilesService> logger,
        AppDbContext dbContext,
        IValidationService validationService
    )
    {
        _logger = logger;
        _dbContext = dbContext;
        _validationService = validationService;
    }

    #endregion

    public async Task<ErrorOr<List<ConnectionProfileDTO>>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Fetching all connection profiles from database");

        var entities = await _dbContext
            .ConnectionProfiles.AsNoTracking()
            .Select(cp => new ConnectionProfileDTO(cp))
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "Fetched {Count} mapped connection profiles from database",
            entities.Count
        );
        return entities;
    }

    public async Task<ErrorOr<ConnectionProfileDTO>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Fetching connection profile with ID {Id} from database", id);

        var entity = await _dbContext
            .ConnectionProfiles.AsNoTracking()
            .Where(cp => cp.Id == id)
            .Select(cp => new ConnectionProfileDTO(cp))
            .SingleOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            _logger.LogDebug("Connection profile with ID {Id} not found in database", id);
            return Error.NotFound(
                ErrorCodes.NotFound,
                $"Connection profile with ID {id} was not found"
            );
        }

        _logger.LogDebug("Fetched mapped connection profile with ID {Id} from database", entity.Id);
        return entity;
    }

    public async Task<ErrorOr<ConnectionProfileDTO>> CreateAsync(
        CreateConnectionProfileRequest request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Creating new connection profile");

        var validationResult = _validationService.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogDebug(
                "Validation failed for CreateConnectionProfileRequest with {ErrorCount} errors",
                validationResult.Errors.Count
            );
            return validationResult.ToValidationErrors();
        }
        _logger.LogDebug("Validation passed for CreateConnectionProfileRequest");

        if (!await IsNameUniqueAsync(request.Name, null, cancellationToken))
        {
            _logger.LogDebug("Connection profile named {Name} already exists", request.Name);
            return Error.Conflict(
                ErrorCodes.NotUnique,
                $"A connection profile named {request.Name} already exists"
            );
        }

        var entity = new ConnectionProfile
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ConnectionType = request.ConnectionType,
            Host = request.Host,
            Port = request.Port,
            Username = request.Username,
            KeyPath = request.KeyPath,
            Password = request.Password,
        };

        _dbContext.ConnectionProfiles.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Persisted new entity to database");

        _logger.LogDebug("Created new connection profile with ID {Id}", entity.Id);
        return new ConnectionProfileDTO(entity);
    }

    public async Task<ErrorOr<ConnectionProfileDTO>> UpdateAsync(
        UpdateConnectionProfileRequest request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Updating connection profile with ID {Id}", request.Id);

        var entity = await _dbContext.ConnectionProfiles.SingleOrDefaultAsync(
            cp => cp.Id == request.Id,
            cancellationToken
        );

        if (entity is null)
        {
            _logger.LogDebug("Connection profile with ID {Id} not found in database", request.Id);
            return Error.NotFound(
                ErrorCodes.NotFound,
                $"Connection profile with ID {request.Id} was not found"
            );
        }

        var validationResult = _validationService.Validate(request);
        if (!validationResult.IsValid)
        {
            _logger.LogDebug(
                "Validation failed for UpdateConnectionProfileRequest with {ErrorCount} errors",
                validationResult.Errors.Count
            );
            return validationResult.ToValidationErrors();
        }
        _logger.LogDebug("Validation passed for UpdateConnectionProfileRequest");

        if (
            request.Name != entity.Name
            && !await IsNameUniqueAsync(request.Name, entity.Id, cancellationToken)
        )
        {
            _logger.LogDebug("Connection profile named {Name} already exists", request.Name);
            return Error.Conflict(
                ErrorCodes.NotUnique,
                $"A connection profile named {request.Name} already exists"
            );
        }

        entity.Name = request.Name;
        entity.ConnectionType = request.ConnectionType;
        entity.Host = request.Host;
        entity.Port = request.Port;
        entity.Username = request.Username;
        entity.KeyPath = request.KeyPath;
        entity.Password = request.Password;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Persisted changes to database");

        _logger.LogDebug("Updated connection profile with ID {Id}", entity.Id);
        return new ConnectionProfileDTO(entity);
    }

    private Task<bool> IsNameUniqueAsync(
        string name,
        Guid? id,
        CancellationToken cancellationToken = default
    ) =>
        _dbContext
            .ConnectionProfiles.AsNoTracking()
            .AllAsync(
                cp => cp.Name != name || (id.HasValue && cp.Id == id.Value),
                cancellationToken
            );

    public async Task<ErrorOr<Deleted>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Deleting connection profile with ID {Id} from database", id);

        var entity = await _dbContext.ConnectionProfiles.SingleOrDefaultAsync(
            cp => cp.Id == id,
            cancellationToken
        );

        if (entity is null)
        {
            _logger.LogDebug("Connection profile with ID {Id} not found in database", id);
            return Error.NotFound(
                ErrorCodes.NotFound,
                $"Connection profile with ID {id} was not found"
            );
        }

        _dbContext.ConnectionProfiles.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Deleted connection profile with ID {Id} from database", id);
        return Result.Deleted;
    }
}
