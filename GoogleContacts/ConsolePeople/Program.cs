using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using System.IO;
//using Google.Apis.People.v1.Data;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Google.Apis.PeopleService.v1.Data;

namespace ConsolePeople
{

    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/people-dotnet-quickstart.json
        static string[] scopes = new[] { PeopleServiceService.Scope.ContactsReadonly};
        static string applicationName = "People API .NET Quickstart";

        static ClientSecrets secrets = new ClientSecrets()
        {
            ClientId = "358176506527-dnen7chgjvc5sudk5tegevcq4lgl69ke.apps.googleusercontent.com",
            ClientSecret = "otRXzSCrxrdNIyeu60CDjtm_"
        };

        static void Main(string[] args)
        {
            UserCredential credential;


            string credPath = System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/people-dotnet-quickstart");

            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                scopes,
                "user1",
                CancellationToken.None).Result;
            Console.WriteLine("Credential file saved to: " + credPath);

            // Create Drive API service.
            var service = new PeopleServiceService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });

            // List People.               
            Console.WriteLine("People:");
            GetPeople(service, null);

            Console.WriteLine("Done!");
            Console.Read();
        }

        static void GetPeople(PeopleServiceService service, string pageToken)
        {
            // Define parameters of request.
            PeopleResource.ConnectionsResource.ListRequest peopleRequest =
                    service.People.Connections.List("people/me");
            peopleRequest.PersonFields = "names,emailAddresses";

            if (pageToken != null)
            {
                peopleRequest.PageToken = pageToken;
            }

            ListConnectionsResponse people = peopleRequest.Execute();

            if (people != null && people.Connections != null && people.Connections.Count > 0)
            {
                foreach (var person in people.Connections)
                {
                    Console.WriteLine(person.Names != null ? (person.Names[0].DisplayName ?? "n/a") : "n/a");
                }

                if (people.NextPageToken != null)
                {
                    GetPeople(service, people.NextPageToken);
                }
            }
            else
            {
                Console.WriteLine("No people found / end of list");
                return;
            }
        }
    }
}