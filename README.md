# Yapper

Yep Another Wrapper for [Dapper](https://github.com/StackExchange/dapper-dot-net).  This
wrapper provides a simple Unit of Work pattern for isolating transactional business rules,
a simple caching layer that uses "tags", and a simple CRUD layer for easy repository
management.


_Highlights_

-	Dapper - lean & mean
-	Support for identities (does not return the identity but ignores on inserts)
-	Support for Composite Primary Keys
-	Support for Paging (in progress)
-	Support for Strongly Typed Aggregation & Selection
-	Optional Data Annontations for mapping tables & columns

### Show me the Code!

Annotation Samples

``` csharp
[Table("CUSTOMERS")]
public class Customer : IUpdatedAt
{
	[Key,Column("customer_id"),DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int ID { get; set; }

	[Column("customer_name")]
	public string Name { get; set; }

	//	calculated by database - so not part of updates
	[Column("pricing_level"),DatabaseGenerated(DatabaseGeneratedOption.Computed)]
	public int PricingLevel { get; set; }
	
	//	not stored in, or read from database - since we are using annontations
	public bool IsNew { get { return ID > 0; } }
}
```

Insert,Select,Update,Delete (CRUD) Operations

``` csharp
using (var db = Database.OpenSession())
{
	Customer c = new Customer { Name = "Stu" };

	db.Insert(c);

	//	example! - dont get hung up with the proper way to pull
	//	the identity value - demonstrating that its well formed
	//	insert SQL - so optionally you can create the proper
	//	insert statement and use that instead
	c.ID = db.Query<int>("select max(customer_id) from CUSTOMERS");

	c.Name = "Stu";

	db.Update(c);

	//	pulls using the primary key (again annotations are key here)
	Customer match = db.Select(c);

	if (match != null && match.ID == c.ID)
	{
		db.Delete(c);
	}
}

Insert,Select,Update,Delete (CRUD) Operations w/Anonymous Objects

``` csharp
using (var db = Database.OpenSession())
{
	//	ALL anonymous calls ignore extra properties
	db.Insert<Customer>(new { Name = "Stu", IsModified = true });

	//	again..example! - not going for proper SQL here
	int id = db.Query<int>("select max(customer_id) from CUSTOMERS");

	Customer c = db.Select<Customer>(new { ID = id });

	c.Name = "Stu";

	db.Update(new { c.Name }, new { c.ID });
	//db.Update(new { Name = "Stu" }, new { ID = 99 });

	if (match != null && match.ID == c.ID)
	{
		db.Delete<int>(new { c.ID });
		//db.Delete<int>(new { ID = 99 });
	}
}
```

Basic Query/Execute Operations

``` csharp
using (var db = Database.OpenSession())
{
	//	ALL anonymous calls ignore extra properties
	DateTime dt = db.Query<DateTime>("select getutcdate()");
	
	IList<DateTime> dates = db.Query<DateTime>("select getutcdate() union select dateadd(year, 100, getutcdate())").ToList();
	//DateTime date = db.Query<DateTime>("select @dt", new { dt = new DateTime(2000, 1, 1) }).First();

	int rows = db.Execute("delete from CUSTOMERS");
	//int rows = db.Execute("delete from CUSTOMERS where name = @nm", new { nm = "Stu" });

	using (var greader = db.QueryMultiple("select getutcdate(); select 100"))
	{
		DateTime date = greader.Read<DateTime>().First();
		int number = greader.Read<int>().First();
	}
}
```

Unit of Work Operations

``` csharp
using (var db = Database.OpenSession())
{
	using (var trx = db.BeginTransaction())
	{
		trx.Commit();
		//	otherwise on Dispose .Rollback by default
		//	if .Commit not already called
	}
}
```
