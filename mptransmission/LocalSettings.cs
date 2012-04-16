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
using System.IO;
using System.Text;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using Microsoft.Win32;
using System.Windows.Forms;
using MediaPortal.Configuration;

namespace mptransmission.Settings
{
    public class LocalSettings
    {

        #region SettingValues

        /// <summary>
        /// The transmission username
        /// </summary>
        public static string Username
        {
            get { return username; }
            set { username = value; }
        }
        private static string username = String.Empty;

        /// <summary>
        /// The transmission hostname
        /// </summary>
        public static string Hostname
        {
            get { return hostname; }
            set { hostname = value; }
        }
        private static string hostname = String.Empty;

        /// <summary>
        /// The transmission port
        /// </summary>
        public static string Port
        {
            get { return port; }
            set { port = value; }
        }
        private static string port = String.Empty;

        /// <summary>
        /// The transmission password
        /// </summary>
        public static string Password
        {
            get { return password; }
            set { password = value; }
        }
        private static string password = String.Empty;

        /// <summary>
        /// The transmission authentication setting
        /// </summary>
        public static bool Authentication
        {
            get { return authentication; }
            set { authentication = value; }
        }
        private static bool authentication;

        /// <summary>
        /// Temporary check for boolian conversion
        /// </summary>
        public static string AuthenticationCheck
        {
            get { return authenticationCheck; }
            set { authenticationCheck = value; }
        }
        private static string authenticationCheck = String.Empty;

        /// <summary>
        /// The transmission username
        /// </summary>
        public static string refreshRate
        {
            get { return refresh; }
            set { refresh = value; }
        }
        private static string refresh = String.Empty;

        #endregion

        #region LoadSettings

        /// <summary>
        /// Load the settings from the mediaportal config
        /// </summary>
        public static void Load()
        {
            using (MediaPortal.Profile.Settings reader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                Username = reader.GetValue(mptransmission.ConfigSection, "username");
                Hostname = reader.GetValue(mptransmission.ConfigSection, "hostname");
                Port = reader.GetValue(mptransmission.ConfigSection, "port");
                AuthenticationCheck = reader.GetValue(mptransmission.ConfigSection, "authentication");
                refreshRate = reader.GetValue(mptransmission.ConfigSection, "refreshRate");

                // Convert String to Bool
                if (AuthenticationCheck == "False")
                {
                    authentication = false;
                }
                if (AuthenticationCheck == "True")
                {
                    authentication = true;
                }

                string encryptedPassword = reader.GetValue(mptransmission.ConfigSection, "password");
                Password = decryptString(encryptedPassword);
            }
        }

        #endregion

        #region SaveSettings

        /// <summary>
        /// Save the settings to the MP config
        /// </summary>
        public static void Save()
        {
            using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
            {
                // Encrypt the password
                string encryptedPassword = encryptString(Password);
                xmlwriter.SetValue(mptransmission.ConfigSection, "username", Username);
                xmlwriter.SetValue(mptransmission.ConfigSection, "hostname", Hostname);
                xmlwriter.SetValue(mptransmission.ConfigSection, "password", encryptedPassword);
                xmlwriter.SetValue(mptransmission.ConfigSection, "port", Port);
                xmlwriter.SetValue(mptransmission.ConfigSection, "authentication", Authentication);
                xmlwriter.SetValue(mptransmission.ConfigSection, "refreshRate", refreshRate);
            }
        }

        #endregion

        #region Decrypt

        /// <summary>
        /// Decrypt an encrypted setting string
        /// </summary>
        /// <param name="encrypted">The string to decrypt</param>
        /// <returns>The decrypted string or an empty string if something went wrong</returns>
        private static string decryptString(string encrypted)
        {
            string decrypted = String.Empty;

            EncryptDecrypt Crypto = new EncryptDecrypt();
            try
            {
                decrypted = Crypto.Decrypt(encrypted);
            }
            catch (Exception)
            {
                MediaPortal.GUI.Library.Log.Error("Could not decrypt config string!");
            }

            return decrypted;
        }

        #endregion

        #region Encrypt

        /// <summary>
        /// Encrypt a setting string
        /// </summary>
        /// <param name="decrypted">An unencrypted string</param>
        /// <returns>The string encrypted</returns>
        private static string encryptString(string decrypted)
        {
            EncryptDecrypt Crypto = new EncryptDecrypt();
            string encrypted = String.Empty;

            try
            {
                encrypted = Crypto.Encrypt(decrypted);
            }
            catch (Exception)
            {
                MediaPortal.GUI.Library.Log.Error("Could not encrypt setting string!");
                encrypted = String.Empty;
            }

            return encrypted;
        }

        #endregion

    }
   
}