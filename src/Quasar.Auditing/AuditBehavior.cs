using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Quasar.Cqrs;
using Quasar.Security;

namespace Quasar.Auditing;

/// <summary>
/// A mediator pipeline behavior that creates and stores an audit entry for every command.
/// </summary>
public sealed class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IAuditStore _auditStore;

    public AuditBehavior(IAuditStore auditStore)
    {
        _auditStore = auditStore;
    }

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, Func<Task<TResponse>> next)
    {
        // Only audit commands
        if (request is not ICommand<TResponse>)
        {
            return await next().ConfigureAwait(false);
        }

        var subjectId = (request as IAuthorizableRequest)?.SubjectId;
        var commandType = request.GetType().FullName ?? request.GetType().Name;
        var commandJson = JsonSerializer.Serialize(request, request.GetType());

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next().ConfigureAwait(false);
            sw.Stop();

            var entry = new AuditEntry(
                Timestamp: DateTime.UtcNow,
                SubjectId: subjectId,
                CommandType: commandType,
                CommandJson: commandJson,
                IsSuccess: true,
                Error: null);

            await _auditStore.StoreAsync(entry);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();

            var entry = new AuditEntry(
                Timestamp: DateTime.UtcNow,
                SubjectId: subjectId,
                CommandType: commandType,
                CommandJson: commandJson,
                IsSuccess: false,
                Error: ex.Message);

            await _auditStore.StoreAsync(entry);
            throw;
        }
    }
}
