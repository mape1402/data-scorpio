namespace DataScorpio.Profiles;

using System.Linq.Expressions;
using DataScorpio.Querying;

/// <summary>
/// Defines reusable query conventions that can be applied to compatible profiles.
/// </summary>
public interface IQueryConventionBuilder
{
    /// <summary>
    /// Adds a reusable custom filter for entities that implement or inherit a contract.
    /// </summary>
    /// <typeparam name="TContract">The base class or interface contract.</typeparam>
    /// <param name="name">The public filter name.</param>
    /// <param name="predicate">Builds the contract predicate from the query value.</param>
    /// <returns>The same builder.</returns>
    IQueryConventionBuilder CustomFilter<TContract>(
        string name,
        Func<QueryValue, Expression<Func<TContract, bool>>> predicate);

    /// <summary>
    /// Adds a reusable custom sort for entities that implement or inherit a contract.
    /// </summary>
    /// <typeparam name="TContract">The base class or interface contract.</typeparam>
    /// <param name="name">The public sort name.</param>
    /// <param name="keySelector">The contract key selector.</param>
    /// <returns>The same builder.</returns>
    IQueryConventionBuilder CustomSort<TContract>(
        string name,
        Expression<Func<TContract, object>> keySelector);
}

