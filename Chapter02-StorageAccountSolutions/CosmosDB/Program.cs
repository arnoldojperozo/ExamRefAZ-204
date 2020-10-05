//C# .NET Core. Program.cs
using System;
using System.Linq;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using ch2_1_3_SQL.Model;
using System.Net;

namespace ch2_1_3_SQL
{
    class Program
    {
        private const string EndpointUri = "<PUT YOUR ENDPOINT URL HERE>";
        private const string Key = "<PUT YOUR COSMOS DB KEY HERE>";
        private CosmosClient client;
        private Database database;
        private Container container;

        static void Main(string[] args)
        {

            try
            {
                Program demo = new Program();
                demo.StartDemo().Wait();
            }
            catch (CosmosException ce)
            {
                Exception baseException = ce.GetBaseException();
                System.Console.WriteLine($"{ce.StatusCode} error ocurred: {ce.Message}, Message: {baseException.Message}");
            }
            catch (Exception ex)
            {
                Exception baseException = ex.GetBaseException();
                System.Console.WriteLine($"Error ocurred: {ex.Message}, Message: {baseException.Message}");
            }

        }

        private async Task StartDemo()
        {
            Console.WriteLine("Starting Cosmos DB SQL API Demo!");

            //Create a new demo database

            string databaseName = "demoDB_" + Guid.NewGuid().ToString().Substring(0, 5);

            this.SendMessageToConsoleAndWait($"Creating database {databaseName}...");

            this.client = new CosmosClient(EndpointUri, Key);
            this.database = await this.client.CreateDatabaseIfNotExistsAsync(databaseName);

            //Create a new demo collection inside the demo database.
            //This creates a collection with a reserved throughput. You can customize the options using a ContainerProperties object
            //This operation has pricing implications.
            string containerName = "collection_" + Guid.NewGuid().ToString().Substring(0, 5);


            this.SendMessageToConsoleAndWait($"Creating collection demo {containerName}...");

            this.container = await this.database.CreateContainerIfNotExistsAsync(containerName, "/LastName");

            //Create some documents in the collection
            Person person1 = new Person
            {
                Id = "Person.1",
                FirstName = "Santiago",
                LastName = "Fernandez",
                Devices = new Device[]
                {
                     new Device { OperatingSystem = "iOS", CameraMegaPixels = 7, Ram = 16, Usage = "Personal"},
                     new Device { OperatingSystem = "Android", CameraMegaPixels = 12, Ram = 64, Usage = "Work"}
                },
                Gender = "Male",
                Address = new Address
                {
                    City = "Seville",
                    Country = "Spain",
                    PostalCode = "28973",
                    Street = "Diagonal",
                    State = "Andalucia"
                },
                IsRegistered = true
            };


            await this.CreateDocumentIfNotExistsAsync(databaseName, containerName,person1);

            Person person2 = new Person
            {
                Id = "Person.2",
                FirstName = "Agatha",
                LastName = "Smith",
                Devices = new Device[]
                {

                     new Device { OperatingSystem = "iOS", CameraMegaPixels = 12,
                     Ram = 32, Usage = "Work"},
                     new Device { OperatingSystem = "Windows", CameraMegaPixels = 12,
                     Ram = 64, Usage = "Personal"}
                },
                Gender = "Female",
                Address = new Address
                {
                    City = "Laguna Beach",
                    Country = "United States",
                    PostalCode = "12345",
                    Street = "Main",
                    State = "CA"
                },
                IsRegistered = true
            };


            await this.CreateDocumentIfNotExistsAsync(databaseName, containerName,person2);

            //Make some queries to the collection
            this.SendMessageToConsoleAndWait($"Getting documents from the collection {containerName}...");

            //Find documents using LINQ

            IQueryable<Person> queryablePeople = this.container.GetItemLinqQueryable<Person>(true).Where(p => p.Gender == "Male");

            System.Console.WriteLine("Running LINQ query for finding men...");
            foreach (Person foundPerson in  queryablePeople)
            {
                System.Console.WriteLine($"\tPerson: {foundPerson}");
            }

            //Find documents using SQL

            var sqlQuery = "SELECT * FROM Person WHERE Person.Gender = 'Female'";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
            FeedIterator<Person> peopleResultSetIterator = this.container.GetItemQueryIterator<Person>(queryDefinition);

            System.Console.WriteLine("Running SQL query for finding women...");
            while (peopleResultSetIterator.HasMoreResults)
            {
                FeedResponse<Person> currentResultSet = await peopleResultSetIterator.ReadNextAsync();

                foreach (Person foundPerson in currentResultSet)
                {
                    System.Console.WriteLine($"\tPerson: {foundPerson}");
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            //Update documents in a collection
            this.SendMessageToConsoleAndWait($"Updating documents in the collection {containerName}...");
            person2.FirstName = "Mathew";
            person2.Gender = "Male";

            await this.container.UpsertItemAsync(person2);
            this.SendMessageToConsoleAndWait($"Document modified {person2}");

            //Delete a single document from the collection
            this.SendMessageToConsoleAndWait($"Deleting documents from the collection {containerName}...");

            PartitionKey partitionKey = new PartitionKey(person1.LastName);
            await this.container.DeleteItemAsync<Person>(person1.Id, partitionKey);
            this.SendMessageToConsoleAndWait($"Document deleted {person1}");

            //Delete created demo database and all its children elements
            this.SendMessageToConsoleAndWait("Cleaning-up your Cosmos DB account...");
            await this.database.DeleteAsync();
        }
        private void SendMessageToConsoleAndWait(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private async Task CreateDocumentIfNotExistsAsync(string database, string collection, Person person)
        {
            try
            {
                await this?.container.ReadItemAsync<Person>(person.Id, new PartitionKey(person.LastName));

                this.SendMessageToConsoleAndWait($"Document {person.Id} already exists in collection {collection}");
            }
            catch (CosmosException dce)
            {
                if (dce.StatusCode == HttpStatusCode.NotFound)
                {

                    await this?.container.CreateItemAsync<Person>(person, new PartitionKey(person.LastName));

                    this.SendMessageToConsoleAndWait($"Created new document {person.Id} in collection {collection}");
                }
            }
        }
    }
}