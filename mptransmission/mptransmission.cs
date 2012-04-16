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

/* This is the main class for the mp-transmission plugin and includes the 
   configuration data for the plugin to use in MediaPortal configuration,
   as well as the main methods that drive the plugin. */

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
    // Here we define the two images to use within MediaPortal to show if the plugin
    // is enabled or disabled.
    [PluginIcons("mptransmission.Icon.png", "mptransmission.Icon_Disabled.png")]
    public class mptransmission : GUIWindow, ISetupForm
    {
        // Defining the list item that is used to hold all torrents.
        [SkinControlAttribute(5)] public GUIListControl torrentList = null;

        // Initialising the timer that will be used accross the plugin to refresh data.
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
        public const string Version = "0.20";

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

        #region Methods

        // This method is invoked when the plugin is loaded, it loads the settings from the main MediaPortal
        // XML which holds all the data from the plugin setup. It then creates the timer, sets some variables
        // to "0" and then loads the main skin file.
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

        // This method deals with the initial XML skin file load, it checks to see if the file is being loaded
        // from another window (stats or details XML) or if it is the first to be loaded. If it is being loaded
        // from another mp-transmission XML then it will restore the saved list. It then starts the activeTorrents
        // method.
        protected override void OnPageLoad()
        {
            if (variables.needsRestore)
            {
                restoreList();
            }
            variables.needsRestore = false;
            activeTorrents();
        }

        // This method sets the timer interval dependant on the plugin configuration. It then starts the timer.
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

        // This is the method that is called at each timer interval, it simply calls the main Connect method below.
        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Connect();
        }

        // This is the main method that gets all data from the RPC server and updates a set of arrays within variables.cs.
        public void Connect()
        {
                // Set the url to the stored RPC server location and create the correct string format.
                var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
                var client = new TransmissionClient(url);
                // The below commands set the single variables.
                JsonObject session = (JsonObject)client.Invoke("session-stats", null);
                JsonNumber num = (JsonNumber)session["activeTorrentCount"];
                variables.activeTorrents = (int)num;
                JsonNumber numPaused = (JsonNumber)session["pausedTorrentCount"];
                variables.pausedTorrents = (int)numPaused;
                JsonNumber download = (JsonNumber)session["downloadSpeed"];
                variables.totalDownload = (int)download;
                JsonNumber upload = (JsonNumber)session["uploadSpeed"];
                variables.totalUpload = (int)upload;
                JsonObject cumulative_stats = (JsonObject)session["cumulative-stats"];
                JsonObject current_stats = (JsonObject)session["current-stats"];
                JsonNumber statsUploaded = (JsonNumber)cumulative_stats["uploadedBytes"];
                JsonNumber statsDownloaded = (JsonNumber)cumulative_stats["downloadedBytes"];
                JsonNumber statsFiles = (JsonNumber)cumulative_stats["filesAdded"];
                JsonNumber statsSessions = (JsonNumber)cumulative_stats["sessionCount"];
                JsonNumber statsSeconds = (JsonNumber)cumulative_stats["secondsActive"];
                JsonNumber cur_statsUploaded = (JsonNumber)current_stats["uploadedBytes"];
                JsonNumber cur_statsDownloaded = (JsonNumber)current_stats["downloadedBytes"];
                JsonNumber cur_statsFiles = (JsonNumber)current_stats["filesAdded"];
                JsonNumber cur_statsSessions = (JsonNumber)current_stats["sessionCount"];
                JsonNumber cur_statsSeconds = (JsonNumber)current_stats["secondsActive"];
                variables.statsUploaded = (long)statsUploaded;
                variables.statsDownloaded = (long)statsDownloaded;
                variables.statsFiles = (long)statsFiles;
                variables.statsSessions = (long)statsSessions;
                variables.statsSeconds = (long)statsSeconds;
                variables.cur_statsUploaded = (long)cur_statsUploaded;
                variables.cur_statsDownloaded = (long)cur_statsDownloaded;
                variables.cur_statsFiles = (long)cur_statsFiles;
                variables.cur_statsSessions = (long)cur_statsSessions;
                variables.cur_statsSeconds = (long)cur_statsSeconds;
                // Some objects are groups of "fields" that need to be parsed.
                var torrent = (IDictionary)client.Invoke("torrent-get", new { fields = new[] { "name", "percentDone", "sizeWhenDone", "peersConnected", "peersGettingFromUs", "peersSendingToUs", "eta", "rateDownload", "rateUpload", "id"} }, null);
                var i = 0;
                // Loop to assign all the values for each torrent in the list.
                foreach (IDictionary torrents in (IList)torrent["torrents"])
                {
                    if (i < (variables.activeTorrents + variables.pausedTorrents))
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

            //Main Window Labels
            GUIPropertyManager.SetProperty("#numDownloads", variables.activeTorrents.ToString("0"));
            GUIPropertyManager.SetProperty("#numPausedDownloads", variables.pausedTorrents.ToString("0"));
            GUIPropertyManager.SetProperty("#uploadSpeedTotal", UnitConvert.TransferSpeedToString(variables.totalUpload));
            GUIPropertyManager.SetProperty("#downloadSpeedTotal", UnitConvert.TransferSpeedToString(variables.totalDownload));

            //Details Window Labels
            GUIPropertyManager.SetProperty("#mptransmission.Details.Name", variables.torrentName[variables.selTorrent]);
            GUIPropertyManager.SetProperty("#mptransmission.Details.DownloadSpeed", variables.torrentDown[variables.selTorrent]);
            GUIPropertyManager.SetProperty("#mptransmission.Details.UploadSpeed", variables.torrentUp[variables.selTorrent]);
            GUIPropertyManager.SetProperty("#mptransmission.Details.Size", variables.torrentSize[variables.selTorrent]);
            GUIPropertyManager.SetProperty("#mptransmission.Details.Peers", variables.torrentPeers[variables.selTorrent]);
            GUIPropertyManager.SetProperty("#mptransmission.Details.Seeds", variables.torrentSeeds[variables.selTorrent]);
            GUIPropertyManager.SetProperty("#mptransmission.Details.Progress", variables.torrentProgress[variables.selTorrent]);

            //Cumulative Stats Window Labels
            GUIPropertyManager.SetProperty("#mptransmission.Stats.TotalUploads", UnitConvert.SizeToString(variables.statsUploaded));
            GUIPropertyManager.SetProperty("#mptransmission.Stats.TotalDownloads", UnitConvert.SizeToString(variables.statsDownloaded));
            GUIPropertyManager.SetProperty("#mptransmission.Stats.TotalFiles", variables.statsFiles.ToString());
            GUIPropertyManager.SetProperty("#mptransmission.Stats.TotalSession", variables.statsSessions.ToString());
            GUIPropertyManager.SetProperty("#mptransmission.Stats.TotalSeconds", UnitConvert.TimeRemainingToString(variables.statsSeconds));

            //Current Stats Window Labels
            GUIPropertyManager.SetProperty("#mptransmission.Stats.cur_TotalUploads", UnitConvert.SizeToString(variables.cur_statsUploaded));
            GUIPropertyManager.SetProperty("#mptransmission.Stats.cur_TotalDownloads", UnitConvert.SizeToString(variables.cur_statsDownloaded));
            GUIPropertyManager.SetProperty("#mptransmission.Stats.cur_TotalFiles", variables.cur_statsFiles.ToString());
            GUIPropertyManager.SetProperty("#mptransmission.Stats.cur_TotalSession", variables.cur_statsSessions.ToString());
            GUIPropertyManager.SetProperty("#mptransmission.Stats.cur_TotalSeconds", UnitConvert.TimeRemainingToString(variables.cur_statsSeconds));

            // Checking for a completed torrent to display a readable message.
            if (variables.torrentETA[variables.selTorrent] == "-1s")
            {
                GUIPropertyManager.SetProperty("#mptransmission.Details.ETA", "Completed - Seeding");
            }
            // Checking for a torrent that has not started yet to display a readable message.
            else if (variables.torrentETA[variables.selTorrent] == "-2s")
            {
                GUIPropertyManager.SetProperty("#mptransmission.Details.ETA", "No Data - Awaiting Download");
            }
            // Otherwise put the actual ETA calculated.
            else
            {
                GUIPropertyManager.SetProperty("#mptransmission.Details.ETA", variables.torrentETA[variables.selTorrent]);
            }

            // If there are less items in the list than the current sum of torrents, we need to re-populate
            // the list to get the new torrent data.
            if (torrentList.ListItems.Count < (variables.activeTorrents + variables.pausedTorrents))
            {
                rePopulateList();
            }
            // If there are more torrents in the list than the sum of torrents then a torrent has been removed.
            // We re-populate the list.
            if (torrentList.ListItems.Count > (variables.activeTorrents + variables.pausedTorrents))
            {
                rePopulateList();
                PopulateList();
            }
            // Otherwise we should just update the existing data for the list.
            else
            {
                updateList();
            }
        }

        // This method saves the list data, including the number of items in the list and associated labels.
        public void saveList()
        {
            int i = 0;
            while (i < torrentList.ListItems.Count)
            {
                variables.listSize = torrentList.ListItems.Count;
                variables.itemLabel[i] = torrentList[i].Label;
                variables.itemLabel2[i] = torrentList[i].Label2;
                variables.itemLabel3[i] = torrentList[i].Label3;
                i++;
            }
        }

        // This method restores the list from saved data.
        public void restoreList()
        {
            int i = 0;
            while (i < variables.listSize)
            {
                GUIListItem item = new GUIListItem();
                item.Label = variables.itemLabel[i];
                item.Label2 = variables.itemLabel2[i];
                item.Label3 = variables.itemLabel3[i];
                torrentList.Add(item);
                i++;
            }
        }

        // This method is called from the context menu and takes an integer that represents the torrent selected to be paused.
        public static void pauseTorrent(int pauseID)
        {
            var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
            var client = new TransmissionClient(url);
            client.Invoke("torrent-stop", new { ids = new[] { pauseID } }, null);
        }

        // This method is called from the context menu and pauses all torrents as it does not define a single torrent.
        public static void pauseTorrent()
        {
            var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
            var client = new TransmissionClient(url);
            client.Invoke("torrent-stop", null, null);
        }

        // This method is called from the context menu and takes an integer that represents the torrent selected to be started.
        public static void startTorrent(int startID)
        {
            var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
            var client = new TransmissionClient(url);
            client.Invoke("torrent-start", new { ids = new[] { startID } }, null);
        }

        // This method is called from the context menu and starts all torrents as it does not define a single torrent.
        public static void startTorrent()
        {
            var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
            var client = new TransmissionClient(url);
            client.Invoke("torrent-start", null, null);
        }

        // This method is called from the context menu and takes an integer that represents the torrent selected to be removed.
        // The downloaded data is preserved using this method.
        public static void removeTorrent(int removeID)
        {
            var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
            var client = new TransmissionClient(url);
            client.Invoke("torrent-remove", new { ids = new[] { removeID } }, null);
        }

        // This method is called from the context menu and takes an integer that represents the torrent selected to be removed.
        // The downloaded data is deleted permanently using this method.
        public static void removeTorrentandFiles(int removeID)
        {
            var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
            var client = new TransmissionClient(url);
            client.Invoke("torrent-remove", new JsonObject { { "ids", new[] {removeID} }, { "delete-local-data", true } }, null);
        }

        // This method checks to see if there are any torrents to be listed, if there are then it runs through the stored
        // arrays of data and uses them to build the list.
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

        // This method does not add any items, it simply updates the labes with any changed data in the arrays.
        private void updateList()    
        {
            int i = 0;
            if (torrentList.ListItems.Count == 0)
            {
                torrentList.Clear();
                GUIListItem item = new GUIListItem();
                item.Label = "No Torrents :'(";
                torrentList.Add(item);
            }
            else
            {
                while (i < (variables.activeTorrents + variables.pausedTorrents))
                {
                    torrentList.ListItems[i].Label = variables.torrentName[i];
                    torrentList.ListItems[i].Label2 = variables.torrentProgress[i];
                    string temp = string.Format("S-{0}({1}) ~ L-{2}", variables.torrentPeersConnected[i], variables.torrentSeeds[i], variables.torrentPeers[i]);
                    torrentList.ListItems[i].Label3 = temp;
                    i++;
                }
            }
        }

        // This method is invoked when the window is changed within MediaPortal and a new XML skin file is loaded.
        // At this point the timer is stopped.
        protected override void OnPageDestroy(int newWindowId)
        {
            base.OnPageDestroy(newWindowId);
            aTimer.Stop();
        }

        // This method removes any torrents in the list that are no longer active or paused (they have been removed)
        // and it rebuilds the list with the remaining data in the arrays.
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

        // This method is invoked when an item in the list is clicked, it will perform no action if there
        // are no torrents, otherwise it will save the current list, set the needsRestore boolean to true
        // (for list recovery later) and switch the skin file to the torrent detail page.
        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            variables.selTorrent = torrentList.SelectedListItemIndex;
            if (torrentList.SelectedListItem.Label == "No Torrents :'(")
            {
            }
            else
            {
                variables.needsRestore = true;
                saveList();
                Connect();
                GUIWindowManager.ActivateWindow(56348);
            }
        }

        // This method is invoked when the context menu key is pressed within MediaPortal.
        protected override void OnShowContextMenu()
        {
            // Check what the selected torrent is.
            variables.selTorrent = torrentList.SelectedListItemIndex;
            // Create a dialog menu item.
            GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_MENU);
            if (dlg == null)
            {
                return;
            }
            // Here are the options presented to the user for that torrent.
            dlg.Reset();
            dlg.SetHeading("Torrent Options");
            dlg.Add("Start");
            dlg.Add("Stop");
            dlg.Add("Remove");
            dlg.Add("Details");
            dlg.Add("Start All");
            dlg.Add("Stop All");
            dlg.Add("Stats");
            dlg.DoModal(GUIWindowManager.ActiveWindow);

            // The below set of switches call the various methods described above depending on the option chosen.
            switch (dlg.SelectedLabelText)
            {
                case "Start":
                    {
                        int selected = variables.torrentID[variables.selTorrent];
                        startTorrent(selected);
                        break;
                    };
                case "Start All":
                    {
                        startTorrent();
                        break;
                    };
                case "Stop":
                    {
                        int selected = variables.torrentID[variables.selTorrent];
                        pauseTorrent(selected);
                        break;
                    };
                case "Stop All":
                    {
                        pauseTorrent();
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
                            variables.needsRestore = true;
                            saveList();
                            Connect();
                            GUIWindowManager.ActivateWindow(56348);
                        }
                        break;
                    };
                case "Stats":
                    {
                        variables.needsRestore = true;
                        saveList();
                        Connect();
                        GUIWindowManager.ActivateWindow(56349);
                        break;
                    };
                case "Remove":
                    {
                        // In this case we need to ask the user which type of remove they want to perform, the default
                        // option is to highlight "No" to preserve data.
                        int selected = variables.torrentID[variables.selTorrent];
                        GUIDialogYesNo ask = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
                        ask.Reset();
                        ask.SetHeading("Remove downloaded files?");
                        ask.SetLine(1, "Files will be removed permanently.");
                        ask.SetLine(2, "This cannot be undone!");
                        ask.SetDefaultToYes(false);
                        ask.DoModal(GUIWindowManager.ActiveWindow);

                        if (ask.IsConfirmed)
                        {
                            removeTorrentandFiles(selected);
                        }
                        else
                        {
                            removeTorrent(selected);
                        }
                        break;
                    };
            }
        }

        #endregion

        #region TransmissionClient

        // This class is the transmission client that connects to the RPC server and passes the methods that
        // have been requested. This was written by Atif Aziz and can be found here: https://groups.google.com/forum/?fromgroups#!topic/jayrock/x04j8lfmAbE
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

        #endregion

    }
}