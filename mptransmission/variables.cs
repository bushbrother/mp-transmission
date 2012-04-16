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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;

namespace mptransmission
{
    // This class defines all the variables to be shared accross the plugin.
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
