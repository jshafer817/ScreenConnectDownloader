using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace ScreenConnectDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            string host = "https://remote.onsitedentalsystems.com";
            string supportCode = txtSupportCode.Text.Trim();

            if (string.IsNullOrEmpty(supportCode))
            {
                MessageBox.Show("Please enter a valid support code.");
                return;
            }

            CookieContainer cookies = new CookieContainer();
            string html;

            try
            {
                // Step 1: Get anti-forgery token
                var getReq = (HttpWebRequest)WebRequest.Create(host + "/");
                getReq.CookieContainer = cookies;
                getReq.UserAgent = "Mozilla/5.0";

                using (var resp = (HttpWebResponse)getReq.GetResponse())
                using (var reader = new StreamReader(resp.GetResponseStream()))
                    html = reader.ReadToEnd();

                var match = Regex.Match(html, @"""antiForgeryToken"":\s*""([^""]+)""");
                if (!match.Success)
                    throw new Exception("Token not found.");

                string token = match.Groups[1].Value;

                // Step 2: Get GUID for session
                string jsonPayload = "[ { \"GuestSessionInfo\": { \"sessionCodes\": [\"" + supportCode + "\"], \"sessionIDs\": [] } }, 0 ]";
                var postReq = (HttpWebRequest)WebRequest.Create(host + "/Services/PageService.ashx/GetLiveData");
                postReq.Method = "POST";
                postReq.ContentType = "application/json";
                postReq.CookieContainer = cookies;
                postReq.Headers.Add("x-anti-forgery-token", token);
                postReq.Headers.Add("Origin", host);
                postReq.Referer = host + "/";
                postReq.UserAgent = "Mozilla/5.0";

                byte[] postBytes = Encoding.UTF8.GetBytes(jsonPayload);
                postReq.ContentLength = postBytes.Length;
                using (var stream = postReq.GetRequestStream())
                    stream.Write(postBytes, 0, postBytes.Length);

                string responseText;
                using (var response = postReq.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                    responseText = reader.ReadToEnd();

                var serializer = new JavaScriptSerializer();
                var root = serializer.Deserialize<Dictionary<string, object>>(responseText);
                var responseInfoMap = (Dictionary<string, object>)root["ResponseInfoMap"];
                var guestSessionInfo = (Dictionary<string, object>)responseInfoMap["GuestSessionInfo"];
                var sessions = (ArrayList)guestSessionInfo["Sessions"];

                if (sessions.Count == 0)
                    throw new Exception("No sessions found.");

                var session = (Dictionary<string, object>)sessions[0];
                string guid = (string)session["SessionID"];

                // Step 3: LogInitiatedJoin Call
                string joinPath = "WindowsDesktop[6.0-X]:Chrome:Default/WindowsInstallerDownloadZip";
                cookies.Add(new Cookie("settings", "{\"joinPath\":\"" + joinPath + "\"}", "/", "remote.onsitedentalsystems.com"));

                string logPayload = "[\"" + guid + "\",2,\"(" + joinPath + ") Mozilla/5.0\"]";
                var logReq = (HttpWebRequest)WebRequest.Create(host + "/Services/PageService.ashx/LogInitiatedJoin");
                logReq.Method = "POST";
                logReq.ContentType = "application/json";
                logReq.CookieContainer = cookies;
                logReq.Headers.Add("x-anti-forgery-token", token);
                logReq.Headers.Add("x-unauthorized-status-code", "403");
                logReq.UserAgent = "Mozilla/5.0";
                logReq.Referer = host + "/";
                logReq.Accept = "*/*";

                byte[] logBytes = Encoding.UTF8.GetBytes(logPayload);
                logReq.ContentLength = logBytes.Length;
                using (var stream = logReq.GetRequestStream())
                    stream.Write(logBytes, 0, logBytes.Length);
                logReq.GetResponse().Close();

                // Step 3.5: Extract Script.ashx GUID and parse it
                var scriptMatch = Regex.Match(html, @"Script\.ashx\?__Cache=([a-f0-9\-]+)");
                if (!scriptMatch.Success)
                    throw new Exception("Script GUID not found.");
                string scriptGuid = scriptMatch.Groups[1].Value;

                var scriptReq = (HttpWebRequest)WebRequest.Create(host + "/Script.ashx?__Cache=" + scriptGuid);
                scriptReq.CookieContainer = cookies;
                scriptReq.UserAgent = "Mozilla/5.0";
                scriptReq.Referer = host + "/";

                string scriptContent;
                using (var resp = (HttpWebResponse)scriptReq.GetResponse())
                using (var reader = new StreamReader(resp.GetResponseStream()))
                    scriptContent = reader.ReadToEnd();

                var kMatch = Regex.Match(scriptContent, @"""k""\s*:\s*""([^""]+)""");
                var pMatch = Regex.Match(scriptContent, @"""p""\s*:\s*(\d+)");
                if (!kMatch.Success || !pMatch.Success)
                    throw new Exception("Could not extract download parameters.");
                string k = kMatch.Groups[1].Value;
                string port = pMatch.Groups[1].Value;

                // Step 4: Download and launch
                string zipUrl = host + "/Bin/ScreenConnect.WindowsClient.zip?h=remote.onsitedentalsystems.com&p=" + port + "&k=" + WebUtility.UrlEncode(k) + "&s=" + guid + "&i=Untitled%20Session&e=Support&y=Guest&r=";
                string tempDir = Path.Combine(Path.GetTempPath(), "SCDownload");
                string zipPath = Path.Combine(tempDir, "client.zip");

                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                var zipReq = (HttpWebRequest)WebRequest.Create(zipUrl);
                zipReq.CookieContainer = cookies;
                zipReq.UserAgent = "Mozilla/5.0";
                zipReq.Referer = host + "/";
                zipReq.Accept = "*/*";

                using (var zipResp = zipReq.GetResponse())
                using (var fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
                    zipResp.GetResponseStream().CopyTo(fs);

                ZipFile.ExtractToDirectory(zipPath, tempDir);

                string[] exeFiles = Directory.GetFiles(tempDir, "*.exe", SearchOption.AllDirectories);
                if (exeFiles.Length == 0)
                    throw new Exception("No .exe found in zip.");

                Process.Start(new ProcessStartInfo
                {
                    FileName = exeFiles[0],
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(exeFiles[0])
                });

                //MessageBox.Show("Client launched successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
