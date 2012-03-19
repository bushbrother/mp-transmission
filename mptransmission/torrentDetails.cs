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
    public class torrentDetails : GUIWindow
    {

        public override int GetID
        {
            get
            {
                return 56348;
            }

            set
            {
            }
        }

        public override bool Init()
        {
            return Load(GUIGraphicsContext.Skin + @"\mptransmission.Details.xml");
        }

        protected override void OnPageLoad()
        {
            mptransmission.aTimer.Start();
        }

        protected override void OnPageDestroy(int new_windowId)
        {
            base.OnPageDestroy(new_windowId);
            mptransmission.aTimer.Stop();
        }
    }
}
