using NetworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using NetworkCore.Control;
using NetworkCore.Commands;

namespace SetWallpaper
{
    public partial class Form1 : Form
    {
        private ClientManager m_client;
        private Wallpaper.Style m_wallpaperStyle;
        private Panel m_overlay;

        private bool IsConnected { get { return m_client != null; } }

        private string Statut
        {
            get { return lblStatut.Text; }
            set { lblStatut.Text = value; }
        }

        public Form1()
        {
            InitializeComponent();
            UpdateUIState();
            Statut = "Not connected";
            m_wallpaperStyle = Wallpaper.Style.Center;
            logViewer.Document.Body.DragOver += Body_DragOver;

			foreach(Wallpaper.Style style in Enum.GetValues(typeof(Wallpaper.Style)))
			{
				cboStyle.Items.Add(style);
			}

			cboStyle.SelectedItem = Wallpaper.Style.Center;
        }


        private void selectImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog opfDiag = new OpenFileDialog();
            opfDiag.Filter = "Image Files (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png";


            if (opfDiag.ShowDialog() == DialogResult.OK)
            {
                SendWallpaper(opfDiag.FileName);
            }
        }

        private void SendWallpaper(string path)
        {
            try
            {
                Image image = Image.FromFile(path);
                logViewer.WriteLine("Sending " + Path.GetFileName(path) + " to server...");
                Statut = "Sending " + Path.GetFileName(path) + " to server...";

				m_client.SendCommand(new SetWallpaperCommand(image, m_wallpaperStyle));

                Statut = "Image " + Path.GetFileName(path) + " successfully sent";
            }
            catch (ArgumentException ex)
            {
                logViewer.WriteLine("Selected image is invalid:" + Environment.NewLine + ex.Message, MessageType.Error);
                Statut = "Error with file: " + path;
				if (!this.Visible)
					notifyIcon.ShowBalloonTip(3000, "SetWallpaper", "Error with file \"" + path + "\". See log for more details", ToolTipIcon.Error);

            }
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
			Application.Exit();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            bool isConnected = m_client.FindServer(m_client.Port);

            if (InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    ConnectionFound(isConnected);
                });
            }
            else
            {
                ConnectionFound(isConnected);
            }

        }

        private void ConnectionFound(bool isConnected)
        {
            pgbProgress.Visible = false;
            mnuConnect.Enabled = true;

            if (!isConnected)
            {
                logViewer.WriteLine("No server found", MessageType.Error);
                Statut = "No server found";
				if (!this.Visible)
					notifyIcon.ShowBalloonTip(2000, "SetWallpaper", "No server found", ToolTipIcon.Warning);

                m_client.Disconnect();
                m_client = null;
            }
            else
            {
                logViewer.WriteLine("Connected to server " + m_client.Ip + ":" + m_client.Port);
                Statut = "Connected to server " + m_client.Ip + ":" + m_client.Port;
				if (!this.Visible)
					notifyIcon.ShowBalloonTip(2000, "SetWallpaper", "Connected to server " + m_client.Ip + ":" + m_client.Port, ToolTipIcon.Info);

                m_client.OnDisconnected += m_client_OnDisconnected;
				m_client.OnCommandError += m_client_OnCommandError;
				m_client.OnCommandReceived += m_client_OnCommandReceived;

                logViewer.WriteLine("Ready to receive a wallpaper");
            }

            UpdateUIState();
        }

		#region Client events

		#region Command handler

		#region Command error

		void m_client_OnCommandError(object sender, AltarNet.TcpErrorEventArgs e)
		{
			if (InvokeRequired)
			{
				this.BeginInvoke((MethodInvoker)delegate
				{
					OnCommandError(e);
				});
			}
			else
			{
				OnCommandError(e);
			}
		}

		private void OnCommandError(AltarNet.TcpErrorEventArgs e)
		{
			logViewer.WriteLine("A command from the server could not be read!", MessageType.Error);
		}

		#endregion

		#region Command received
		
		void m_client_OnCommandReceived(object sender, CommandReceivedArgs e)
		{
			if (InvokeRequired)
			{
				this.BeginInvoke((MethodInvoker)delegate
				{
					OnCommandReceived(e);
				});
			}
			else
			{
				OnCommandReceived(e);
			}
		}

		private void OnCommandReceived(CommandReceivedArgs e)
		{
			switch (e.Command.Type)
			{
				case NetworkCore.Commands.CommandType.Notification:
					break;
				case NetworkCore.Commands.CommandType.SetWallpaper:
					OnWallpaperReceived((SetWallpaperCommand)e.Command);
					break;
			}
		}

		#endregion

		#region OnWallpaperReceived

		private void OnWallpaperReceived(SetWallpaperCommand command)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "temp_wallpaper.wllpr");

            if (File.Exists(path))
                File.Delete(path);

			logViewer.WriteLine("New wallpaper received (" + command.Style + ")");

			command.Image.Save(path);
			command.Image.Dispose();
			Wallpaper.Set(new Uri(path), command.Style);
            File.Delete(path);
        }

		#endregion

		#region OnNotificationReceived

		private void OnNotificationReceived(NotificationCommand e)
        {
            logViewer.WriteLine(e.Text, MessageType.Notification);
        }

		#endregion

		#endregion

		void m_client_OnDisconnected(object sender, AltarNet.TcpEventArgs e)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    OnDisconnected();
                });
            }
            else
            {
                OnDisconnected();
            }
        }

		private void OnDisconnected()
		{
			if (m_client == null)
				return;

			logViewer.WriteLine("Connection lost with " + m_client.Ip + ":" + m_client.Port, MessageType.Error);
			Statut = "Connection lost with " + m_client.Ip + ":" + m_client.Port;

			if (!this.Visible)
				notifyIcon.ShowBalloonTip(3000, "SetWallpaper", "Connection lost with " + m_client.Ip + ":" + m_client.Port, ToolTipIcon.Warning);

			m_client = null;
			UpdateUIState();
		}

		#endregion

		private void UpdateUIState()
        {
            if (IsConnected)
            {
                mnuConnect.Text = "&Disconnect";
            }
            else
            {
                mnuConnect.Text = "&Connect...";
            }

			mnuIcoSetWallpaper.Enabled = IsConnected;
			mnuIcoConnect.Enabled = !IsConnected;
			mnuIcoFindServer.Enabled = !IsConnected;
			mnuIcoDisconnect.Enabled = IsConnected;

			mnuFindServer.Enabled = !IsConnected;
            mnuSetWallpaper.Enabled = IsConnected;

			btnSendWallpaper.Enabled = IsConnected;
        }

		#region Drag&drop events

		private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            //e.Effect = DragDropEffects.Copy;
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (FileList.Length > 0)
            {
                SendWallpaper(FileList[0]);
            }

            HideOverlay();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (!IsConnected)
                return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                ShowOverlay();
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

		void Body_DragOver(object sender, HtmlElementEventArgs e)
		{
			if (IsConnected)
				ShowOverlay();
		}

        private void Form1_DragLeave(object sender, EventArgs e)
        {
            HideOverlay();
        }

		#endregion

		private void ShowOverlay()
        {
            if (m_overlay != null)
                return;

            m_overlay = new Panel();
            m_overlay.Dock = System.Windows.Forms.DockStyle.Fill;
            m_overlay.Location = new System.Drawing.Point(0, 24);
            m_overlay.Name = "overlay";
            m_overlay.Size = new System.Drawing.Size(598, 245);
            m_overlay.TabIndex = 8;
            m_overlay.BackColor = SystemColors.Control;
            this.Controls.Add(m_overlay);
            m_overlay.BringToFront();
        }

        private void HideOverlay()
        {
            if (m_overlay == null)
                return;
            this.Controls.Remove(m_overlay);
            m_overlay = null;

        }

        private void mnuConnect_Click(object sender, EventArgs e)
        {
            if (!IsConnected)
            {
                new frmConnect(this).ShowDialog();
            }
            else
            {
                m_client.Disconnect();
                m_client = null;
                logViewer.WriteLine("You have been disconnected from the server");
                Statut = "Successfully disconnected";
                UpdateUIState();
            }
        }
        private void mnuFindServer_Click(object sender, EventArgs e)
        {
            if (IsConnected)
                return;

            logViewer.WriteLine("Looking for server...");
            Statut = "Finding a server to connect...";
            pgbProgress.Visible = true;
            pgbProgress.Style = ProgressBarStyle.Marquee;
            mnuFindServer.Enabled = false;
            mnuConnect.Enabled = false;
            m_client = new ClientManager();

            backgroundWorker1.RunWorkerAsync();

        }

        public async void Connect(IPAddress ip, int port)
        {
            if (IsConnected)
                return;

            logViewer.WriteLine("Connecting to " + ip + ":" + port + "...");
            Statut = "Connecting to " + ip + ":" + port + "...";
			if (!this.Visible)
				notifyIcon.ShowBalloonTip(1000, "SetWallpaper", "Connecting to " + ip + ":" + port + "...", ToolTipIcon.None);


            m_client = new ClientManager(ip, port);
            pgbProgress.Visible = true;
            mnuConnect.Enabled = false;
            mnuFindServer.Enabled = false;

            bool isConnected = await m_client.ConnectAsync();

            ConnectionFound(isConnected);
        }

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;

				this.Hide();
				notifyIcon.ShowBalloonTip(2500, "SetWallpaper", "SetWallpaper is still running in the background...", ToolTipIcon.Info);
			}
		}

		private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if(e.Button == System.Windows.Forms.MouseButtons.Left)
				this.Show();
		}

		private void mnuIcoOpen_Click(object sender, EventArgs e)
		{
			this.Show();
		}

		private void cboStyle_SelectedIndexChanged(object sender, EventArgs e)
		{
			m_wallpaperStyle = (Wallpaper.Style)cboStyle.SelectedItem;
		}

    }
}
