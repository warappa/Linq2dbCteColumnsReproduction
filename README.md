### Reproduction of CTE Column-Removal Bug
In `Linq2dbCteColumnsReproduction.ApiService.csproj` you can switch between the **working (5.4.1/9.0.0)** NuGet packages and the **broken ones (6.3.0/9.5.0)**.

I created an **Aspire** solution so it's **"checkout'n'run"**.

Checked-in configuration is the broken one.

#### Linq2dbCteColumnsReproduction.ApiService.csproj
```xml
    <!-- working -->
    <!--<PackageReference Include="linq2db" Version="5.4.1" />
    <PackageReference Include="linq2db.EntityFrameworkCore" Version="9.0.0" />-->

    <!-- broken -->
    <PackageReference Include="linq2db" Version="6.3.0" />
    <PackageReference Include="linq2db.EntityFrameworkCore" Version="9.5.0" />
```

### Execute the Query
With debugger attached, open the endpoint `/test` on the ApiService.

The SQL is written to the output.
- On 6.3.0 the debugger should halt at the exception.
- On 5.4.1 it should return a result.

#### Saved Generated SQL
You find the sql query texts - which are written to the console - in those 2 files:
- [Query_linq2db_5.4.1_linq2db.EntityFrameworkCore_9.0.0_output.txt](Query_linq2db_5.4.1_linq2db.EntityFrameworkCore_9.0.0_output.txt)
- [Query_linq2db_6.3.0_linq2db.EntityFrameworkCore_9.5.0_output.txt](Query_linq2db_6.3.0_linq2db.EntityFrameworkCore_9.5.0_output.txt)

#### Stacktrace of 6.3.0
```log
Exception has occurred: CLR/Microsoft.Data.SqlClient.SqlException
An exception of type 'Microsoft.Data.SqlClient.SqlException' occurred in linq2db.dll but was not handled in user code: 'Invalid column name 'PartId'.'
   at Microsoft.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
   at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)
   at Microsoft.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)
   at Microsoft.Data.SqlClient.SqlDataReader.TryConsumeMetaData()
   at Microsoft.Data.SqlClient.SqlDataReader.get_MetaData()
   at Microsoft.Data.SqlClient.SqlCommand.FinishExecuteReader(SqlDataReader ds, RunBehavior runBehavior, String resetOptionsString, Boolean isInternal, Boolean forDescribeParameterEncryption, Boolean shouldCacheForAlwaysEncrypted)
   at Microsoft.Data.SqlClient.SqlCommand.RunExecuteReaderTds(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, Boolean isAsync, Int32 timeout, Task& task, Boolean asyncWrite, Boolean inRetry, SqlDataReader ds, Boolean describeParameterEncryptionRequest)
   at Microsoft.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, TaskCompletionSource`1 completion, Int32 timeout, Task& task, Boolean& usedCache, Boolean asyncWrite, Boolean inRetry, String method)
   at Microsoft.Data.SqlClient.SqlCommand.RunExecuteReader(CommandBehavior cmdBehavior, RunBehavior runBehavior, Boolean returnStream, String method)
   at Microsoft.Data.SqlClient.SqlCommand.ExecuteReader(CommandBehavior behavior)
   at Microsoft.Data.SqlClient.SqlCommand.ExecuteDbDataReader(CommandBehavior behavior)
   at LinqToDB.Data.DataConnection.ExecuteReader(CommandBehavior commandBehavior)
   at LinqToDB.Data.DataConnection.ExecuteDataReader(CommandBehavior commandBehavior)
   at LinqToDB.Data.DataConnection.QueryRunner.ExecuteReader()
   at LinqToDB.Internal.Linq.QueryRunner.BasicResultEnumerable`1.<GetEnumerator>d__8.MoveNext()
   at LinqToDB.Internal.Linq.Builder.ExpressionBuilder.Preamble`2.Execute(IDataContext dataContext, IQueryExpressions expressions, Object[] parameters, Object[] preambles)
   at LinqToDB.Internal.Linq.Query.InitPreambles(IDataContext dc, IQueryExpressions expressions, Object[] ps)
   at LinqToDB.Internal.Linq.ExpressionQuery`1.System.Collections.Generic.IEnumerable<T>.GetEnumerator()
   at System.Collections.Generic.List`1..ctor(IEnumerable`1 collection)
   at System.Linq.Enumerable.ToList[TSource](IEnumerable`1 source)
   at DataService.QueryWithDependantsInternal[T](ReadModelDbContext dbContext, DataConnection db, Expression`1 projection, IQueryable`1 initalQuery, Expression`1 orderBy, Boolean descending, Nullable`1 maxHierarchyLevels, Nullable`1 pointInTime, Int32 page, Int32 pageSize) in /home/warappa/Projects/Linq2dbCteColumnsReproduction/src/Linq2dbCteColumnsReproduction.ApiService/DataService.cs:line 156
   at DataService.QueryWithDependants[T](Expression`1 projection, Expression`1 predicate, Expression`1 orderBy, Boolean descending, Nullable`1 maxHierarchyLevels, Nullable`1 pointInTime, Int32 page, Int32 pageSize) in /home/warappa/Projects/Linq2dbCteColumnsReproduction/src/Linq2dbCteColumnsReproduction.ApiService/DataService.cs:line 82
   at DataService.QueryWithDependants(Expression`1 predicate, Expression`1 orderBy, Boolean descending, Nullable`1 maxHierarchyLevels, Nullable`1 pointInTime, Int32 page, Int32 pageSize) in /home/warappa/Projects/Linq2dbCteColumnsReproduction/src/Linq2dbCteColumnsReproduction.ApiService/DataService.cs:line 43
   at Program.<>c.<<Main>$>b__0_3(DataService service) in /home/warappa/Projects/Linq2dbCteColumnsReproduction/src/Linq2dbCteColumnsReproduction.ApiService/Program.cs:line 43
   at Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddlewareImpl.Invoke(HttpContext context)
```