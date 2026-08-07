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
