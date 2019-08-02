using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Contacts;
using Google.GData.Client;
using System.Text;
using System.Configuration;
using System.IO;
using Google.Apis.PeopleService.v1;
using Google.Apis.Services;
using Google.Apis.PeopleService.v1.Data;
using System.Net.NetworkInformation;

namespace WinFormsContacts
{
    class GContacts_Manager
    {
        static string applicationName = ConfigurationManager.AppSettings.Get("applicationName");
        //For contacts API
        private static OAuth2Parameters parameters;
        private static PeopleServiceService service;
        public Feed<Contact> feed;
        string credPath;
        string[] scopes = new string[] { "https://www.googleapis.com/auth/userinfo.profile", "https://www.googleapis.com/auth/contacts.readonly" };

        public bool Auth(string clientId, string clientSecret, string user = "user")
        {
            // view your basic profile info.
            credPath = System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/people");
            IPStatus status;
            try
            {
                Ping p = new Ping();

                PingReply pr = p.Send(@"google.com");
                status = pr.Status;
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret }
                                                                                             , scopes
                                                                                             , user
                                                                                             , CancellationToken.None
                                                                                             , new FileDataStore(credPath)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);

                // Create People API service.
                service = new PeopleServiceService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = applicationName,
                });
                return true;
            }
            catch (System.Net.Http.HttpRequestException)
            {
                MessageBox.Show("Проблемы с интернетом");
                return false;
            }
            catch (System.Net.NetworkInformation.PingException)
            {
                MessageBox.Show("Проблемы с интернетом");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        //For people API
        public List<Person> GetPeople(string pageToken=null, string user="user")
        {
            // Define parameters of request.
            PeopleResource.ConnectionsResource.ListRequest peopleRequest =
                    service.People.Connections.List("people/me");
            peopleRequest.PersonFields = "names,emailAddresses,photos,phoneNumbers,birthdays,interests,organizations";
            ListConnectionsResponse people=null;
            if (pageToken != null)
            {
                peopleRequest.PageToken = pageToken;
            }

            try
            {
                people = peopleRequest.Execute();
            }
            catch(Google.GoogleApiException)
            {
                File.Delete(Path.Combine(credPath, "Google.Apis.Auth.OAuth2.Responses.TokenResponse-"+user));
                MessageBox.Show("Необходимо дать согласие на обработку ваших данных, попробуйте еще раз.");    
                return null;
            }

            if (people != null && people.Connections != null && people.Connections.Count > 0)
            {
                foreach (var person in people.Connections)
                {
                    Console.WriteLine(person.Names != null ? (person.Names[0].DisplayName ?? "n/a") : "n/a");
                }

                if (people.NextPageToken != null)
                {
                    GetPeople(people.NextPageToken);
                }
                return people.Connections.ToList();
            }
            else
            {
                Console.WriteLine("No people found / end of list");
                return null;
            }
        }

        //Get my Profile
        public Person GetMyProfile()
        {
            PeopleResource.GetRequest peopleRequest =
    service.People.Get("people/me");
            peopleRequest.PersonFields = "names,photos";
            Person profile;
            try
            {
                profile = peopleRequest.Execute();
            }
            catch (System.Net.Http.HttpRequestException)
            {
                MessageBox.Show("Проблемы с интернетом");
                return null;
            }
            if (profile != null )
            {
                    Console.WriteLine(profile.Names != null ? (profile.Names[0].DisplayName ?? "n/a") : "n/a");
                return profile;
            }
            else
            {
                Console.WriteLine("didn't find myself");
                return null;
            }
        }


        //For contacts API
        public bool GetContacts()
        {
            try
            {
                RequestSettings settings = new RequestSettings(applicationName, parameters);
                ContactsRequest contactRequest = new ContactsRequest(settings);
                feed = contactRequest.GetContacts();
                return true;
            }
            catch (Exception a)
            {
                Console.WriteLine("A Google Apps error occurred.");
                Console.WriteLine();
                return false;
            }
        }


    }
}
