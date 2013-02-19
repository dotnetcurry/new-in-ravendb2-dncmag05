using System;
using System.Reflection;
using Raven.Abstractions.Data;
using Raven.Client.Document;
using Raven.Client.Indexes;
using RavenDBArticle.Model;

namespace RavenDBArticle
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var documentStore = new DocumentStore() { ConnectionStringName = "RavenDB" })
            {
                documentStore.Initialize();

                IndexCreation.CreateIndexes(Assembly.GetAssembly(typeof(Album)), documentStore);

                CheckForAlbumChanges(documentStore);

                DoBulkInsert(documentStore);
            }
        }

        /// <summary>
        /// Write to the console if an Album document changes.
        /// </summary>
        /// <param name="documentStore"></param>
        private static void CheckForAlbumChanges(DocumentStore documentStore)
        {
            documentStore.Changes().ForDocumentsStartingWith("Albums/")
                .Subscribe(change =>
                    {
                        if (change.Type == DocumentChangeTypes.Put)
                        {
                            Console.WriteLine("An Album has changed on the server!!");
                        }
                    });

            Console.WriteLine("Running :)");
            Console.Read();
        }

        /// <summary>
        /// Bulk Insert 1000 Albums using the BulkInsert API
        /// </summary>
        /// <param name="documentStore"></param>
        private static void DoBulkInsert(DocumentStore documentStore)
        {
            using (BulkInsertOperation bulkInsert = documentStore.BulkInsert())
            {
                for (int i = 0; i < 1000; i++)
                {
                    bulkInsert.Store(new Album
                        {
                            Title = "Title #" + i,
                            Price = 5.99
                        });
                }
            }
        }
    }
}
