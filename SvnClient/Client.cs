using System;
using System.Collections.Generic;
using System.IO;
using SharpSvn;

namespace Inb4.SvnClient
{
    public class Client
    {
        public string SVNRoot { get; set; }

        public List<Repository> Repositories()
        {
            List<Repository> dirs = new List<Repository>();
            var _dirs = Directory.EnumerateDirectories(this.SVNRoot);

            foreach (string dir in _dirs)
            {
                dirs.Add(new Repository(Path.GetFileName(dir), this.SVNRoot));
            }

            return dirs;
        }

        public List<User> AllUsers()
        {
            List<User> users = new List<User>();

            foreach(Repository repository in this.Repositories())
            {
                foreach (User user in repository.Users())
                {
                    users.Add(user);
                }
            }

            return users;
        }

        // TODO: Rename to AddGlobalUser(string username, string password)
        public void AddUserToAllRepositories(string username, string password)
        {
            User newuser = new User(username, password);

            foreach(Repository repository in this.Repositories())
            {
                repository.AddUser(newuser);
            }
        }

        public Client() { }

        public Client(string svnroot)
        {
            SVNRoot = svnroot;
        }

    }

}
