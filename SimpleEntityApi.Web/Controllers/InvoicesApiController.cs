using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using SimpleEntityApi.Web.Models;

namespace SimpleEntityApi.Web.Controllers
{
    public class InvoicesApiController : SimpleEntityApi<Invoice, int, InvoiceDbContext>
    {
      
        protected override IQueryable<Invoice> PrepareQuery(DbSet<Invoice> query)
        {
            return query.Include(x => x.LineItems);
        }

        protected override HttpResponseMessage OnAdding(Invoice item)
        {
     
            return base.OnAdding(item);
        }

        //protected override Invoice GetOriginal(Invoice entity)
        //{
        //    return GetQuery()
        //        .Include(x => x.LineItems)
        //        .Single(x => x.Id == entity.Id);
        //    //return base.GetOriginal(entity);
        //}
    }
}