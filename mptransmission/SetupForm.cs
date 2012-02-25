
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
using System.Collections;
using System.Windows.Forms;
using System.Net;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using mptransmission.Settings;

namespace mptransmission
{
    public partial class SetupForm : Form
    {
        public SetupForm()
        {
            InitializeComponent();
            this.comboBox1.Items.AddRange(new object[] { "5 Seconds", "10 Seconds", "15 Seconds", "20 Seconds", "25 Seconds", "30 Seconds" });
            LocalSettings.Load();
            textBoxHostname.Text = LocalSettings.Hostname;
            textBoxPort.Text = LocalSettings.Port;
            checkBoxAuth.Checked = LocalSettings.Authentication;
            textBoxUsername.Text = LocalSettings.Username;
            textBoxPassword.Text = LocalSettings.Password;
            comboBox1.Text = LocalSettings.refreshRate;
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
            LocalSettings.refreshRate = comboBox1.Text;
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
            LocalSettings.refreshRate = comboBox1.Text;
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
            makeWhite();
            Connect();
        }

        public void makeWhite()
        {
            downloadText.Text = "";
            downloadText.BackColor = System.Drawing.Color.White;
        }

        public void Connect()
        {
            string tempDownloadDir = "empty";
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
                    tempDownloadDir = (string)session["download-dir"];
                    if (tempDownloadDir != "empty")
                    {
                        downloadText.Text = "Success!";
                        downloadText.BackColor = System.Drawing.Color.PaleGreen;
                    }
                    tempDownloadDir = "empty";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        private void downloadText_TextChanged(object sender, EventArgs e)
        {
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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
