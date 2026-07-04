using System.Diagnostics;
using System.Linq.Expressions;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.EntityFrameworkCore;
using LinqToDB.Linq;
using Microsoft.EntityFrameworkCore;

public class DataService
{
    static DataService()
    {
        AddEnumSupport();
    }

    // somewhere called for Enum support
    static void AddEnumSupport()
    {
        // Support Enums in Linq2Db as per https://github.com/linq2db/linq2db/pull/2190
        Expressions.MapMember(
            (Enum e, Enum e2) => e.HasFlag(e2),
            (t, flag) => (Sql.ConvertTo<int>.From(t) & Sql.ConvertTo<int>.From(flag)) != 0);
    }

    private ReadModelDbContext _dbContext;

    public DataService(ReadModelDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        _dbContext = dbContext;
    }

    public Paged<PartReadModel[]> QueryWithDependants(
        Expression<Func<PartReadModel, bool>> predicate,
        Expression<Func<PartReadModel, object>> orderBy,
        bool descending = false,
        int? maxHierarchyLevels = null,
        DateTime? pointInTime = null,
        int page = 0,
        int pageSize = 20)
    {
        return QueryWithDependants(x => x, predicate, orderBy, descending, maxHierarchyLevels, pointInTime, page, pageSize);
    }

    public Paged<T[]> QueryWithDependants<T>(
        Expression<Func<PartReadModel, T>> projection,
        Expression<Func<PartReadModel, bool>> predicate,
        Expression<Func<PartReadModel, object>> orderBy,
        bool descending = false,
        int? maxHierarchyLevels = null,
        DateTime? pointInTime = null,
        int page = 0,
        int pageSize = 20)
    {
        var dbContext = _dbContext;
        using (var db = dbContext.CreateLinqToDBConnection())
        {
            // db.UseQueryTraceOptions(x =>
            // {
            //     return x
            //         .WithOnTrace(traceInfo =>
            //         {
            //             Debug.WriteLine(traceInfo.SqlText);
            //         })
            //         .WithTraceLevel(TraceLevel.Verbose);
            // });
            db.OnTraceConnection = traceInfo =>
            {
               if (!string.IsNullOrEmpty(traceInfo.SqlText))
               {
                   Debug.WriteLine(traceInfo.SqlText);
               }
            };
            db.TraceSwitchConnection =
               new TraceSwitch("DataConnection", "DataConnection trace switch", TraceLevel.Verbose.ToString());

            var initialQuery = dbContext.Parts.TemporalAsOf(pointInTime)
                .AsNoTracking()
                .Where(predicate);

            return QueryWithDependantsInternal(dbContext, db, projection, initialQuery, orderBy, descending, maxHierarchyLevels, pointInTime, page, pageSize);
        }
    }


    private static Paged<T[]> QueryWithDependantsInternal<T>(
        ReadModelDbContext dbContext,
        DataConnection db,
        Expression<Func<PartReadModel, T>> projection,
        IQueryable<PartReadModel> initalQuery,
        Expression<Func<PartReadModel, object>> orderBy,
        bool descending = false,
        int? maxHierarchyLevels = null,
        DateTime? pointInTime = null,
        int page = 0,
        int pageSize = 20)
    {
        var cteInitialQuery = initalQuery
            .Select(x => new PartHierarchyCte
            {
                RootPartSortField = orderBy.Compile()(x),
                RootPartId = x.Id,
                HierarchyLevel = 0,
                PartId = x.Id
            })
            //.OrderBy(x => x.RootPartSortField, descending) // Should be enough
            .OrderBy(x => x.RootPartSortField)
            .Skip(page * pageSize)
            .Take(pageSize);

        var partCte = db.GetCte<PartHierarchyCte>(partHierarchy =>
        {
            var dataReferenceQueryable = dbContext.PartDataReferences.TemporalAsOf(pointInTime)
                .AsNoTracking();

            return
                // CTE initial query
                cteInitialQuery
                .Concat(
                    // CTE recursive query
                    partHierarchy
                    .InnerJoin(
                        dataReferenceQueryable,
                        (cte, reference) =>
                            reference.Type.HasFlag(DataReferenceType.Hierarchy) &&
                            reference.ParentId == cte.PartId,
                        (cte, reference) => new PartHierarchyCte
                        {
                            RootPartSortField = cte.RootPartSortField,
                            RootPartId = cte.RootPartId,
                            PartId = reference.ReferenceId,
                            HierarchyLevel = cte.HierarchyLevel + 1
                        }));
        });

        if (maxHierarchyLevels is not null)
        {
            partCte = partCte
                .Where(x => x.HierarchyLevel < maxHierarchyLevels);
        }

        var partsQueryable = dbContext.Parts.TemporalAsOf(pointInTime)
            .AsNoTracking();

        var allRelevant = partsQueryable
            .IncludeAll()
            .ToLinqToDB(db) // ToLinqToDB call needs to be *after* IncludeAll, otherwise Includes will be ignored
            .InnerJoin(
                partCte,
                (me, cte) => me.Id == cte.PartId,
                (me, id) => new { id.RootPartId, id.RootPartSortField, me });

        var totalCount = initalQuery.Count();

        var result = allRelevant
            .ToList()
            .GroupBy(x => new { x.RootPartId, x.RootPartSortField })
            .Select(x => x.Select(y => y.me).Select(x => projection.Compile()(x)).ToArray())
        .ToArray();

        return new Paged<T[]>(page, pageSize, totalCount, result);
    }
}
