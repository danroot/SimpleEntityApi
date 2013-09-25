using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Metadata.Edm;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Http.OData.Query;
using Newtonsoft.Json;

namespace SimpleEntityApi
{
    public abstract class SimpleEntityApi<T, TKey, TContext> : ApiController
        where T : class, new()
        where TContext : DbContext, new()
    {

        protected DbContext context;

        protected virtual DbContext GetDbContext()
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

        protected virtual HttpResponseMessage OnFinding(TKey id)
        {
            return null;
        }
        protected virtual HttpResponseMessage OnFound(TKey id, T item)
        {
            return null;
        }
        [HttpGet]
        [ActionName("Single")]
        public HttpResponseMessage Find(TKey id)
        {
            var result = OnFinding(id);
            if (result != null) return result;
            var db = GetDbContext();
            db.Configuration.ProxyCreationEnabled = false;
            var item = GetQuery().WhereKey(id).Single();
            result = OnFound(id, item);
            if (result != null) return result;
            return Request.CreateResponse(HttpStatusCode.OK, item);

        }

        protected virtual HttpResponseMessage OnDeleting(T id)
        {
            return null;
        }
        protected virtual HttpResponseMessage OnDeleted(TKey id, T item)
        {
            return null;
        }
        [HttpDelete]
        [ActionName("Single")]
        public HttpResponseMessage Delete(TKey id)
        {

            var db = GetDbContext();
            db.Configuration.ProxyCreationEnabled = false;
            var toDelete = GetQuery().WhereKey(id).Single();
            var result = OnDeleting(toDelete);
            if (result != null) return result;
            db.Set<T>().Remove(toDelete);
            db.SaveChanges();
            result = OnDeleted(id, toDelete);
            if (result != null) return result;
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        protected virtual HttpResponseMessage OnUpdating(TKey id, T item)
        {
            return null;
        }
        protected virtual HttpResponseMessage OnUpdated(TKey id, T item)
        {
            return null;
        }
        protected virtual HttpResponseMessage OnUpdateInvalid(TKey id, T item)
        {
            return null;
        }

        [HttpPut]
        [ActionName("Single")]
        public virtual HttpResponseMessage Update(TKey id, T item)
        {
            var db = GetDbContext();
            db.Configuration.ProxyCreationEnabled = false;
            var toUpdate = GetQuery().WhereKey(id).Single();
            UpdateModel(toUpdate, item);
            var result = OnUpdating(id, item);
            if (result != null) return result;
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                result = OnUpdated(id, toUpdate);
                if (result != null) return result;
                return Request.CreateResponse(HttpStatusCode.OK, toUpdate);
            }
            result = OnUpdateInvalid(id, item);
            if (result != null) return result;
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }


        [ActionName("Endpoint")]
        public virtual ODataMetadata<T> Get(ODataQueryOptions<T> options)
        {
            var db = GetDbContext();
            var settings = new ODataQuerySettings();
            if (options.InlineCount != null)
            {
                var all = PrepareQuery(db.Set<T>());
                if (options.Filter != null) all = (IQueryable<T>)options.Filter.ApplyTo(all, settings);
                var count = all.Count();
                var paged = (IQueryable<T>)options.ApplyTo(PrepareQuery(db.Set<T>()));
                return new ODataMetadata<T>(paged, count);
            }
            else
            {
                var all = (IQueryable<T>)options.ApplyTo(PrepareQuery(db.Set<T>()));
                return new ODataMetadata<T>(all, -1);
            }

        }

        [HttpGet]
        [ActionName("New")]
        public virtual T New()
        {
            return new T();
        }

        [HttpGet]
        [ActionName("Metadata")]
        public IEnumerable<EntityPropertyMetadata> Metadata()
        {
            //TODO: Maybe just use reflection here...
            var objContext = ((IObjectContextAdapter)GetDbContext()).ObjectContext;
            var workspace = objContext.MetadataWorkspace;
            //TODO: child items
            var properties = from meta in workspace.GetItems(DataSpace.CSpace)
                .Where(m => m.BuiltInTypeKind == BuiltInTypeKind.EntityType)
                             from p in (meta as EntityType).Properties
                             where p.DeclaringType.Name == (typeof(T).Name)
                             select new EntityPropertyMetadata(p, typeof(T));
            //select new
            //{
            //    EntityTypeName = p.DeclaringType.Name,
            //    PropertyName = p.Name,
            //    Nullable = p.Nullable,
            //    TypeUsageName = p.TypeUsage.EdmType.Name,
            //    DefaultValue = p.DefaultValue,
            //    Documentation = p.Documentation != null ? p.Documentation.LongDescription : null
            //};
            return properties;
        }

        [HttpGet]
        [ActionName("Help")]
        public HttpResponseMessage Help()
        {
            //TODO: add validation.
            var rootUrl = Request.RequestUri.ToString().Replace("/help", "");
            if (!rootUrl.StartsWith("http://localhost")) return new HttpResponseMessage(HttpStatusCode.NotFound);
            var result = new StringBuilder();
            result.Append("<html><head><title></title></head><body>");
            result.AppendFormat("<h1>Entity API for {0}</h1>", typeof(T).Name);
            result.AppendFormat("<p>This RESTful API allows working with {0} data in the database.</p>", typeof(T).Name);
            var objContext = ((IObjectContextAdapter)GetDbContext()).ObjectContext;
            var workspace = objContext.MetadataWorkspace;
            //TODO: child items
            var properties = from meta in workspace.GetItems(DataSpace.CSpace)
                .Where(m => m.BuiltInTypeKind == BuiltInTypeKind.EntityType)
                             from p in (meta as EntityType).Properties
                             where p.DeclaringType.Name == (typeof(T).Name)
                             select new EntityPropertyMetadata(p, typeof(T));

            result.Append("<h2>Properties</h2><ul>");
            foreach (var property in properties)
            {
                result.AppendFormat("<li>{0} {1} - {2}</li>",property.TypeUsageName, property.Name, property.DisplayName);
            }
            result.Append("</ul>");
         
            result.Append("<h2>Querying</h2>");
            result.Append("<p>This API serves up an <a href=\"http://www.odata.org/\">OData</a> compatible endpoint.  Make ajax GET requests to query using the OData URI syntax.</p>");
            result.AppendFormat("<b>Query:</b><a href=\"{0}?$filter={1} eq 'Bob'&$top=20\">{0}?$filter={1} eq 'Bob'&$top=20</a><br/>", rootUrl, properties.First(x => x.TypeUsageName == "String").Name);
            result.AppendFormat("<b>Get:</b><a href=\"{0}/123\">{0}/123</a><br/>", rootUrl);
            result.AppendFormat("<code>$.ajax({{url:'{0}/123',dataType:'json',contentType:'application/json'}}).success(function(data){{item=data}});</a></code>", rootUrl);


            result.AppendFormat("<h2>Creating a New {0}</h2>", typeof(T).Name );
            var ser = new JsonSerializer();
            var tw = new StringWriter();
            ser.Serialize(tw, New());
            result.AppendFormat("<code>var item = {0};</code>", tw.ToString());
            result.Append("<div>OR</div>");
            result.AppendFormat("<code>$.ajax({{url:'{0}/new',dataType:'json',contentType:'application/json'}}).success(function(data){{self.item=data;}});</code>", rootUrl);

            result.AppendFormat("<h2>Adding a {0}</h2><p>HTTP POST JSON data to add it to the database.</p>", typeof(T).Name);
            result.AppendFormat("<code>$.ajax({{url:'{0}',data:JSON.stringify(item),dataType:'json',contentType:'application/json', type:'POST'}}).success(function(data){{item=data}});</a></code>", rootUrl);

            result.AppendFormat("<h2>Updating a {0}</h2><p>HTTP PUT JSON data to update a {0} in the database.  You may pass the primary key of the item to update, but it is not required.</p>", typeof(T).Name);
            result.AppendFormat("<code>$.ajax({{url:'{0}',data:JSON.stringify(item),dataType:'json',contentType:'application/json', type:'PUT'}}).success(function(data){{item=data}});</a></code>", rootUrl);
            result.Append("<div>OR</div>");
            result.AppendFormat("<code>$.ajax({{url:'{0}/123',data:JSON.stringify(item),dataType:'json',contentType:'application/json', type:'PUT'}}).success(function(data){{item=data}});</a></code>", rootUrl);

            result.AppendFormat("<h2>Deleting a {0}</h2><p>HTTP DELETE JSON data to delete a {0}.  You may either pass the entire entity to delete, or just the primary key.</p>",typeof(T).Name);
            result.AppendFormat("<code>$.ajax({{url:'{0}',data:JSON.stringify(item),dataType:'json',contentType:'application/json', type:'DELETE'}});</a></code>", rootUrl);
            result.Append("<div>OR</div>");
            result.AppendFormat("<code>$.ajax({{url:'{0}/123',dataType:'json',contentType:'application/json', type:'DELETE'}});</a></code>", rootUrl);


            result.Append("<h2>Examples</h2><ul>");
            result.AppendFormat("<li>Query: <a href=\"{0}?$filter={1} eq 'Bob'\">{0}?$filter={1} eq 'Bob'</a></li>",rootUrl, properties.First(x => x.TypeUsageName == "String").Name);
            result.AppendFormat("<li>POST: $.ajax({{url:'{0}',data:JSON.stringify(item),dataType:'json',contentType:'application/json', type:'POST'}});</a></li>", rootUrl);

            result.Append("</ul>");
            result.Append("</body></html>");
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(result.ToString(), Encoding.UTF8, "text/html");

            return response;
        }


        protected virtual T GetOriginal(T entity)
        {

            var db = GetDbContext();
            return GetQuery().WhereKeysMatch(entity).Single(); //TODO: Authorize
            //var db = GetDbContext();
            //var entry = db.Entry(entity);
            //entry.State = EntityState.Modified;
            //var set = db.Set<T>();
            //set.Attach(entity);
            //return entity;
        }

        protected bool KeysEqual(IEnumerable<PropertyInfo> props, object source, object dest)
        {
            //TODO: cache or reuse these somehow?
            var keyProps =
                props.Where(
                    p =>
                        p.CustomAttributes.Any(
                            a => a.AttributeType == typeof(System.ComponentModel.DataAnnotations.KeyAttribute)) ||
                        p.Name == "Id").Select(k => k.Name);
            return
                keyProps.All(
                    k =>
                        (source.GetType().GetProperty(k).GetValue(source) ==
                         dest.GetType().GetProperty(k).GetValue(dest)));
        }

        [HttpPut]
        [ActionName("Endpoint")]
        public virtual HttpResponseMessage Put(T item)
        {

            HttpResponseMessage result;


            var db = GetDbContext();
            var original = GetOriginal(item);
            UpdateModel(original, item);
            result = OnUpdating(default(TKey), item);
            if (result != null) return result;
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                result = OnUpdated(default(TKey), original);
                if (result != null) return result;
                return Request.CreateResponse(HttpStatusCode.OK, original); //TODO: switch to these everywhere...
            }
            result = OnUpdateInvalid(default(TKey), item);
            if (result != null) return result;
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        private void UpdateModel(object original, object update)
        {
            var type = update.GetType();
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var prop in props)
            {


                var isList = prop.PropertyType.IsGenericType && prop.PropertyType.Name.Contains("IList");
                if (prop.CanWrite && !isList) prop.SetValue(original, prop.GetValue(update));
                if (!isList) continue;

                var sourceList = prop.GetValue(update) as IList;
                var destList = prop.GetValue(original) as IList;
                if (sourceList == null && destList != null)
                    destList.Clear(); //TODO: May need to opt-in for this: what if we don't _want_ to delete?
                if (destList == null && sourceList != null) destList = sourceList;
                if (sourceList == null) continue;
                if (destList.Count == 0 && sourceList.Count > 0)
                {
                    foreach (var sourceItem in sourceList)
                    {
                        destList.Add(sourceItem);
                    }
                }
                var toDelete = new List<object>();
                foreach (var destItem in destList)
                {
                    var matchingSourceItem =
                        sourceList.Cast<object>()
                            .SingleOrDefault(sourceItem => KeysEqual(props, destItem, sourceItem));
                    if (matchingSourceItem != null) UpdateModel(destItem, matchingSourceItem);
                    if (matchingSourceItem == null) toDelete.Add(destItem);
                }
                foreach (var itemToDelete in toDelete)
                {
                    destList.Remove(itemToDelete);
                }
                foreach (var sourceItem in sourceList)
                {
                    var matchingDestItem =
                        destList.Cast<object>()
                            .SingleOrDefault(destItem => KeysEqual(props, destItem, sourceItem));

                    if (matchingDestItem == null) destList.Add(sourceItem);
                }
            }
        }

