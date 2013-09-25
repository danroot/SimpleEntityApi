using System;
using System.Data.Entity.Migrations.Infrastructure;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using SimpleEntityApi.Web.Models;

namespace SimpleEntityApi.Web.Controllers
{

    public class TodoApiController : SimpleEntityApi<TodoItem, int, TodoDbContext>
    {
        
    }

    public class TodoController : SimpleEntityController<TodoItem, TodoDbContext>
    {
        
    }

    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

    }

    //public class InvoicesController : SimpleEntityController
    //{
        
    //}
}
