namespace SimpleEntityApi.Web.Controllers
{
    public class InvoicesController : AnyViewController //SimpleEntityController<Invoice, InvoiceDbContext>
    {

     

        //protected override IQueryable<Invoice> PrepareQuery(DbSet<Invoice> query)
        //{
        //    return query.Include(x => x.LineItems);
        //}

        //public ActionResult LoadTestData()
        //{
        //    var db = this.GetDbContext();
        //    Enumerable.Range(0,1000).ForEach(x => db.Invoices.Add(new Invoice()
        //    {
        //        CustomerName = "Customer " + x,
        //        CustomerCompany = "Company " + x,
        //        CustomerStreet1 = "Street " + x,
        //        Total = 2.3*x
        //    }));
        //    db.SaveChanges();
        //    return new EmptyResult();
        //}
    }
}