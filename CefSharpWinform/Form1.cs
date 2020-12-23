using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace CefSharpWinform
{
    public partial class Form1 : Form
    {
        string url = "https://wx.qq.com/";
        ChromiumWebBrowser webview;
        Timer timer = new Timer();
        bool isfirsttime = true;
        int count = 0;
        public Form1()
        {
            InitializeComponent();
            if (File.Exists("./Keywords.txt") && File.ReadAllText("./Keywords.txt").Length > 0)
                txt_Keywords.Text = File.ReadAllText("./Keywords.txt");
            if (File.Exists("./Keypersons.txt") && File.ReadAllText("./Keypersons.txt").Length > 0)
                txt_keyperson.Text = File.ReadAllText("./Keypersons.txt");

            webview = new ChromiumWebBrowser(url);
            webview.Dock = DockStyle.Fill;
            this.panel1.Controls.Add(webview);
            timer.Interval = 5000;
            timer.Tick += Timer_Tick;
            timer.Enabled = false;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isfirsttime)
            {
                timer.Interval = 500;
                isfirsttime = false; return;
            }
            object EvaluateJavaScriptResult = new object();
            var script = @"var dataArray= $('.msg');
                for (var i = 0; i < dataArray.length; i++){
                        var keywords='" + txt_Keywords.Text.Trim() + @"';
                        var keypersons='"+txt_keyperson.Text.Trim()+ @"';
                        var strs = keywords.split('|');
                        var persons=keypersons.split('|');
                        for(k=0;k<strs.length;k++)
                            if (dataArray[i].innerText.search(strs[k])>=0&&strs[k].length>0)
                            {   console.log(dataArray[i].outerText);
                                console.log(dataArray[i].innerText);
                                for(j=0;j<persons.length;j++)
                                if (dataArray[i].innerText.search(persons[j])>=0&&persons[j].length>0)
                            {$('.msg')[i].click();" +
                            "$('#editArea').text('我');" +
                             "angular.element(document.getElementsByClassName('btn btn_send')[0]).triggerHandler('click');" +
                             "if(i>0)$('.chat_item .info')[0].click();else $('.chat_item .info')[1].click();" +
                             "$('.msg')[i].click();}}};";
            if (count % 100 == 0 || count % 333 == 0 || count % 555 == 0 || count % 777 == 0)
            {
                Random ran = new Random();
                int RandKey = ran.Next(100, 999);
                //if (RandKey % 2 == 0)
                //    script += "$('.chat_item .info')[1].click();$('.chat_item .info')[0].click();";
            }
            var task = webview.GetBrowser().MainFrame.EvaluateScriptAsync(script, null);

            task.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;
                    EvaluateJavaScriptResult = response.Success ? (response.Result ?? "null") : response.Message;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
            count++;
            if (count > 100000) count = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                object EvaluateJavaScriptResult = new object();
                var script = @"if($('.chat_item .info').length>0)$('.chat_item .info')[0].click(); ";
                var task = webview.GetBrowser().MainFrame.EvaluateScriptAsync(script, null);
                task.ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        var response = t.Result;
                        EvaluateJavaScriptResult = response.Success ? (response.Result ?? "null") : response.Message;
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
                timer.Enabled = true;
            }
            else
            {
                timer.Enabled = false;
            }

        }

        private void txt_Keywords_TextChanged(object sender, EventArgs e)
        {
            File.WriteAllLines("./Keywords.txt", new List<string> { txt_Keywords.Text });
        }

        private void txt_keyperson_TextChanged(object sender, EventArgs e)
        {
            File.WriteAllLines("./Keypersons.txt", new List<string> { txt_keyperson.Text });
        }
    }
}
