using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Tmds.MDns;

namespace ServiceFinder
{
    class Program
    {
        static System.Collections.Concurrent.ConcurrentDictionary<string,ServiceAnnouncement> endpoints = new System.Collections.Concurrent.ConcurrentDictionary<string,ServiceAnnouncement>();

        static void Main(string[] args)
        {
            string serviceType = "_reach._tcp";
            if (args.Length >= 1)
            {
                serviceType = args[0];
            }

            ServiceBrowser serviceBrowser = new ServiceBrowser();
            serviceBrowser.ServiceAdded += onServiceAdded;
            serviceBrowser.ServiceRemoved += onServiceRemoved;
            serviceBrowser.ServiceChanged += onServiceChanged;

            Console.WriteLine("searching for reach devices... pres Enter to exit");
            serviceBrowser.StartBrowse(serviceType);
            Console.ReadLine();
        }

        static void onServiceChanged(object sender, ServiceAnnouncementEventArgs e)
        {
            addService(e.Announcement);
        }

        static void onServiceRemoved(object sender, ServiceAnnouncementEventArgs e)
        {
        }

        static void onServiceAdded(object sender, ServiceAnnouncementEventArgs e)
        {
            addService(e.Announcement);
        }

        static void addService(ServiceAnnouncement service)
        {
            if (service.Port != 80)
                return;

            foreach (var addr in service.Addresses)
                if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    Match m = service.Txt.Select(txt=>System.Text.RegularExpressions.Regex.Match(txt,@"device=(?<deviceType>.+)")).FirstOrDefault(_=>_.Success);
                    string deviceType = m!= default(Match) && m.Success ? m.Groups["deviceType"].Value : "<Unknown>";

                    string msg = $"{service.Instance} ({deviceType}) - http://{addr} on '{service.NetworkInterface.Name}'";
                    if (endpoints.TryAdd(msg,service))
                    {
                           Console.WriteLine(msg);
                    }
                }

        }
    }
}