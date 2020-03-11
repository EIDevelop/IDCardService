using Fleck;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace IDCardService
{
    public partial class IDCardService : ServiceBase
    {
        public IDCardService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Logger("Service Start...");
            WebSocketStart();
        }

        protected override void OnStop()
        {
            Logger("Service Stop...");
        }

        private void Logger(string msg)
        {
            string filePath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "log.txt";

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath, true))
            {
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss - ") + msg);
            }
        }

        public void WebSocketStart()
        {
            Logger("websocket will start...");
            try
            {
                FleckLog.Level = LogLevel.Info;

                var allSockets = new List<IWebSocketConnection>();
                var server = new WebSocketServer("ws://0.0.0.0:50000");

                Logger("websocket connection is OK");

                server.RestartAfterListenError = true;

                server.Start(socket =>
                {
                    socket.OnOpen = () =>
                    {
                        Logger("WebSocket is Open...");
                    };
                    socket.OnClose = () =>
                    {
                        Logger("WebSocket is Close...");
                    };
                    socket.OnMessage = message =>
                    {
                        if (message.Equals("ACT_READ"))
                        {
                            socket.Send(ReadCard());
                        }
                    };
                });
            }
            catch (Exception ex)
            {
                Logger("WebSocket Error:" + ex.Message);
                throw;
            }


        }

        private string ReadCard()
        {
            string sMsg = string.Empty;
            string JsonString = string.Empty;

            try
            {
                if (IDCardReader.InitCom())
                {
                    string sPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                     
                    CardInfo objIDCardInfo = IDCardReader.ReadCardInfo(0, out sMsg, sPath);

                    if (objIDCardInfo != null)
                    {
                        JsonString += "{";
                        JsonString += "\"ReadState\":true";
                        JsonString += string.Format(",\"CardNo\":\"{0}\"", objIDCardInfo.CardNo);
                        JsonString += string.Format(",\"Name\":\"{0}\"", objIDCardInfo.Name);
                        JsonString += string.Format(",\"Sex\":\"{0}\"", objIDCardInfo.Sex);
                        JsonString += string.Format(",\"Birthday\":\"{0}\"", objIDCardInfo.Birthday);
                        JsonString += string.Format(",\"Address\":\"{0}\"", objIDCardInfo.Address);
                        JsonString += string.Format(",\"AddressEx\":\"{0}\"", objIDCardInfo.AddressEx);
                        JsonString += string.Format(",\"Department\":\"{0}\"", objIDCardInfo.Department);
                        JsonString += string.Format(",\"StartDate\":\"{0}\"", objIDCardInfo.StartDate);
                        JsonString += string.Format(",\"EndDate\":\"{0}\"", objIDCardInfo.EndDate);
                        JsonString += string.Format(",\"Nation\":\"{0}\"", objIDCardInfo.Nation);
                        JsonString += string.Format(",\"base64Data\":\"{0}\"", ImageToBase64(objIDCardInfo.PhotoPath));
                        JsonString += "}";
                    }
                    else
                    {
                        JsonString = string.Format("{{\"ReadState\":false, \"errMsg\":\"{0}\"}}", sMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger("ReadCard ERROR: " + ex.Message);
                throw ex;
            }
            finally
            {
                IDCardReader.CloseCom(0);
            }

            return JsonString;
        }

        public static string ImageToBase64(string fileFullName)
        {
            string result = string.Empty;
            try
            {
                Bitmap bmp = new Bitmap(fileFullName);
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0; ms.Read(arr, 0, (int)ms.Length);
                bmp.Dispose();
                ms.Close();
                result = Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }


    }
}
