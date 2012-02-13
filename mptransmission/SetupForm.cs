
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using Jayrock;
using mptransmission.Settings;

namespace mptransmission
{
    public partial class SetupForm : Form
    {
        public SetupForm()
        {
            InitializeComponent();
            LocalSettings.Load();
            textBoxHostname.Text = LocalSettings.Hostname;
            textBoxPort.Text = LocalSettings.Port;
            checkBoxAuth.Checked = LocalSettings.Authentication;
            textBoxUsername.Text = LocalSettings.Username;
            textBoxPassword.Text = LocalSettings.Password;
            downloadText.Text = "Waiting ...";
        }

        private void SetupForm_Load(object sender, EventArgs e)
        {
        }

        private void checkBoxAuth_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAuth.Checked){
                this.textBoxUsername.Enabled = true;
                this.textBoxPassword.Enabled = true;
            }
            else {
                this.textBoxUsername.Enabled = false;
                this.textBoxPassword.Enabled = false;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            LocalSettings.Hostname = textBoxHostname.Text;
            LocalSettings.Port = textBoxPort.Text;
            if (checkBoxAuth.Checked){
                LocalSettings.Authentication = true;
                LocalSettings.Username = textBoxUsername.Text;
                LocalSettings.Password = textBoxPassword.Text;
            }
            else {
                LocalSettings.Authentication = false;
            }
            LocalSettings.Save();
            this.Close();
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            LocalSettings.Hostname = textBoxHostname.Text;
            LocalSettings.Port = textBoxPort.Text;
            if (checkBoxAuth.Checked)
            {
                LocalSettings.Authentication = true;
                LocalSettings.Username = textBoxUsername.Text;
                LocalSettings.Password = textBoxPassword.Text;
            }
            else
            {
                LocalSettings.Authentication = false;
            }
            LocalSettings.Save();
            Connect();
        }

        public void Connect()
        {
                if (LocalSettings.Hostname.Equals(""))
                {
                    MessageBox.Show("No Hostname Set!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!Uri.IsWellFormedUriString("http://" + LocalSettings.Hostname + ":"+LocalSettings.Port + "/transmission/rpc", UriKind.Absolute))
                {
                    MessageBox.Show("Invalid Hostname Location!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                try
                {
                    var url = new Uri("http://" + LocalSettings.Hostname + ":" + LocalSettings.Port + "/transmission/rpc");
                    var client = new TransmissionClient(url);
                    JsonObject session = (JsonObject)client.Invoke("session-get", null);
                    downloadText.Text = (string)session["download-dir"];
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        private void downloadText_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
