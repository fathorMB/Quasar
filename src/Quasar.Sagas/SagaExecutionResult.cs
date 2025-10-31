namespace Quasar.Sagas;

/// <summary>
/// Represents the outcome of a saga step.
/// </summary>
public readonly record struct SagaExecutionResult(SagaExecutionStatus Status)
{
    /// <summary>
    /// Gets a value indicating whether the saga has completed.
    /// </summary>
    public bool IsCompleted => Status == SagaExecutionStatus.Completed;

    /// <summary>
    /// Continue saga execution.
    /// </summary>
    public static SagaExecutionResult Continue() => new(SagaExecutionStatus.Continue);

    /// <summary>
    /// Continue saga execution while ensuring the state remains active.
    /// </summary>
    public static SagaExecutionResult Continue<TState>(TState state)
        where TState : class, ISagaState
    {
        if (state is null) throw new ArgumentNullException(nameof(state));
        state.IsCompleted = false;
        state.UpdatedUtc = DateTimeOffset.UtcNow;
        return Continue();
    }

    /// <summary>
    /// Mark the saga as completed.
    /// </summary>
    public static SagaExecutionResult Completed() => new(SagaExecutionStatus.Completed);

    /// <summary>
    /// Mark the saga as completed and update the provided state.
    /// </summary>
    public static SagaExecutionResult Completed<TState>(TState state)
        where TState : class, ISagaState
    {
        if (state is null) throw new ArgumentNullException(nameof(state));
        state.IsCompleted = true;
        state.UpdatedUtc = DateTimeOffset.UtcNow;
        return Completed();
    }
    /// <summary>
    /// Skip processing the message without mutating state.
    /// </summary>
    public static SagaExecutionResult Ignore() => new(SagaExecutionStatus.Ignored);
}

/// <summary>
/// Values describing the status returned by a saga handler.
/// </summary>
public enum SagaExecutionStatus
{
    /// <summary>
    /// Continue tracking saga state.
    /// </summary>
    Continue = 0,

    /// <summary>
    /// Saga instance completed successfully.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Message ignored with no state mutation.
    /// </summary>
    Ignored = 2
}
