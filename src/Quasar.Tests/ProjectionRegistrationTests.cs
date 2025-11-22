using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quasar.EventSourcing.Abstractions;
using Quasar.Projections.Abstractions;
using Quasar.Web;
using Xunit;

namespace Quasar.Tests;

public class ProjectionRegistrationTests
{
    private sealed record SampleEvent() : IEvent;

    private sealed class SampleProjection : IProjection<SampleEvent>
    {
        public int CallCount { get; private set; }

        public Task HandleAsync(SampleEvent @event, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void AddProjections_registers_projection_as_concrete_and_object()
    {
        var services = new ServiceCollection();

        services.AddProjections(typeof(SampleProjection).Assembly);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var concrete = scope.ServiceProvider.GetService<SampleProjection>();
        var asObject = scope.ServiceProvider.GetServices<object>().OfType<SampleProjection>().FirstOrDefault();

        Assert.NotNull(concrete);
        Assert.NotNull(asObject);
        Assert.IsType<SampleProjection>(concrete);
        Assert.IsType<SampleProjection>(asObject);
    }

    [Fact]
    public void QuasarBuilder_AddQuasar_registers_projections()
    {
        var services = new ServiceCollection();

        services.AddQuasar(q => q.AddProjections(typeof(SampleProjection).Assembly));

        using var provider = services.BuildServiceProvider();
        var projection = provider.GetService<SampleProjection>();
        Assert.NotNull(projection);
    }
}
