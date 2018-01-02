using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetSHAForJenkinsBuild
{
    public partial class Form1 : Form
    {
        bool loginPrompt = false;
        bool navigated2BuildPage = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            navigated2BuildPage = false;
            loginPrompt = false;
            if (!textBox2.Text.Contains("#"))
            {
                textBox2.Text = "#" + textBox2.Text;
            }

            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Url = new Uri("https://jenkins-mtp-acc.platform.mnscorp.net/job/MTP_Core/job/mtp_allocation_orchestration/job/build_mtp_allocation_orchestration/");
            webBrowser1.Refresh();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string buildNo = "";
            string responseText = ((WebBrowser)sender).DocumentText;
            HtmlDocument doc = webBrowser1.Document;
            if (loginPrompt == true)
            {
                loginPrompt = false;
                HtmlElement username = doc.GetElementById("login_field");
                if (username != null)
                {
                    HtmlElement password = doc.GetElementById("password");
                    HtmlElement submit = doc.GetElementById("commit");
                    username.SetAttribute("value", textBox3.Text);
                    password.SetAttribute("value", textBox4.Text);
                    submit.InvokeMember("click");
                }
                else if (responseText.Contains("Reauthorization"))
                {
                    HtmlElementCollection buttons = doc.GetElementsByTagName("A");
                    HtmlElement button = buttons[1];
                    button.InvokeMember("click");
                }
            }
            else if (responseText.Contains("Two-factor authentication"))
            {
                HtmlElement otp = doc.GetElementById("otp");
                if (otp != null)
                {
                    HtmlElementCollection forms = doc.Forms;
                    HtmlElement mainForm = forms[2];
                    HtmlElement submit = mainForm.GetElementsByTagName("Button")[0];
                    otp.SetAttribute("value", textBox5.Text);
                    submit.InvokeMember("click");
                }
            }
            else if (responseText.Contains("Revision"))
            {
                int startIndex = responseText.IndexOf("Revision");
                int spaceIndex = responseText.IndexOf(" ", startIndex + 1);
                string SHA = responseText.Substring(spaceIndex + 1, 40);
                textBox1.Text = SHA;
            }
            else if (responseText.Contains(textBox2.Text))
            {
                HtmlElementCollection anchorTags = doc.GetElementsByTagName("A");
                foreach (HtmlElement aTag in anchorTags)
                {
                    //Console.WriteLine(aTag.InnerHtml);
                    if (aTag.InnerHtml != null)
                    {
                        // Create two different encodings.
                        Encoding ascii = Encoding.ASCII;
                        Encoding unicode = Encoding.Unicode;

                        // Convert the string into a byte array.
                        byte[] unicodeBytes = unicode.GetBytes(aTag.InnerHtml);

                        // Perform the conversion from one encoding to the other.
                        byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicodeBytes);

                        // Convert the new byte[] into a char[] and then into a string.
                        char[] asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
                        ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
                        string asciiString = new string(asciiChars);

                        buildNo = asciiString.Replace("?", "").Trim();

                        if (buildNo == textBox2.Text)
                        {
                            navigated2BuildPage = true;
                            aTag.InvokeMember("click");
                            break;
                        }
                    }
                }
            }            
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            string responseText = ((WebBrowser)sender).DocumentText;
            if (responseText.Contains("Authentication required"))
            {
                loginPrompt = true;
            }
        }
    }
}