        protected virtual HttpResponseMessage OnAdding(T item)
        {
            return null;
        }
        protected virtual HttpResponseMessage OnAdded(T item)
        {
            return null;
        }
        protected virtual HttpResponseMessage OnAddInvalid(T item)
        {
            return null;
        }
        [HttpPost]
        [ActionName("Endpoint")]
        public virtual HttpResponseMessage Post(T item)
        {
            var db = GetDbContext();
            item = db.Set<T>().Add(item);
            var result = OnAdding(item);
            if (result != null) return result;
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                result = OnUpdated(default(TKey), item);
                if (result != null) return result;
                return Request.CreateResponse(HttpStatusCode.OK, item); //TODO: switch to these everywhere...
            }
            result = OnAddInvalid(item);
            if (result != null) return result;
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }


        [HttpDelete]
        [ActionName("Endpoint")]
        public virtual HttpResponseMessage Delete(T item)
        {
            var result = OnDeleting(item);
            if (result != null) return result;

            var db = GetDbContext();
            var entry = db.Entry(item);
            db.Set<T>().Attach(item);
            entry.State = EntityState.Deleted;
            db.SaveChanges();
            result = OnDeleted(default(TKey), item);
            if (result != null) return result;
            return Request.CreateResponse(HttpStatusCode.OK);

        }


        //[HttpPut]
        //[ActionName("MultipleEndpoint")]
        //public virtual IEnumerable<T> Put(IEnumerable<T> entities)
        //{
        //    var db = GetDbContext();
        //    foreach (var entity in entities)
        //    {
        //        var entry = db.Entry(entity);
        //        db.Set<T>().Attach(entity);
        //        entry.State = EntityState.Modified;
        //    }
        //    db.SaveChanges();
        //    return entities;
        //}
        //[HttpPost]
        //[ActionName("MultipleEndpoint")]
        //public virtual IEnumerable<T> Post(IEnumerable<T> entities)
        //{
        //    var db = GetDbContext();
        //    foreach (var entity in entities)
        //    {
        //        db.Set<T>().Add(entity);
        //    }

        //    db.SaveChanges();
        //    return entities;
        //}
        //[HttpDelete]
        //[ActionName("MultipleEndpoint")]
        //public virtual IEnumerable<T> Delete(IEnumerable<T> entities)
        //{
        //    var db = GetDbContext();
        //    foreach (var entity in entities)
        //    {
        //        var entry = db.Entry(entity);
        //        db.Set<T>().Attach(entity);
        //        entry.State = EntityState.Deleted;
        //    }

        //    db.SaveChanges();
        //    return entities;
        //}

        protected override void Dispose(bool disposing)
        {
            if (context != null) context.Dispose();
            base.Dispose(disposing);
        }
    }
}