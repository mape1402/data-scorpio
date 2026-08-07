namespace DataScorpio.DynaBee;

/// <summary>
/// Defines the opt-in DynaBee acceleration boundary for DataScorpio.
/// </summary>
public interface IDataScorpioDynaBeeAccelerator
{
    /// <summary>
    /// Gets a value indicating whether DynaBee acceleration is enabled.
    /// </summary>
    bool IsEnabled { get; }
}
