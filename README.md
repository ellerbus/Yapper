# Yapper

Yep Another Wrapper for [Dapper](https://github.com/StackExchange/dapper-dot-net).  This
wrapper provides a simple Unit of Work pattern for isolating transactional business rules,
and a simple CRUD statement builder for easy repository management.



### Unit of Work Operations

``` csharp
using (var db = new DatabaseSession(myIDbConnection))
{
	using (var trx = db.BeginTransaction())
	{
		trx.Commit();
		//	otherwise on Dispose .Rollback by default
		//	if .Commit not already called
	}
}
```


### CRUD Statement Builders (MSSQL Dialect)

``` csharp
var sql = StatementBuilder.InsertOne<Member>();
var sql = StatementBuilder.UpdateOne<Member>();
var sql = StatementBuilder.DeleteOne<Member>();
var sql = StatementBuilder.SaveOne<Member>(); /* performs update then insert when not found */
var sql = StatementBuilder.SelectOne<Member>( /*[optional anonymous where]*/ );
var sql = StatementBuilder.SelectMany<Member>( /*[optional anonymous where]*/ );
```
