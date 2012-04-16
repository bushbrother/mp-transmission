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
using MediaPortal.Dialogs;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using mptransmission.Settings;
using System.Timers;

namespace mptransmission
{
    public class torrentStats : GUIWindow
    {
        // Define the window ID for MediaPortal
        public override int GetID
        {
            get
            {
                return 56349;
            }

            set
            {
            }
        }

        // Load the correct skin XML file.
        public override bool Init()
        {
            return Load(GUIGraphicsContext.Skin + @"\mptransmission.Stats.xml");
        }

        // Start the timer again when the page is loaded.
        protected override void OnPageLoad()
        {
            mptransmission.aTimer.Start();
        }

        // Stop the timer when page is changed.
        protected override void OnPageDestroy(int new_windowId)
        {
            base.OnPageDestroy(new_windowId);
            mptransmission.aTimer.Stop();
        }
    }
}
