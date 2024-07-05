using Dalamud.Game.Gui.PartyFinder.Types;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace Somethingway
{
    internal class Handler : IDisposable
    {
        private const string ServerUrl = "http://127.0.0.1:5000/";
        private Plugin Plugin { get; set; }
        private IPartyFinderGui PartyFinder { get; set; }
        private IFramework Framework { get; set; }
        internal List<Listing> pfListingsQueue { get; set; }
        private bool pending = false;
        private IPluginLog log;
        private IGameGui GameGui;
        private Stopwatch cooldown;
        private Stopwatch lockout;
        public Handler(Plugin plugin)
        {
            this.Plugin = plugin;
            this.PartyFinder = Plugin.PartyFinder;
            this.Framework = Plugin.Framework;
            this.pfListingsQueue = new List<Listing>();
            this.log = Plugin.PluginLog;
            this.GameGui = Plugin.GameGui;
            PartyFinder.ReceiveListing += this.OnPartyListing;
            Framework.Update += this.OnFrame;
            cooldown = new Stopwatch();
            lockout = new Stopwatch();
            cooldown.Start();
        }

        public void Dispose()
        {
            PartyFinder.ReceiveListing -= this.OnPartyListing;
            Framework.Update -= this.OnFrame;
        }

        public void OnPartyListing(IPartyFinderListing pfListing, IPartyFinderListingEventArgs args)
        {
            lockout.Restart();
            Listing listing = new Listing(pfListing);
            pfListingsQueue.Add(listing);
            pending = true;
        }

        public void OnFrame(IFramework framework)
        {
            if (!pending || cooldown.ElapsedMilliseconds < 10000 || lockout.ElapsedMilliseconds < 1000)
            {
                return;
            }

            // Make top level object a dictionary 
            Dictionary<string, List<Listing>> json = new Dictionary<string, List<Listing>>();
            json.Add("Listings", pfListingsQueue);
            var payload = JsonConvert.SerializeObject(json);

            UploadJson(payload);
            pending = false;
            pfListingsQueue.Clear();
            cooldown.Restart();
        }

        private async void UploadJson(string payload)
        {
            using (var client = new HttpClient())
            {
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                try
                {
                    var response = await client.PostAsync(ServerUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        log.Info("PF listing data sent.");
                    }
                } catch (Exception e)
                {
                    log.Error("Unable to send PF listing data. " + e);
                }
            }
        }
    }
}
