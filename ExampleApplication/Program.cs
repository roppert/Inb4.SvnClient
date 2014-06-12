using System;
using System.Configuration;
using Inb4.SvnClient;

namespace ExampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            string svnroot = ConfigurationManager.AppSettings["SVNRoot"];
            Client client = new Client(svnroot);
            foreach (Repository repository in client.Repositories())
            {
                foreach (User user in repository.Users())
                    Console.WriteLine("{0}: {1}", repository.Name, user.Name);

                Console.WriteLine("---");
                Console.WriteLine(string.Format("{0} has user 'bamse': {1}", repository.Name, repository.HasUser("bamse")));
                Console.WriteLine("---");
            }

            //Console.Read(); // Hold off closing the console window
        }
    }
}
