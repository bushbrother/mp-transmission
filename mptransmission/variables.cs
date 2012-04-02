using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;

namespace mptransmission
{
   public static class variables
    {
        public static int selTorrent = 0;
        public static int listSize = 0;
        public static int activeTorrents = 0;
        public static int pausedTorrents = 0;
        public static string[] torrentName = new string[100];
        public static string[] torrentETA = new string[100];
        public static string[] torrentDown = new string[100];
        public static string[] torrentUp = new string[100];
        public static string[] torrentSize = new string[100];
        public static string[] torrentPeers = new string[100];
        public static string[] torrentPeersConnected = new string[100];
        public static string[] torrentSeeds = new string[100];
        public static string[] torrentProgress = new string[100];
        public static int[] torrentID = new int[100];
        public static int totalUpload = 0;
        public static int totalDownload = 0;
        public static bool needsRestore = false;
        public static string[] itemLabel = new string[100];
        public static string[] itemLabel2 = new string[100];
        public static string[] itemLabel3 = new string[100];
        public static long cumulative_stats = 0;
        public static long current_stats = 0;
        public static long statsUploaded = 0;
        public static long statsDownloaded = 0;
        public static long statsFiles = 0;
        public static long statsSessions = 0;
        public static long statsSeconds = 0;
        public static long cur_statsUploaded = 0;
        public static long cur_statsDownloaded = 0;
        public static long cur_statsFiles = 0;
        public static long cur_statsSessions = 0;
        public static long cur_statsSeconds = 0;
    }
}
