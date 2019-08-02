using Google.Apis.PeopleService.v1.Data;
using Google.Contacts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsContacts
{
    public partial class ContactBookForm : Form
    {
        static string clientId = ConfigurationManager.AppSettings.Get("clientId");
        static string clientSecret = ConfigurationManager.AppSettings.Get("clientSecret");
        string user = "user";
        string credPath= Path.Combine(System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.Personal), ".credentials/people");
        
        Person myProfile;
        static List<Person> contactList=new List<Person>();
        public ContactBookForm()
        {
            InitializeComponent();
        }

        private void authBtn_Click(object sender, EventArgs e)
        {
            user = textBox1.Text;
            GContacts_Manager GCM = new GContacts_Manager();
            if (user == "")
                MessageBox.Show("Необходимо придумать логин");
            else
            if (GCM.Auth(clientId, clientSecret, user))
            {
                if ((myProfile = GCM.GetMyProfile()) == null) return;
                pictureBoxUser.Load(myProfile.Photos[0].Url);
                labelUser.Text = "Пользователь: " + myProfile.Names[0].DisplayName;
                listBoxUser.Items.Clear();

                if ((contactList=GCM.GetPeople(null,user)) == null)
                    return;
                contactList = contactList.Select(x => x).Where(x => x.Names != null && x.Names[0].DisplayName != null).ToList();
                listBoxUser.Items.AddRange(contactList.Select(x => x.Names[0].DisplayName).ToArray());
            }
        }

        private void listBoxUser_SelectedIndexChanged(object sender, EventArgs e)
        {

            Person person = new Person();
            person = contactList[listBoxUser.SelectedIndex];
            richTextBox1.Clear();
            labelContact.Text = person.Names[0].DisplayName;
            pictureBoxContact.Load(person.Photos[0].Url);
            richTextBox1.Text += (person.EmailAddresses != null && person.EmailAddresses[0].Value != null) ?
                 $"Почта: { person.EmailAddresses[0].Value }\n" : $"Почта:(нет)\n";
            richTextBox1.Text += (person.PhoneNumbers != null && person.PhoneNumbers[0].Value != null) ?
                 $"Телефон: { person.PhoneNumbers[0].Value }\n" : $"Телефон:(нет)\n";
            richTextBox1.Text += (person.Birthdays != null && person.Birthdays[0].Date != null) ?
                $"День рождения: { person.Birthdays[0].Date.Day }.{ person.Birthdays[0].Date.Month }.{ person.Birthdays[0].Date.Year }\n" : $"День рождения:(нет)\n";
            richTextBox1.Text += (person.Organizations != null && person.Organizations[0].Name != null) ?
                 $"Организации: { person.Organizations[0].Name }\n" : $"Организации:(нет)\n";
            richTextBox1.Text += (person.Interests != null && person.Interests[0].Value != null) ?
                 $"Интересы: { person.Interests[0].Value }\n" : $"Интересы:(нет)\n";


        }

        private void ContactBookForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(credPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }
    }
}
