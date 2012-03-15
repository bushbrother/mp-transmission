// mp-transmission
// http://code.google.com/p/mp-transmission/
// Copyright (C) 2012 Laurie R
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

/* Class Description here ... */

using System;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using mptransmission.Settings;
using System.Net;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using Jayrock;
using System.Collections;
using System.Timers;

namespace mptransmission
{
    [PluginIcons("mptransmission.Icon.png", "mptransmission.Icon_Disabled.png")]
    public class mptransmission : GUIWindow, ISetupForm
    {
        [SkinControlAttribute(5)] public GUIListControl torrentList = null;
        System.Timers.Timer aTimer = new System.Timers.Timer();

        public mptransmission()
        {
            
        }

        #region Constants

        /// <summary>
        /// Section in the MediaPortal config for this plugin
        /// </summary>
        public const string ConfigSection = "mptransmission";

        /// <summary>
        /// Version of the plugin
        /// </summary>
        public const string Version = "0.1 ALPHA";

        #endregion

        #region ISetupForm Members

        // Returns the name of the plugin which is shown in the plugin menu
        public string PluginName()
        {
            return "mp-transmission";
        }

        // Returns the description of the plugin is shown in the plugin menu
        public string Description()
        {
            return "A Plugin To Interface To Transmission Daemon";
        }

        // Returns the author of the plugin which is shown in the plugin menu
        public string Author()
        {
            return "Bushbrother";
        }

        // show the setup dialog
        public void ShowPlugin()
        {
            SetupForm setup = new SetupForm();
            setup.ShowDialog();
        }

        // Indicates whether plugin can be enabled/disabled
        public bool CanEnable()
        {
            return true;
        }

        // Get Windows-ID
        public int GetWindowId()
        {
            // WindowID of windowplugin belonging to this setup
            // enter your own unique code
            return 56345;
        }

        // Indicates if plugin is enabled by default;
        public bool DefaultEnabled()
        {
            return true;
        }

        // indicates if a plugin has it's own setup screen
        public bool HasSetup()
        {
            return true;
        }

        /// <summary>
        /// If the plugin should have it's own button on the main menu of Mediaportal then it
        /// should return true to this method, otherwise if it should not be on home
        /// it should return false
        /// </summary>
        /// <param name="strButtonText">text the button should have</param>
        /// <param name="strButtonImage">image for the button, or empty for default</param>
        /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
        /// <param name="strPictureImage">subpicture for the button or empty for none</param>
        /// <returns>true : plugin needs it's own button on home
        /// false : plugin does not need it's own button on home</returns>

        public bool GetHome(out string strButtonText, out string strButtonImage,
          out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = PluginName();
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = String.Empty;
            return true;
        }

        // With GetID it will be an window-plugin / otherwise a process-plugin
        // Enter the id number here again
        public override int GetID
        {
            get
            {
                return 56345;
            }

            set
            {

            }
        }

        #endregion

        #region Skin

        public override bool Init()
        {
            LocalSettings.Load();
            GUIPropertyManager.SetProperty("#numDownloads", "0");
            GUIPropertyManager.SetProperty("#numPausedDownloads", "0");
            GUIPropertyManager.SetProperty("#uploadSpeedTotal", UnitConvert.TransferSpeedToString(0));
            GUIPropertyManager.SetProperty("#downloadSpeedTotal", UnitConvert.TransferSpeedToString(0));
            return Load(GUIGraphicsContext.Skin+@"\mptransmission.xml");
        }

        protected override void OnPageLoad()
        {
            activeTorrents();
        }

        private void activeTorrents()
        {
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            // Set the Interval to settings value.
            if (LocalSettings.refreshRate == null)
            {
                aTimer.Interval = 5000;
            }
            if (LocalSettings.refreshRate == "5 Seconds")
            {
                aTimer.Interval = 5000;
            }
            if (LocalSettings.refreshRate == "10 Seconds")
            {
                aTimer.Interval = 10000;
            }
            if (LocalSettings.refreshRate == "15 Seconds")
            {
                aTimer.Interval = 15000;
            }
            if (LocalSettings.refreshRate == "20 Seconds")
            {
                aTimer.Interval = 20000;
            }
            if (LocalSettings.refreshRate == "25 Seconds")
            {
                aTimer.Interval = 25000;
            }
            if (LocalSettings.refreshRate == "30 Seconds")
            {
                aTimer.Interval = 30000;
            }
                aTimer.Start();
            }


        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Connect();
        }

        #endregion

