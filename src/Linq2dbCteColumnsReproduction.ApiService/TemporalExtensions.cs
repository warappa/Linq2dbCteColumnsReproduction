using Microsoft.EntityFrameworkCore;

internal static class TemporalExtensions
{
    public static IQueryable<TTemporalEntity> TemporalAsOf<TTemporalEntity>(this DbSet<TTemporalEntity> dbSet, DateTime? pointInTime)
        where TTemporalEntity : class, ITemporalEntity
    {
        return pointInTime is null ?
            dbSet :
            throw new NotSupportedException("Not in this repo"); //SqlServerDbSetExtensions.TemporalAsOf(dbSet, pointInTime.Value);
    }

    public static IQueryable<PartReadModel> IncludeAll(this IQueryable<PartReadModel> query)
    {
        return query
            .Include(x => x.ExternalIds)
            .Include(x => x.DataReferences);
    }
}
