using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpSvn;

namespace Inb4.SvnClient
{
    public enum Access
    {
        None, Read, Write
    }

    /// Hello this is a comment
    public class Repository
    {
        public string SVNRoot { get; set; }
        public string Name { get; set; }
        public string RepositoryPath { get; set; }

        public Access AnonymousAccess
        {
            get
            {
                string val = readParameterValueFromConfig("svnserve.conf", "anon-access");
                return (Access)Enum.Parse(typeof(Access), val);
            }
        }

        public Access AuthenticatedAccess
        {
            get
            {
                string val = readParameterValueFromConfig("svnserve.conf", "auth-access");
                return (Access)Enum.Parse(typeof(Access), val);
            }
        }

        public Repository() { }

        public Repository(string repositoryname, string svnroot)
        {
            Name = repositoryname;
            SVNRoot = svnroot;
            RepositoryPath = Path.Combine(SVNRoot, Name);
        }

        public List<User> Users(string repositoryname = null)
        {
            string passwd = (repositoryname == null) ? RepositoryPath : Path.Combine(SVNRoot, repositoryname);
            passwd = Path.Combine(passwd, "conf", "passwd");

            List<User> users = new List<User>();

            using (var input = new StreamReader(passwd))
            {
                bool inUserSection = false;
                string user = input.ReadLine();
                while (user != null)
                {
                    if (!user.StartsWith("#"))
                    {
                        if (inUserSection && user.Contains("="))
                        {
                            string[] tmp = user.Split('='); // name = password
                            users.Add(new User(tmp[0].Trim(), tmp[1].Trim()));
                        }
                        inUserSection = inUserSection ? inUserSection : (user == "[users]");
                    }
                    user = input.ReadLine();
                }
            }

            return users;
        }

        public bool HasUser(string username)
        {
            foreach (User user in this.Users())
                if (username == user.Name)
                    return true;

            return false;
        }

        [Obsolete("Replaced by Repository.Create()", true)]
        public void Create(string name)
        {
            this.Create();
        }

        public void Create()
        {
            SvnRepositoryClient client = new SvnRepositoryClient();

            if (Directory.Exists(RepositoryPath))
                throw new Exception("Repository already exists");

            Directory.CreateDirectory(RepositoryPath);

            client.LoadConfiguration(RepositoryPath);

            if (!client.CreateRepository(RepositoryPath))
                throw new Exception("Failed to create new repository");

            usePasswd(true);
            anonAccess(Access.None);
            authAccess(Access.Write);
        }

        public void AddUser(User user)
        {
            string userdb = Path.Combine(this.SVNRoot, this.Name, "conf", "passwd");
            string tmpdb = userdb + ".tmp";

            List<string> newconf = new List<string>();
            using (var input = new StreamReader(userdb))
            {
                using (var output = new StreamWriter(tmpdb))
                {
                    string line = input.ReadLine();
                    while (line != null)
                    {
                        output.WriteLine(line);

                        if (line.StartsWith("[users]"))
                            output.WriteLine(string.Format("{0} = {1}", user.Name, user.Password));

                        line = input.ReadLine();
                    }
                }
            }

            File.Delete(userdb);
            File.Copy(tmpdb, userdb);
            File.Delete(tmpdb);
        }

        private string readParameterValueFromConfig(string configfile, string parameter_name)
        {
            string value = "none";
            string svnconf = Path.Combine(RepositoryPath, "conf", configfile);

            using (var input = new StreamReader(svnconf))
            {
                string line = input.ReadLine();
                while (line != null)
                {
                    if (line.Trim().StartsWith(parameter_name))
                    {
                        value = line.Split('=')[1].Trim();
                        break;
                    }
                    line = input.ReadLine();
                }
            }

            return string.Format("{0}{1}", value.Substring(0, 1).ToUpper(), value.Substring(1).ToLower());
        }

        private void replaceStringInConfig(string configfile, string match, string replacement)
        {
            string svnconf = Path.Combine(RepositoryPath, "conf", configfile);
            string tmpconf = Path.Combine(RepositoryPath, "conf", configfile + ".tmp");

            List<string> newconf = new List<string>();
            using (var input = new StreamReader(svnconf))
            {
                using (var output = new StreamWriter(tmpconf))
                {
                    string line = input.ReadLine();
                    while (line != null)
                    {
                        if (line.Contains(match))
                            output.WriteLine(replacement);
                        else
                            output.WriteLine(line);

                        line = input.ReadLine();
                    }
                }
            }

            File.Delete(svnconf);
            File.Copy(tmpconf, svnconf);
            File.Delete(tmpconf);
        }

        private void usePasswd(bool enabled)
        {
            string newpasswdline = "password-db = passwd";
            newpasswdline = enabled ? newpasswdline : "# " + newpasswdline;
            replaceStringInConfig("svnserve.conf", "password-db = passwd", newpasswdline);
        }

        private void anonAccess(Access access)
        {
            string str = string.Format("anon-access = {0}", Enum.GetName(typeof(Access), access).ToLower());
            replaceStringInConfig("svnserve.conf", "anon-access = ", str);
        }

        private void authAccess(Access access)
        {
            string str = string.Format("auth-access = {0}", Enum.GetName(typeof(Access), access).ToLower());
            replaceStringInConfig("svnserve.conf", "auth-access = ", str);
        }
    }
}
