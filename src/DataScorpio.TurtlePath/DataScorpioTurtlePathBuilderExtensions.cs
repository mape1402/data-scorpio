namespace Microsoft.Extensions.DependencyInjection;

using DataScorpio.TurtlePath;
using global::TurtlePath;
using global::TurtlePath.Persistence;

/// <summary>
/// Provides TurtlePath integration helpers for DataScorpio.
/// </summary>
public static class DataScorpioTurtlePathBuilderExtensions
{
    /// <summary>
    /// Registers DataScorpio criteria support on the current TurtlePath pipeline.
    /// </summary>
    /// <param name="builder">The TurtlePath builder.</param>
    /// <returns>The same TurtlePath builder.</returns>
    public static ITurtlePathBuilder UseDataScorpio(this ITurtlePathBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        builder.Services.AddSingleton<IStorageCriteriaApplier, DataScorpioStorageCriteriaApplier>();

        return builder;
    }
}