        public void Connect()
        {
                var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
                var client = new TransmissionClient(url);
                string[] torrentNames = new string[100];
                string[] torrentPercent = new string[100];
                string[] torrentSize = new string[100];
                string[] torrentPeers = new string[100];
                string[] torrentLeechers = new string[100];
                string[] torrentSeeds = new string[100];
                string[] torrentEta = new string[100];
                JsonObject session = (JsonObject)client.Invoke("session-stats", null);
                JsonNumber num = (JsonNumber)session["activeTorrentCount"];
                int numTorrents = (int)num;
                JsonNumber numPaused = (JsonNumber)session["pausedTorrentCount"];
                int pausedTorrents = (int)numPaused;
                JsonNumber download = (JsonNumber)session["downloadSpeed"];
                int downSpeed = (int)download;
                JsonNumber upload = (JsonNumber)session["uploadSpeed"];
                int upSpeed = (int)upload;
                var torrent = (IDictionary)client.Invoke("torrent-get", new { fields = new[] { "name", "percentDone", "sizeWhenDone", "peersConnected", "peersGettingFromUs", "peersSendingToUs", "eta" } }, null);
                var i = 0;
                foreach (IDictionary torrents in (IList)torrent["torrents"])
                {
                    if (i < numTorrents)
                    {
                        torrentNames[i] = (string)torrents["name"];
                        JsonNumber percent = (JsonNumber)torrents["percentDone"];
                        double tempPercent = (double)percent;
                        torrentPercent[i] = tempPercent.ToString("0.00%");
                        JsonNumber size = (JsonNumber)torrents["sizeWhenDone"];
                        double tempSize = (double)size;
                        torrentSize[i] = tempSize.ToString();
                        JsonNumber peers = (JsonNumber)torrents["peersConnected"];
                        double tempPeers = (double)peers;
                        torrentPeers[i] = tempPeers.ToString();
                        JsonNumber leechers = (JsonNumber)torrents["peersGettingFromUs"];
                        double tempLeechers = (double)leechers;
                        torrentLeechers[i] = tempLeechers.ToString();
                        JsonNumber seeds = (JsonNumber)torrents["peersSendingToUs"];
                        double tempSeeds = (double)seeds;
                        torrentSeeds[i] = tempSeeds.ToString();
                        JsonNumber eta = (JsonNumber)torrents["eta"];
                        long tempEta = (long)eta;
                        torrentEta[i] = UnitConvert.TimeRemainingToString(tempEta).ToString();
                        i++;
                    }
                }
            GUIPropertyManager.SetProperty("#numDownloads", numTorrents.ToString("0"));
            GUIPropertyManager.SetProperty("#numPausedDownloads", pausedTorrents.ToString("0"));
            GUIPropertyManager.SetProperty("#uploadSpeedTotal", UnitConvert.TransferSpeedToString(upSpeed));
            GUIPropertyManager.SetProperty("#downloadSpeedTotal", UnitConvert.TransferSpeedToString(downSpeed));
            

            if (torrentList.ListItems.Count == 0)
            {
                PopulateList(torrentNames, torrentPercent, numTorrents, pausedTorrents, torrentEta, torrentPeers, torrentLeechers, torrentSeeds);
            }
            if (torrentList.ListItems.Count < (numTorrents + pausedTorrents))
            {
                rePopulateList(torrentNames, torrentPercent, numTorrents, pausedTorrents, torrentEta, torrentPeers, torrentLeechers, torrentSeeds);
            }
            if (torrentList.ListItems.Count > (numTorrents + pausedTorrents))
            {
                rePopulateList(torrentNames, torrentPercent, numTorrents, pausedTorrents, torrentEta, torrentPeers, torrentLeechers, torrentSeeds);
                PopulateList(torrentNames, torrentPercent, numTorrents, pausedTorrents, torrentEta, torrentPeers, torrentLeechers, torrentSeeds);
            }
            else
            {
                updateList(torrentNames, torrentPercent, numTorrents, pausedTorrents);
            }
        }

        private void PopulateList(string[] names, string[] percent, int active, int paused, string[] eta, string[] peers, string[] leechers, string[] seeds)
        {
            torrentList.Clear();
            int i = 0;
            while (i < (active+paused))
            {
                GUIListItem item = new GUIListItem();
                item.Label = names[i];
                item.Label2 = percent[i];
                item.Label3 = eta[i];
                string temp = string.Format("S-{0}({1}) ~ L-{2}",peers[i],seeds[i],leechers[i]);
                item.Label3 = temp;
                torrentList.Add(item);
                i++;
            }
        }

        private void updateList(string[] names, string[] percent, int active, int paused)
        {
            int i = 0;
            while (i < (active + paused))
            {
                torrentList.ListItems[i].Label = names[i];
                torrentList.ListItems[i].Label2 = percent[i];
                i++;
            }
        }

