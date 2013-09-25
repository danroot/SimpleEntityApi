SimpleEntityApi
===============

SimpleEntityApi provides a simple Web API over Entity Framework classes, and javascript to help you consume your API in 
grids and forms.

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

Features:

* RESTful Add/Edit/Delete
* Validation returning ModelState errors
* Before/After Intercepts
* Javascript for hooking up Angular/Knockout/JQuery/VanillaJS

more docs soon!
