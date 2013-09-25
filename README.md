SimpleEntityApi
===============

SimpleEntityApi provides a simple Web API over Entity Framework classes.

Simplest thing that works:

     install-package SimpleEntityApi -pre
     ...
     public class Todo { public int Id{get;set;} public string Title {get;set;}}
     public class TodoDbContext: DbContext { public DbSet<Todo> Todos {get;set;}}
     public class TodoController: SimpleEntityApi<Todo,int,TodoDbContext>{}
     ...
     //In WebApiConfig.cs:
    SimpleEntityApiRoutes.RegisterRoutes(config, false);

That's it!  Now you have an OData Web API endpoint for adding Todos!

Also supports:

* RESTful Add/Edit/Delete
* Validation returning ModelState errors
* Before/After Intercepts

Docs soon!
