namespace DataScorpio.Profiles;

using System.Linq.Expressions;
using DataScorpio.Querying;

/// <summary>
/// Defines the fluent configuration surface for a typed query profile.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IQueryProfileBuilder<TEntity>
{
    /// <summary>
    /// Allows filtering by a field using the member name as the public query name.
    /// </summary>
    /// <param name="field">The field expression.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> AllowFilter(Expression<Func<TEntity, object>> field);

    /// <summary>
    /// Allows filtering by a field using a public alias.
    /// </summary>
    /// <param name="name">The public field name or alias.</param>
    /// <param name="field">The field expression.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> AllowFilter(string name, Expression<Func<TEntity, object>> field);

    /// <summary>
    /// Allows sorting by a field using the member name as the public query name.
    /// </summary>
    /// <param name="field">The field expression.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> AllowSort(Expression<Func<TEntity, object>> field);

    /// <summary>
    /// Allows sorting by a field using a public alias.
    /// </summary>
    /// <param name="name">The public field name or alias.</param>
    /// <param name="field">The field expression.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> AllowSort(string name, Expression<Func<TEntity, object>> field);

    /// <summary>
    /// Allows search by a field using the member name as the public query name.
    /// </summary>
    /// <param name="field">The field expression.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> AllowSearch(Expression<Func<TEntity, object>> field);

    /// <summary>
    /// Allows search by a field using a public alias.
    /// </summary>
    /// <param name="name">The public field name or alias.</param>
    /// <param name="field">The field expression.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> AllowSearch(string name, Expression<Func<TEntity, object>> field);

    /// <summary>
    /// Allows including a navigation path using the member name as the public include name.
    /// </summary>
    /// <param name="include">The include expression.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> AllowInclude(Expression<Func<TEntity, object>> include);

    /// <summary>
    /// Allows including a navigation path using a public alias.
    /// </summary>
    /// <param name="name">The public include name or alias.</param>
    /// <param name="include">The include expression.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> AllowInclude(string name, Expression<Func<TEntity, object>> include);

    /// <summary>
    /// Registers a custom filter by name.
    /// </summary>
    /// <param name="name">The public filter name.</param>
    /// <param name="filter">The custom filter implementation.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> CustomFilter(
        string name,
        Func<IQueryable<TEntity>, QueryValue, IQueryable<TEntity>> filter);

    /// <summary>
    /// Registers a reusable custom filter for entities that implement or inherit a contract.
    /// </summary>
    /// <typeparam name="TContract">The base class or interface contract.</typeparam>
    /// <param name="name">The public filter name.</param>
    /// <param name="predicate">Builds the contract predicate from the query value.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> CustomFilter<TContract>(
        string name,
        Func<QueryValue, Expression<Func<TContract, bool>>> predicate);

    /// <summary>
    /// Registers a custom filter by name with access to the full filter descriptor.
    /// </summary>
    /// <param name="name">The public filter name.</param>
    /// <param name="filter">The custom filter implementation.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> CustomFilterDescriptor(
        string name,
        Func<IQueryable<TEntity>, FilterDescriptor, IQueryable<TEntity>> filter);

    /// <summary>
    /// Registers a custom sort by name.
    /// </summary>
    /// <param name="name">The public sort name.</param>
    /// <param name="sort">The custom sort implementation.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> CustomSort(
        string name,
        Func<IQueryable<TEntity>, SortDirection, IQueryable<TEntity>> sort);

    /// <summary>
    /// Registers a reusable custom sort for entities that implement or inherit a contract.
    /// </summary>
    /// <typeparam name="TContract">The base class or interface contract.</typeparam>
    /// <param name="name">The public sort name.</param>
    /// <param name="keySelector">The contract key selector.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> CustomSort<TContract>(
        string name,
        Expression<Func<TContract, object>> keySelector);

    /// <summary>
    /// Applies reusable query conventions to the current profile.
    /// </summary>
    /// <param name="conventions">The convention set.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> Use(QueryConventionSet conventions);

    /// <summary>
    /// Applies reusable query conventions to the current profile.
    /// </summary>
    /// <typeparam name="TConventionSet">The convention set type.</typeparam>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> Use<TConventionSet>()
        where TConventionSet : QueryConventionSet, new();

    /// <summary>
    /// Sets the default sort using the member name as the public query name.
    /// </summary>
    /// <param name="field">The field expression.</param>
    /// <param name="direction">The sort direction.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> DefaultSort(
        Expression<Func<TEntity, object>> field,
        SortDirection direction = SortDirection.Ascending);

    /// <summary>
    /// Sets the default sort using a public alias.
    /// </summary>
    /// <param name="name">The public field name or alias.</param>
    /// <param name="field">The field expression.</param>
    /// <param name="direction">The sort direction.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> DefaultSort(
        string name,
        Expression<Func<TEntity, object>> field,
        SortDirection direction = SortDirection.Ascending);

    /// <summary>
    /// Sets the maximum page size allowed for the profile.
    /// </summary>
    /// <param name="maxPageSize">The maximum page size.</param>
    /// <returns>The same builder.</returns>
    IQueryProfileBuilder<TEntity> MaxPageSize(int maxPageSize);
}
