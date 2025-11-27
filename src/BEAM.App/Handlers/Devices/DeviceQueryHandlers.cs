using Quasar.Cqrs;
using Quasar.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using BEAM.App.Domain.Devices;
using BEAM.App.ReadModels;
using Quasar.Persistence.Relational.EfCore;

namespace BEAM.App.Handlers.Devices;

/// <summary>
/// Handles query for getting a single device.
/// </summary>
public sealed class GetDeviceHandler : IQueryHandler<GetDeviceQuery, DeviceReadModel?>
{
    private readonly IReadRepository<DeviceReadModel> _repository;

    public GetDeviceHandler(IReadRepository<DeviceReadModel> repository)
    {
        _repository = repository;
    }

    public async Task<DeviceReadModel?> Handle(GetDeviceQuery query, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(query.DeviceId, cancellationToken);
    }
}

/// <summary>
/// Handles query for listing devices with pagination.
/// </summary>
public sealed class ListDevicesHandler : IQueryHandler<ListDevicesQuery, PagedResult<DeviceReadModel>>
{
    private readonly ReadModelContext<BeamReadModelStore> _context;

    public ListDevicesHandler(ReadModelContext<BeamReadModelStore> context)
    {
        _context = context;
    }

    public async Task<PagedResult<DeviceReadModel>> Handle(ListDevicesQuery query, CancellationToken cancellationToken = default)
    {
        // Get total count
        var totalCount = await _context.Set<DeviceReadModel>().CountAsync(cancellationToken);

        // Get paginated results (without ordering - SQLite doesn't support DateTimeOffset in ORDER BY)
        var skip = (query.Page - 1) * query.PageSize;
        var items = await _context.Set<DeviceReadModel>()
            .Skip(skip)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        // Order in-memory after retrieving from database
        items = items.OrderByDescending(d => d.RegisteredAt).ToList();

        return new PagedResult<DeviceReadModel>(
            items,
            totalCount,
            query.Page,
            query.PageSize);
    }
}
