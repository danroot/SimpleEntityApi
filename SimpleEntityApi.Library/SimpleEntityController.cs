using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using System.Web.Mvc;

namespace SimpleEntityApi
{
    public abstract class SimpleEntityController<T, TContext> : Controller
        where T : class, new()
        where TContext : DbContext, new()
    {
       
        public bool IsJsonRequest()
        {
            return Request.ContentType == "application/json";
        }

        protected TContext context;
        protected virtual TContext GetDbContext()
        {
            return context ?? (context = new TContext());
        }

        protected virtual IQueryable<T> PrepareQuery(DbSet<T> query)
        {
            return query;
        }

        protected IQueryable<T> GetQuery()
        {
            return PrepareQuery(GetDbContext().Set<T>());
        }
            
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [System.Web.Http.Queryable]
        public ActionResult Get(ODataQueryOptions<T> options)
        {            
            if (!IsJsonRequest()) return View();
           
            ODataMetadata<T> model;
            var db = GetDbContext();
            var settings = new ODataQuerySettings();
            if (options.InlineCount != null)
            {
                var all = PrepareQuery(db.Set<T>());
                if (options.Filter != null) all = (IQueryable<T>) options.Filter.ApplyTo(all, settings);
                var count = all.Count();
                var paged = (IQueryable<T>) options.ApplyTo(PrepareQuery(db.Set<T>()));
                model = new ODataMetadata<T>(paged, count);
            }
            else
            {
                var all = (IQueryable<T>) options.ApplyTo(PrepareQuery(db.Set<T>()));
                model = new ODataMetadata<T>(all, -1);
            }                     
            return Json(model);
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post(T item)
        {
            var db = GetDbContext();
            item = db.Set<T>().Add(item);
            db.SaveChanges();

            if (IsJsonRequest())
            {
                return Json(item);
            }
            return View(item);
        }

        [HttpPut]
        [ActionName("Index")]
        public ActionResult Put(T item)
        {
            var db = GetDbContext();
            var entry = db.Entry(item);
            entry.State= EntityState.Modified;
            UpdateModel(item);            
            db.SaveChanges();

            if (IsJsonRequest())
            {
                return Json(item);
            }
            return View(item);
        }

        [HttpDelete]
        [ActionName("Index")]
        public ActionResult Delete(T item)
        {
            var db = GetDbContext();
            item = db.Set<T>().Remove(item);
            db.SaveChanges();

            if (IsJsonRequest())
            {
                return Json(item);
            }
            return View(item);
        }


        [HttpGet]
        public virtual ActionResult New()
        {
            var model = new T();
            if (IsJsonRequest())
            {
                return Json(model);
            }
            return View(model);
        }

        public virtual ActionResult Edit()
        {
            return View("Edit");
        }


    }
}