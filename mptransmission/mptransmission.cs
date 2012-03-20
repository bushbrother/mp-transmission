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

        internal static System.Timers.Timer aTimer = new System.Timers.Timer();

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
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
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


        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Connect();
        }

        #endregion

        public void Connect()
        {
                var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
                var client = new TransmissionClient(url);
                JsonObject session = (JsonObject)client.Invoke("session-stats", null);
                JsonNumber num = (JsonNumber)session["activeTorrentCount"];
                variables.activeTorrents = (int)num;
                JsonNumber numPaused = (JsonNumber)session["pausedTorrentCount"];
                variables.pausedTorrents = (int)numPaused;
                JsonNumber download = (JsonNumber)session["downloadSpeed"];
                variables.totalDownload = (int)download;
                JsonNumber upload = (JsonNumber)session["uploadSpeed"];
                variables.totalUpload = (int)upload;
                var torrent = (IDictionary)client.Invoke("torrent-get", new { fields = new[] { "name", "percentDone", "sizeWhenDone", "peersConnected", "peersGettingFromUs", "peersSendingToUs", "eta", "rateDownload", "rateUpload", "id"} }, null);
                var i = 0;
                foreach (IDictionary torrents in (IList)torrent["torrents"])
                {
                    if (i < variables.activeTorrents)
                    {
                        variables.torrentName[i] = (string)torrents["name"];
                        JsonNumber percent = (JsonNumber)torrents["percentDone"];
                        double tempPercent = (double)percent;
                        variables.torrentProgress[i] = tempPercent.ToString("0.00%");
                        JsonNumber size = (JsonNumber)torrents["sizeWhenDone"];
                        long tempSize = (long)size;
                        variables.torrentSize[i] = UnitConvert.SizeToString(tempSize);
                        JsonNumber peers = (JsonNumber)torrents["peersConnected"];
                        double tempPeers = (double)peers;
                        variables.torrentPeersConnected[i] = tempPeers.ToString();
                        JsonNumber leechers = (JsonNumber)torrents["peersGettingFromUs"];
                        double tempLeechers = (double)leechers;
                        variables.torrentPeers[i] = tempLeechers.ToString();
                        JsonNumber seeds = (JsonNumber)torrents["peersSendingToUs"];
                        double tempSeeds = (double)seeds;
                        variables.torrentSeeds[i] = tempSeeds.ToString();
                        JsonNumber eta = (JsonNumber)torrents["eta"];
                        long tempEta = (long)eta;
                        variables.torrentETA[i] = UnitConvert.TimeRemainingToString(tempEta);
                        JsonNumber down = (JsonNumber)torrents["rateDownload"];
                        long tempDown = (long)down;
                        variables.torrentDown[i] = UnitConvert.TransferSpeedToString(tempDown);
                        JsonNumber up = (JsonNumber)torrents["rateUpload"];
                        long tempUp = (long)up;
                        variables.torrentUp[i] = UnitConvert.TransferSpeedToString(tempUp);
                        JsonNumber torrentID = (JsonNumber)torrents["id"];
                        variables.torrentID[i] = (int)torrentID;
                        i++;
                    }
                }
            GUIPropertyManager.SetProperty("#numDownloads", variables.activeTorrents.ToString("0"));
            GUIPropertyManager.SetProperty("#numPausedDownloads", variables.pausedTorrents.ToString("0"));
            GUIPropertyManager.SetProperty("#uploadSpeedTotal", UnitConvert.TransferSpeedToString(variables.totalUpload));
            GUIPropertyManager.SetProperty("#downloadSpeedTotal", UnitConvert.TransferSpeedToString(variables.totalDownload));
            GUIPropertyManager.SetProperty("#mptransmission.Details.Name", variables.torrentName[variables.selTorrent]);
            GUIPropertyManager.SetProperty("#mptransmission.Details.DownloadSpeed", variables.torrentDown[variables.selTorrent]);
            GUIPropertyManager.SetProperty("#mptransmission.Details.UploadSpeed", variables.torrentUp[variables.selTorrent]);
            GUIPropertyManager.SetProperty("#mptransmission.Details.Size", variables.torrentSize[variables.selTorrent]);
            GUIPropertyManager.SetProperty("#mptransmission.Details.Peers", variables.torrentPeers[variables.selTorrent]);
            GUIPropertyManager.SetProperty("#mptransmission.Details.Seeds", variables.torrentSeeds[variables.selTorrent]);
            GUIPropertyManager.SetProperty("#mptransmission.Details.Progress", variables.torrentProgress[variables.selTorrent]);

            if (variables.torrentETA[variables.selTorrent] == "-1s")
            {
                GUIPropertyManager.SetProperty("#mptransmission.Details.ETA", "Completed - Seeding");
            }
            else
            {
                GUIPropertyManager.SetProperty("#mptransmission.Details.ETA", variables.torrentETA[variables.selTorrent]);
            }

            //variables.listSize = torrentList.ListItems.Count;

            if (torrentList.ListItems.Count == 0)
            {
                PopulateList();
            }
            if (torrentList.ListItems.Count < (variables.activeTorrents + variables.pausedTorrents))
            {
                rePopulateList();
            }
            if (torrentList.ListItems.Count > (variables.activeTorrents + variables.pausedTorrents))
            {
                rePopulateList();
                PopulateList();
            }
            else
            {
                updateList();
            }
        }

        public static void pauseTorrent(int pauseID)
        {
            var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
            var client = new TransmissionClient(url);
            int[] fields = new[] { pauseID };
            client.Invoke("torrent-stop", fields, null);
        }

        public static void startTorrent(int startID)
        {
            var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
            var client = new TransmissionClient(url);
            client.Invoke("torrent-start", new { fields = new[] { startID } }, null);
        }

        public static void removeTorrent(int removeID)
        {
            var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
            var client = new TransmissionClient(url);
            client.Invoke("torrent-remove", new { fields = new[] { removeID } }, null);
        }

        public static void removeTorrentandFiles(int removeID)
        {
            var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
            var client = new TransmissionClient(url);
            client.Invoke("torrent-remove", removeID, null);
        }
        
        private void PopulateList()    
        {
        torrentList.Clear();
        int i = 0;
        if (torrentList.ListItems.Count == 0)
            {
                GUIListItem item = new GUIListItem();
                item.Label = "No Torrents :'(";
                torrentList.Add(item);
            }
            else
            {
                while (i < (variables.activeTorrents + variables.pausedTorrents))
                {
                    GUIListItem item = new GUIListItem();
                    item.Label = variables.torrentName[i];
                    item.Label2 = variables.torrentProgress[i];
                    string temp = string.Format("S-{0}({1}) ~ L-{2}", variables.torrentPeersConnected[i], variables.torrentSeeds[i], variables.torrentPeers[i]);
                    item.Label3 = temp;
                    torrentList.Add(item);
                    i++;
                }
            }
        }

        private void updateList()    
        {
            int i = 0;
            while (i < (variables.activeTorrents + variables.pausedTorrents))
            {
                torrentList.ListItems[i].Label = variables.torrentName[i];
                torrentList.ListItems[i].Label2 = variables.torrentProgress[i];
                string temp = string.Format("S-{0}({1}) ~ L-{2}", variables.torrentPeersConnected[i], variables.torrentSeeds[i], variables.torrentPeers[i]);
                torrentList.ListItems[i].Label3 = temp;
                i++;
            }
        }

        protected override void OnPageDestroy(int newWindowId)
        {
            base.OnPageDestroy(newWindowId);
            //aTimer.Elapsed -= new ElapsedEventHandler(OnTimedEvent);
            aTimer.Stop();
        }

        private void rePopulateList()
        {
            int i = torrentList.ListItems.Count;
            if (i < (variables.activeTorrents + variables.pausedTorrents))
            {
                while (i < (variables.activeTorrents + variables.pausedTorrents))
                {
                    GUIListItem item = new GUIListItem();
                    item.Label = variables.torrentName[i];
                    item.Label2 = variables.torrentProgress[i];
                    string temp = string.Format("S-{0}({1}) ~ L-{2}", variables.torrentPeersConnected[i], variables.torrentSeeds[i], variables.torrentPeers[i]);
                    item.Label3 = temp;
                    torrentList.Add(item);
                    i++;
                }
            }
            if (i > (variables.activeTorrents + variables.pausedTorrents))
            {
                while (i > (variables.activeTorrents + variables.pausedTorrents))
                {
                    torrentList.Clear();
                    i--;
                }
            }
        }

        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            variables.selTorrent = torrentList.SelectedListItemIndex;
            if (torrentList.SelectedListItem.Label == "No Torrents :'(")
            {
            }
            else
            {
                Connect();
                GUIWindowManager.ActivateWindow(56348);
            }
        }

        protected override void OnShowContextMenu()
        {
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlg == null)
            {
                return;
            }

            dlg.Reset();
            dlg.SetHeading("Torrent Options");
            dlg.Add("Start");
            dlg.Add("Stop");
            dlg.Add("Remove");
            dlg.Add("Details");
            dlg.DoModal(GUIWindowManager.ActiveWindow);

            switch (dlg.SelectedLabelText)
            {
                case "Start":
                    {
                        int selected = variables.torrentID[variables.selTorrent];
                        startTorrent(selected);
                        break;
                    };
                case "Stop":
                    {
                        int selected = variables.torrentID[variables.selTorrent];
                        pauseTorrent(selected);
                        break;
                    };
                case "Details":
                    {
                        variables.selTorrent = torrentList.SelectedListItemIndex;
                        if (torrentList.SelectedListItem.Label == "No Torrents :'(")
                        {
                        }
                        else
                        {
                            Connect();
                            GUIWindowManager.ActivateWindow(56348);
                        }
                        break;
                    };
                case "Remove":
                    {
                        GUIDialogYesNo ask = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                        ask.Reset();
                        ask.SetHeading("Remove downloaded files?");
                        ask.SetLine(1, "Files will be removed permanently.");
                        ask.SetLine(2, "This cannot be undone!");
                        ask.SetDefaultToYes(false);
                        ask.DoModal(GUIWindowManager.ActiveWindow);

                        if (ask.IsConfirmed)
                        {

                        }
                        else
                        {

                        }
                        break;
                    };
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