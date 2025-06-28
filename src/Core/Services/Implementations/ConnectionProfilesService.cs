using ConnectionManager.Core.Data;
using ConnectionManager.Core.Models;
using ConnectionManager.Core.Services.Contracts.ConnectionProfiles;
using ConnectionManager.Core.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IValidatorFactory = ConnectionManager.Core.Services.Interfaces.IValidatorFactory;

namespace ConnectionManager.Core.Services.Implementations;

internal sealed class ConnectionProfilesService : IConnectionProfilesService
{
    #region construction

    private readonly ILogger<ConnectionProfilesService> _logger;
    private readonly AppDbContext _dbContext;
    private readonly IValidatorFactory _validatorFactory;

    public ConnectionProfilesService(
        ILogger<ConnectionProfilesService> logger,
        AppDbContext dbContext,
        IValidatorFactory validatorFactory
    )
    {
        _logger = logger;
        _dbContext = dbContext;
        _validatorFactory = validatorFactory;
    }

    #endregion

    public async Task<List<ConnectionProfileDTO>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Fetching all connection profiles from database");

        var response = await _dbContext
            .ConnectionProfiles.AsNoTracking()
            .Select(cp => new ConnectionProfileDTO(cp.Id, cp.Name, cp.ConnectionType))
            .ToListAsync(cancellationToken);
        _logger.LogDebug("Fetched mapped ConnectionProfile entities from database");

        return response;
    }

    public async Task<ConnectionProfileDTO?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Fetching connection profile with ID {Id} from database", id);

        var response = await _dbContext
            .ConnectionProfiles.AsNoTracking()
            .Where(cp => cp.Id == id)
            .Select(cp => new ConnectionProfileDTO(cp.Id, cp.Name, cp.ConnectionType))
            .SingleOrDefaultAsync(cancellationToken);
        if (response is null)
        {
            _logger.LogDebug("Connection profile with ID {Id} not found in database", id);
            return null;
        }

        _logger.LogDebug("Fetched mapped ConnectionProfile entity with ID {Id} from database", id);
        return response;
    }

    public async Task<ConnectionProfileDTO> CreateAsync(
        CreateConnectionProfileRequest request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Creating new connection profile");

        var validators = _validatorFactory.GetValidators<CreateConnectionProfileRequest>();
        if (validators.Any(v => !v.Validate(request).IsValid))
        {
            _logger.LogDebug("Validation failed for CreateConnectionProfileRequest");
            throw new ValidationException("Validation failed for CreateConnectionProfileRequest");
        }
        _logger.LogDebug("Validation passed for CreateConnectionProfileRequest");

        // TODO check name is unique

        var entity = new ConnectionProfile
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ConnectionType = request.ConnectionType,
        };
        _dbContext.ConnectionProfiles.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Created new connection profile with ID {Id}", entity.Id);

        return new ConnectionProfileDTO(entity.Id, entity.Name, entity.ConnectionType);
    }

    public async Task<ConnectionProfileDTO> UpdateAsync(
        UpdateConnectionProfileRequest request,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Updating connection profile with ID {Id}", request.Id);

        var entity = await _dbContext
            .ConnectionProfiles.Where(cp => cp.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (entity is null)
        {
            _logger.LogDebug("Connection profile with ID {Id} not found in database", request.Id);
            throw new KeyNotFoundException($"Connection profile with ID {request.Id} not found");
        }

        var validators = _validatorFactory.GetValidators<UpdateConnectionProfileRequest>();
        if (validators.Any(v => !v.Validate(request).IsValid))
        {
            _logger.LogDebug("Validation failed for UpdateConnectionProfileRequest");
            throw new ValidationException("Validation failed for UpdateConnectionProfileRequest");
        }
        _logger.LogDebug("Validation passed for UpdateConnectionProfileRequest");

        // TODO check name is unique

        entity.Name = request.Name;
        entity.ConnectionType = request.ConnectionType;

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Updated connection profile with ID {Id}", entity.Id);
        return new ConnectionProfileDTO(entity.Id, entity.Name, entity.ConnectionType);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting connection profile with ID {Id} from database", id);

        await _dbContext
            .ConnectionProfiles.Where(cp => cp.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        _logger.LogDebug("Deleted connection profile with ID {Id} from database", id);
    }
}
