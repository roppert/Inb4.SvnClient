using System;
using System.Configuration;
using Inb4.SvnClient;
using System.IO;

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

            Repository newRepo = new Repository("NewRepo123", client.SVNRoot);
            newRepo.Create();

            Console.WriteLine("New repo created: {0}", newRepo.Name);

            // Now fix attributes so we can delete and clean up after ourselfes
            foreach (string entry in Directory.GetFileSystemEntries(newRepo.RepositoryPath, "*", SearchOption.AllDirectories))
            {
                Console.WriteLine("Fixing {0}", entry);
                File.SetAttributes(entry, FileAttributes.Normal);
            }
            Directory.Delete(newRepo.RepositoryPath, true);

            Console.Read(); // Hold off closing the console window
        }
    }
}
