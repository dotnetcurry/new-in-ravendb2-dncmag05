using System.Linq;
using Raven.Client.Document;
using Raven.Client.Indexes;
using RavenDBArticle.Model;
using Xunit;

namespace RavenDBArticle
{
    public class WithMapReduce
    {
        public class ProductSalesByZip : AbstractIndexCreationTask<Order, ProductSalesByZip.Result>
        {
            public class Result
            {
                public string Zip { get; set; }
                public string ProductId { get; set; }
                public int Count { get; set; }
            }

            public ProductSalesByZip()
            {
                Map = orders =>
                      from order in orders
                      let zip = LoadDocument<Customer>(order.CustomerId).ZipCode
                      from p in order.ProductIds
                      select new
                      {
                          Zip = zip,
                          ProductId = p,
                          Count = 1
                      };
                Reduce = results =>
                         from result in results
                         group result by new { result.Zip, result.ProductId }
                             into g
                             select new
                             {
                                 g.Key.Zip,
                                 g.Key.ProductId,
                                 Count = g.Sum(x => x.Count)
                             };
            }
        }

        [Fact]
        public void CanUseReferencesFromMapReduceMap()
        {
            using (var store = new DocumentStore() { ConnectionStringName = "RavenDB" })
            {
                store.Initialize();

                new ProductSalesByZip().Execute(store);

                using (var session = store.OpenSession())
                {
                    session.Store(new Product { Name = "Milk", Id = "products/milk" });
                    session.Store(new Product { Name = "Sugar", Id = "products/sugar" });

                    session.Store(new Customer { ZipCode = "1234", Name = "Gregor", Id = "customers/gregor" });
                    session.Store(new Customer { ZipCode = "4321", Name = "Mohammed", Id = "customers/mohammed" });

                    session.Store(new Order { CustomerId = "customers/gregor", ProductIds = new[] { "products/milk" } });
                    session.Store(new Order { CustomerId = "customers/gregor", ProductIds = new[] { "products/milk" } });
                    session.Store(new Order { CustomerId = "customers/gregor", ProductIds = new[] { "products/sugar", "products/sugar" } });

                    session.Store(new Order { CustomerId = "customers/mohammed", ProductIds = new[] { "products/sugar" } });
                    session.Store(new Order { CustomerId = "customers/mohammed", ProductIds = new[] { "products/sugar", "products/milk" } });

                    session.SaveChanges();
                }

                using (var session = store.OpenSession())
                {
                    var results = session.Query<ProductSalesByZip.Result, ProductSalesByZip>()
                        .Customize(x => x.WaitForNonStaleResults())
                        .OrderBy(x => x.Zip).ThenBy(x => x.ProductId)
                        .ToList();

                    Assert.Equal(4, results.Count);

                    Assert.Equal("1234", results[0].Zip);
                    Assert.Equal("products/milk", results[0].ProductId);

                    Assert.Equal("1234", results[1].Zip);
                    Assert.Equal("products/sugar", results[1].ProductId);

                    Assert.Equal("4321", results[2].Zip);
                    Assert.Equal("products/sugar", results[2].ProductId);

                    Assert.Equal("4321", results[3].Zip);
                    Assert.Equal("products/milk", results[3].ProductId);
                }
            }
        }
    }
}
