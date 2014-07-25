# Yapper

Yep Another Wrapper for [Dapper](https://github.com/StackExchange/dapper-dot-net).  This
wrapper provides a simple Unit of Work pattern for isolating transactional business rules,
and encourages _strongly typed SQL_ by using Linq-ish syntax and expressions for building
SQL Statements.


_Highlights_

-	Dapper - lean & mean
-	Flexible storage maps for Enums
-	Support for Identities (or AutoNumbers)
-	Support for Composite Primary Keys
-	Support for Paging (in progress)
-	Support for Strongly Typed Aggregation & Selection


### SQL Databases Currently Supported

-	Microsoft SQL Server
-	SQL CE
-	SQLite


**SQL Databases to be Implemented
-	MySql
-	PostgreSQL


Why another one you say?  _Seriously?!_ Dapper is fun to work with and extend to 
meet the custom needs of a developer, team, or project.

### Show me the Code!

Annotation Samples

``` csharp
[Table("CUSTOMERS")]
public class Customer : IUpdatedAt
{
	[Key,Column("customer_id"),DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int ID { get; set; }

	[Column("customer_name"),MaxLength(50)]
	public int Name { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; }

	[Column("updated_at")]
	public DateTime? UpdatedAt { get; set; }
	
	// uses an ENUM map if value maps are defined (see enum definition below)
	// otherwise uses an int by default
	[Column("customer_option"),MaxLength(1)]
	public CustomerTypes CustomerType { get; set; }
	
	//	stored in database but not read from database
	[Column("preferred"),BooleanValueMap("Y", "N"),MaxLength(1)]
	public bool IsPreferred { get { return CustomerType == CustomerTypes.Preferred; } }
}

public enum CustomerTypes
{
	[EnumValueMap("X")] None,
	//	High Rollers
	[EnumValueMap("P")] Preferred,
	//	They represent no more than 20 percent of our customer base,
	//	but make up more than 50 percent of our sales.
	[EnumValueMap("L")] Loyal,
	//	They shop our stores frequently, but make their decisions
	//	based on the size of our markdowns.
	[EnumValueMap("D")] Discount,
	//	They do not have buying a particular item at the top of
	//	their "To Do" list, but come into the store on a whim.
	[EnumValueMap("I")] Impulse,
	//	Rather, they want a sense of experience and/or community.
	[EnumValueMap("W")] Wandering,
	//	Epic Fan of https://www.facebook.com/pages/EPIC-Comics/203013316384299
	[EnumValueMap("E")] EpicFan,
}
```

Configuration - The Dialect is derived from the provider invariant name, but like the 
example below can be overriden.

``` csharp

Sql.Dialect = new SqlServer2012Dialect();
Sql.Dialect = new SqlServer2008Dialect();

```

Insert,Update,Delete Operations (CUD Specific)

``` csharp
using (var db = DB.Open())
{
	Customer c = new Customer();

	using (var trans = db.CreateUnitOfWork())
	{

		var sql = Sql.Insert(c);
		
		c.ID = db.Query<int>(sql);

		Console.WriteLine("New ID: {0}", c.ID);

		//	oops no validation on the name

		c.Name = "Stu";

		sql = Sql.Update(c);
		
		db.Execute(sql);
	
		//	must be explicitly called otherwise the 
		//	dispose method will automatically rollback
		trans.Commit();
		
		//	if not called the dispose will automatically
		//	rollback
		//	trans.Rollback();
	}
	
	//	eh, we don't need this one
	sql = Sql.Delete(c);

	if (db.Execute(sql) == 0)
	{
		// it never really existed
	}
}
```

Update,Delete Many Operations (non-CUD Specific)

``` csharp
using (var db = DB.Open())
{
	Customer c = new Customer { ID = 999, Name = "Stu" };

	var sql = db.Update<Customer>()
		.Set(new { c.Name, UpdatedAt = new DateTime(2000, 1, 1) })
		.Set(x => x.CreatedAt, x => x.CreatedAt.AddDays(7))
		.Where(new { c.ID })
		;
	
	int rows = db.Execute(sql);
	
	sql = db.Delete<Customer>()
		.Where(x => x.UpdatedAt == new DateTime(2000, 1, 1))
		;
		
	db.Execute(sql);
}
```

Some Fetching Examples

``` csharp
using (var db = DB.Open())
{
	//	fetch by name (null if not found)
	var sql = Sql.Select<Customer>().Where(x => x.Name == "Stu");
	
	Customer stu = db.Query<Customer>(sql).FirstOrDefault();
	
	sql = Sql.Select<Customer>().Top(1).OrderByDescending(x => x.ID);
	
	//	fetch newest by identity
	Customer newest = db.Query<Customer>(sql).FirstOrDefault();
	
	sql = Sql.Select<Customer>().Top(10).OrderByDescending(x => x.ID);
	
	//	fetch newest 10 by identity
	IList<Customer> newestList = db.Query<Customer>(sql).ToList();
	
	sql = Sql.Select<Customer>().Where(x => x.UpdatedAt == null);
	
	//	fetch all those customers that have never been updated
	IList<Customer> neverUpdatedList = db.Query<Customer>(sql).ToList();
	
	sql = Sql.Select<Customer>().Min(x => x.CreatedAt);
	
	//	fetch first one created
	DateTime creation = db.Query<DateTime>(sql).FirstOrDefault();
	
	sql = Sql.Select<Customer>().Count();
	
	//	fetch first one created
	DateTime creation = db.Query<DateTime>(sql).FirstOrDefault();
}
```
