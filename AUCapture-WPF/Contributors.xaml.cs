using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Themes;
using HandyControl.Tools;
using MahApps.Metro.Controls;
using Octokit;

namespace AUCapture_WPF
{
    /// <summary>
    /// Interaction logic for Contributors.xaml
    /// </summary>
    public partial class Contributors
    {
        public Queue<BetterRepoContributor> RepoContributorsToBeAdded = new();
        public ObservableCollection<BetterRepoContributor> CurrentContributors = new();
        public Contributors(bool dark)
        {
            InitializeComponent();
            if (dark)
            {
                SharedResourceDictionary.SharedDictionaries.Clear();
                ResourceHelper.GetTheme("HandyTheme", Resources).Skin = SkinType.Dark;
                
            }
            
            DataContext = CurrentContributors;
            AddContributors();

        }

        public async void AddContributors()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("AmongUsCapture"));
            var autoMuteUsOrgRepos = new List<int>{294825566, 295776544};
            var ListOfContribs = new List<BetterRepoContributor>();
            foreach (var repo in autoMuteUsOrgRepos)
            {
                var RepoContributors = await client.Repository.GetAllContributors(repo);
                foreach (var contributor in RepoContributors)
                {
                    if (ListOfContribs.All(x => x.HtmlUrl != contributor.HtmlUrl))
                    {
                        ListOfContribs.Add(new BetterRepoContributor(contributor.Id,contributor.Contributions, contributor.AvatarUrl, contributor.HtmlUrl, contributor.Login));
                    }
                    else
                    {
                        var indexOfReal = ListOfContribs.FindIndex(x => x.HtmlUrl == contributor.HtmlUrl);
                        ListOfContribs[indexOfReal].Contributions += contributor.Contributions;
                    }
                }
            }
            var tempList = ListOfContribs.Where(x => x.Id != 49699333).OrderBy(x=>x.Contributions).ToList();
            RepoContributorsToBeAdded = new Queue<BetterRepoContributor>(tempList);
            CurrentContributors.Add(RepoContributorsToBeAdded.Dequeue());
            
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
            dispatcherTimer.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            
            if (RepoContributorsToBeAdded.Count != 0)
            {
                var c = RepoContributorsToBeAdded.Dequeue();
                CurrentContributors.Add(c);
                Console.WriteLine(c.Login);
                
                
            }
            else
            {
                var timer = sender as DispatcherTimer;
                timer.Stop();
            }
            

        }
        public static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                // throw 
            }
        }
        private void Gravatar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tar = sender as Gravatar;
            var tarContext = tar.DataContext as RepositoryContributor;
            OpenBrowser(tarContext.HtmlUrl);
        }
    }

    public class BetterRepoContributor
    {
        public BetterRepoContributor(long id, int contributions, string avatarUrl, string htmlUrl, string login)
        {
            Id = id;
            Contributions = contributions;
            AvatarUrl = avatarUrl;
            HtmlUrl = htmlUrl;
            Login = login;
        }

        public long Id { get; set; }
        public int Contributions { get; set; }
        public string AvatarUrl { get; set; }
        public string HtmlUrl { get; set; }
        public string Login { get; set; }
    }
}