        protected override void OnPageDestroy(int newWindowId)
        {
            base.OnPageDestroy(newWindowId);
            aTimer.Elapsed -= new ElapsedEventHandler(OnTimedEvent);
            aTimer.Stop();
        }

        private void rePopulateList(string[] names, string[] percent, int active, int paused, string[] eta, string[] peers, string[] leechers, string[] seeds)
        {
            int i = torrentList.ListItems.Count;
            if (i < (active + paused))
            {
                while (i < (active + paused))
                {
                    GUIListItem item = new GUIListItem();
                    item.Label = names[i];
                    item.Label2 = percent[i];
                    //item.Label3 = eta[i];
                    //string temp = string.Format("{0}({1}) - {2}",peers[i],seeds[i],leechers[i]);
                    //item.Label3 = temp;
                    torrentList.Add(item);
                    i++;
                }
            }
            if (i > (active + paused))
            {
                while (i > (active + paused))
                {
                    torrentList.Clear();
                    i--;
                }
            }
        }

        public class TransmissionClient
        {
            public Uri Url { get; private set; }
            public WebClient WebClient { get; set; }
            public string SessionId { get; private set; }

            public TransmissionClient(Uri url) : this(url, null) { }

            public TransmissionClient(Uri url, WebClient wc)
            {
                if (url == null) throw new ArgumentNullException("url");
                Url = url;
                WebClient = wc;
            }

            public object Invoke(string method, object args)
            {
                return Invoke(method, args, null);
            }

            public virtual object Invoke(string method, object args, JsonNumber? tag)
            {
                if (method == null) throw new ArgumentNullException("method");

                var sessionId = SessionId;
                var wc = WebClient ?? new WebClient();
                var url = Url;

                // 2.1.  Requests
                //  
                //     Requests support three keys:
                //  
                //     (1) A required "method" string telling the name of the method to invoke
                //     (2) An optional "arguments" object of key/value pairs
                //     (3) An optional "tag" number used by clients to track responses.
                //         If provided by a request, the response MUST include the same tag.

                var request = new
                {
                    method,
                    arguments = args,
                    tag,
                };

                while (true)
                {
                    try
                    {
                        if (LocalSettings.Authentication == true)
                        {
                            wc.Credentials = new NetworkCredential(LocalSettings.Username, LocalSettings.Password);
                        }
                        if (!string.IsNullOrEmpty(sessionId))
                            wc.Headers.Add("X-Transmission-Session-Id", sessionId);
                        var requestJson = JsonConvert.ExportToString(request);
                        var responseJson = wc.UploadString(url, requestJson);

                        // 2.2.  Responses
                        //  
                        //     Reponses support three keys:
                        //  
                        //     (1) A required "result" string whose value MUST be "success" on success,
                        //         or an error string on failure.
                        //     (2) An optional "arguments" object of key/value pairs
                        //     (3) An optional "tag" number as described in 2.1.

                        var responseObject = JsonConvert.Import<JsonObject>(responseJson);

                        var result = (responseObject["result"] ?? string.Empty).ToString();
                        if ("error".Equals(result, StringComparison.OrdinalIgnoreCase))
                            throw new Exception("Method failed.");
                        if (!"success".Equals(result, StringComparison.OrdinalIgnoreCase))
                            throw new Exception("Unexpected response result.");

                        if (tag != null && tag.Value.LogicallyEquals(responseObject["tag"]))
                            throw new Exception("Missing or unexpected tag in response.");

                        return responseObject["arguments"];
                    }
                    catch (WebException e)
                    {
                        // 2.3.1.  CSRF Protection
                        //
                        //     Most Transmission RPC servers require a X-Transmission-Session-Id
                        //     header to be sent with requests, to prevent CSRF attacks.
                        //  
                        //     When your request has the wrong id -- such as when you send your first
                        //     request, or when the server expires the CSRF token -- the
                        //     Transmission RPC server will return an HTTP 409 error with the
                        //     right X-Transmission-Session-Id in its own headers.
                        //
                        //     So, the correct way to handle a 409 response is to update your
                        //     X-Transmission-Session-Id and to resend the previous request.

                        HttpWebResponse response;
                        if (e.Status == WebExceptionStatus.ProtocolError
                            && (response = e.Response as HttpWebResponse) != null
                            && response.StatusCode == /* 409 */ HttpStatusCode.Conflict)
                        {
                            sessionId = response.GetResponseHeader("X-Transmission-Session-Id");
                            if (!string.IsNullOrEmpty(sessionId))
                            {
                                SessionId = sessionId;
                                continue;
                            }
                        }

                        throw;
                    }
                }
            }
        }
    }
}