﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server.View
{
    public partial class ucRunServer : UserControl
    {
        private event EventHandler<Controller.EventSendData> eventStartServer;
        public event EventHandler<Controller.EventSendData> EventStartServer
        {
            add { eventStartServer += value; }
            remove { eventStartServer -= value; }
        }

        private static bool start = false;


        public ucRunServer()
        {
            if (!this.DesignMode)
            {
                InitializeComponent();
            }
        }

        private void ucRunServer_Load(object sender, EventArgs e)
        {
            resizeFill();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!start)
            {
                string path = Path.GetDirectoryName(Application.ExecutablePath);
                path = Path.Combine(path, Controller.Constant.nameFolderSaveFile);
                path = Path.Combine(path, Controller.Constant.nameFileSetting);

                if (File.Exists(path))
                {
                    Controller.IO_INI ini = new Controller.IO_INI(path);
                    string ip = ini.IniReadValue(Controller.Constant.sectionInfo, Controller.Constant.keyIP);
                    string port = ini.IniReadValue(Controller.Constant.sectionInfo, Controller.Constant.keyPort);

                    eventStartServer(this, new Controller.EventSendData(ip, port));

                    Socket sckServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        sckServer.Bind(new IPEndPoint(IPAddress.Any, Convert.ToInt32(port)));
                        sckServer.Listen(100);
                        AppendText(txtCmd, "Server start. Waiting for client ..........", new Tuple<int, int, int>(165, 42, 42));
                    }
                    catch
                    {
                        return;
                    }

                    start = true;

                    Thread Listening = new Thread(() =>
                    {
                        try
                        {
                            while (true)
                            {
                                Socket sckClient = sckServer.Accept();

                                Controller.ConnectionHandle server = new Controller.ConnectionHandle(sckClient, txtCmd);
                                server.Run();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    });
                    Listening.IsBackground = true;
                    Listening.Start();

                }
                else
                {
                    MessageBox.Show("Không tìm thấy file " + Controller.Constant.nameFileSetting + "! Vui lòng kiểm tra lại", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                AppendText(txtCmd, "Server was started!", new Tuple<int, int, int>(165, 42, 42));
            }

        }

        private void btnStop_Click(object sender, EventArgs e)
        {

        }





        private void AppendText(RichTextBox box, string text, Tuple<int, int, int> color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = Color.FromArgb(color.Item1, color.Item2, color.Item3);
            box.AppendText(text + "\r\n");
            box.SelectionColor = box.ForeColor;
        }

        public void resizeFill()
        {
            this.Width = this.Parent.Width;
            this.Height = this.Parent.Height;
        }
    }
}
