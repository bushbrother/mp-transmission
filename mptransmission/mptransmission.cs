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

namespace mptransmission
{
    [PluginIcons("mptransmission.Icon.png", "mptransmission.Icon_Disabled.png")]
    public class mptransmission : GUIWindow, ISetupForm
    {
        [SkinControlAttribute(2)]
        protected GUIButtonControl buttonOne = null;
        [SkinControlAttribute(3)]
        protected GUIButtonControl buttonTwo = null;

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
            return Load(GUIGraphicsContext.Skin + @"\mptransmission.xml");
        }

        protected override void OnClicked(int controlId, GUIControl control,
          MediaPortal.GUI.Library.Action.ActionType actionType)
        {
            if (control == buttonOne)
                OnButtonOne();
            if (control == buttonTwo)
                OnButtonTwo();
            base.OnClicked(controlId, control, actionType);
        }

        private void OnButtonOne()
        {
            GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow(
              (int)GUIWindow.Window.WINDOW_DIALOG_OK);
            dlg.SetHeading("Button has been pressed");
            dlg.SetLine(1, "You pressed button 1");
            dlg.SetLine(2, String.Empty);
            dlg.SetLine(3, String.Empty);
            dlg.DoModal(GUIWindowManager.ActiveWindow);
        }

        private void OnButtonTwo()
        {
            GUIDialogOK dlg = (GUIDialogOK)GUIWindowManager.GetWindow(
              (int)GUIWindow.Window.WINDOW_DIALOG_OK);
            dlg.SetHeading("Button has been pressed");
            dlg.SetLine(1, "You pressed button 2");
            dlg.SetLine(2, String.Empty);
            dlg.SetLine(3, String.Empty);
            dlg.DoModal(GUIWindowManager.ActiveWindow);
        }

        #endregion

    }
}