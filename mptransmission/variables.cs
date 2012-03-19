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
        //public static int listSize = 0;
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
    }
}
