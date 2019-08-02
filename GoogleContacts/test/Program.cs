using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Contacts;
using Google.GData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static string clientId = "358176506527-t1cq3tqoqtgpr68ird609465j36cbo5b.apps.googleusercontent.com";
        static string clientSecret = "gjeF9GPqxje8a5k3rtoqcmLp";
        static string applicationName = "Console Client";

        static void Main(string[] args)
        {
            Auth();
        }
        public static void Auth()
        {
            string[] scopes = new string[] { "https://www.googleapis.com/auth/contacts.readonly" };     // view your basic profile info.
            try
            {
                // Use the current Google .net client library to get the Oauth2 stuff.
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret }
                                                                                             , scopes
                                                                                             , "4"
                                                                                             , CancellationToken.None
                                                                                             , new FileDataStore("users")).Result;

                // Translate the Oauth permissions to something the old client libray can read
                OAuth2Parameters parameters = new OAuth2Parameters();
                parameters.AccessToken = credential.Token.AccessToken;
                parameters.RefreshToken = credential.Token.RefreshToken;
                RunContactsSample(parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static void RunContactsSample(OAuth2Parameters parameters)
        {
            try
            {
                RequestSettings settings = new RequestSettings(applicationName, parameters);
                ContactsRequest contactRequest = new ContactsRequest(settings);
                Feed<Contact> feed = contactRequest.GetContacts();
                foreach (Contact contact in feed.Entries)
                {
                    Console.WriteLine(contact.Name.FullName);
                }
            }
            catch (Exception a)
            {
                Console.WriteLine("A Google Apps error occurred.");
                Console.WriteLine();
            }
            Console.ReadKey();
        }
    }
}
