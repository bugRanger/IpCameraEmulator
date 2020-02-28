using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emulator;
//using SystemMetrics;

namespace IpCameraEmulatorStd
{
    public partial class FormMain : Form
    {
        private const int DEFAULT_CAMERAS_GENERATION = 10;

        private SystemConfiguration _AppSettings = new SystemConfiguration(@"IpCameraEmulatorStd");
        private Collection<EmulatorChannel> _Channels = null;
        private bool _EmulatorStarted = false;

        public FormMain()
        {
            InitializeComponent();
            Utilities.DoubleBuffered(lvMain, true);   // to eliminate flickering when the listview is updated
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                LoadApplicationSettings();
                DisplaySystemResourceUsage(_AppSettings.ShowSystemResourceUsage);
                _Channels = _AppSettings.Channels;
                RefreshChannelsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveApplicationSettings();
        }

        private bool LoadApplicationSettings()
        {
            try
            {
                if (_AppSettings == null)
                    throw new InvalidOperationException();

                if (!_AppSettings.Load())
                    return false;
                else
                {
                    if (_AppSettings.WindowLocation.X >= 0 &&
                      _AppSettings.WindowLocation.X < Screen.FromControl(this).Bounds.Width &&
                      _AppSettings.WindowLocation.Y >= 0 &&
                      _AppSettings.WindowLocation.Y < Screen.FromControl(this).Bounds.Height)
                    {
                        this.Location = _AppSettings.WindowLocation;
                    }

                    if (_AppSettings.WindowSize.Width > 0 && _AppSettings.WindowSize.Height > 0)
                    {
                        this.Size = _AppSettings.WindowSize;
                    }

                    if (_AppSettings.AppWIndowState != FormWindowState.Minimized)
                    {
                        this.WindowState = _AppSettings.AppWIndowState;
                    }

                    return true;
                }
            }
            catch
            {
                throw;
            }
        }

        private bool SaveApplicationSettings()
        {
            try
            {
                if (_AppSettings == null)
                    throw new InvalidOperationException();

                _AppSettings.AppWIndowState = this.WindowState;
                _AppSettings.WindowLocation = this.Location;

                if (this.WindowState != FormWindowState.Normal)
                    _AppSettings.WindowSize = new Size(this.RestoreBounds.Width, this.RestoreBounds.Height);
                else
                    _AppSettings.WindowSize = new Size(this.Width, this.Height);

                return _AppSettings.Save();
            }
            catch
            {
                throw;
            }
        }

        private void tbAdd_Click(object sender, EventArgs e)
        {
            try
            {
                AddOneChannel();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private bool AddOneChannel()
        {
            using (FormChannel channelForm = new FormChannel())
            {
                channelForm.SetEditMode(false, null);
                channelForm.AllChannels = _Channels;
                var result = channelForm.ShowDialog() == DialogResult.OK;

                if (result)
                    AddChannels(channelForm.CurrentChannels.ToArray());

                return result;
            }
        }

        private void AddChannels(EmulatorChannel[] newChannels, bool generate = false)
        {
            if (newChannels != null && newChannels.Length > 0)
            {
                var nextIndex = GetNextChannelIndex();

                foreach (var copyVersion in newChannels)
                {
                    var channelName = generate || string.IsNullOrWhiteSpace(copyVersion.Name)
                        ? $"Channel {nextIndex}"
                        : copyVersion.Name;
                    var channelPort = copyVersion.RtspPort  + (generate ? nextIndex - copyVersion.Id : 0);
                    
                    _Channels.Add(new EmulatorChannel(nextIndex, channelName, copyVersion.MediaPath, channelPort, true));
                    nextIndex++;
                }
            }
            RefreshChannelsList();
            SaveApplicationSettings();
        }

        private void ReturnOneChannel()
        {
            if (!_Channels.Any())
                return;

            var restore = _Channels[0];

            _Channels.Clear();
            _Channels.Add(restore);

            RefreshChannelsList();
            SaveApplicationSettings();
        }

        private void RefreshChannelsList()
        {
            try
            {
                lvMain.Items.Clear();
                if (_Channels != null)
                {
                    foreach (EmulatorChannel channel in _Channels)
                    {
                        ListViewItem item = lvMain.Items.Add(channel.Id.ToString());
                        item.UseItemStyleForSubItems = false;
                        item.ForeColor = channel.Enabled ? Color.Black : Color.Silver;
                        ListViewItem.ListViewSubItem subItem = item.SubItems.Add(channel.Name);
                        subItem.ForeColor = channel.Enabled ? Color.Black : Color.Silver;
                        subItem = item.SubItems.Add(channel.MediaPath);
                        subItem.ForeColor = channel.Enabled ? Color.Black : Color.Silver;
                        subItem = item.SubItems.Add(channel.RtspPort.ToString());
                        subItem.ForeColor = channel.Enabled ? Color.Black : Color.Silver;
                        subItem = item.SubItems.Add(channel.Enabled ? "Enabled" : "Disabled");
                        subItem.ForeColor = channel.Enabled ? Color.Black : Color.Silver;
                        subItem = item.SubItems.Add(string.Empty);
                        subItem.ForeColor = channel.Enabled ? Color.Black : Color.Silver;
                        item.Tag = channel;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private int GetNextChannelIndex()
        {
            try
            {
                if (_Channels != null && _Channels.Count > 0)
                {
                    int lastIndex = 0;
                    List<int> indices = new List<int>();
                    foreach (EmulatorChannel channel in _Channels)
                    {
                        indices.Add(channel.Id);
                    }
                    lastIndex = indices.Max() + 1;
                    return lastIndex;
                }
                else
                    return 1;
            }
            catch
            {
                throw;
            }
        }

        private void lvMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbEdit.Enabled = tbDelete.Enabled = (lvMain.SelectedItems.Count == 1 && !_EmulatorStarted);
        }

        private void lvMain_DoubleClick(object sender, EventArgs e)
        {
            EditChannel();
        }

        private void tbEdit_Click(object sender, EventArgs e)
        {
            EditChannel();
        }

        private void EditChannel()
        {
            try
            {
                if (lvMain.SelectedItems.Count > 0 && !_EmulatorStarted)
                {
                    EmulatorChannel selectedChannel = (EmulatorChannel)lvMain.SelectedItems[0].Tag;
                    if (selectedChannel == null)
                        return;

                    using (FormChannel editChannelForm = new FormChannel())
                    {
                        editChannelForm.SetEditMode(true, selectedChannel);
                        editChannelForm.AllChannels = _Channels;
                        if (editChannelForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (editChannelForm.CurrentChannels != null && editChannelForm.CurrentChannels.Count > 0)
                            {
                                EmulatorChannel updatedChannel = editChannelForm.CurrentChannels[0];
                                foreach (EmulatorChannel channel in _Channels)
                                {
                                    if (channel.Id == updatedChannel.Id)
                                    {
                                        channel.Name = updatedChannel.Name;
                                        channel.MediaPath = updatedChannel.MediaPath;
                                        channel.RtspPort = updatedChannel.RtspPort;
                                        channel.Enabled = updatedChannel.Enabled;
                                    }
                                }
                            }
                            RefreshChannelsList();
                            SaveApplicationSettings();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void tbDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvMain.SelectedItems.Count > 0)
                {
                    if (lvMain.SelectedItems == null || lvMain.SelectedItems.Count == 0)
                        return;
                    else
                    {
                        string channelName = (lvMain.SelectedItems[0].Tag is EmulatorChannel) ?
                          ((EmulatorChannel)lvMain.SelectedItems[0].Tag).Name + " ?" : string.Empty;
                        if (MessageBox.Show("Are you sure you want to delete" + Environment.NewLine + channelName,
                          "Delete Channel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                          System.Windows.Forms.DialogResult.Yes)
                        {
                            foreach (ListViewItem selectedItem in lvMain.SelectedItems)
                            {
                                if (selectedItem.Tag is EmulatorChannel)
                                {
                                    _Channels.Remove((EmulatorChannel)selectedItem.Tag);
                                }
                            }
                            RefreshChannelsList();
                            SaveApplicationSettings();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void tbStart_Click(object sender, EventArgs e)
        {
            tbStart.Enabled = false;
            try
            {
                if (tbStart.Text.StartsWith("Start"))
                {
                    DisplayPendingChannelStatuses();
                    StartEmulator();
                    _EmulatorStarted = true;
                    LockUI(true);
                    tbStart.Text = "Stop";
                    tbStart.Image = Properties.Resources.stop;
                    tbStart.ToolTipText = "Stop Emulator Service";
                    tmrGetStatus.Enabled = true;
                }
                else
                {
                    //StartEmulator();
                    tmrGetStatus.Enabled = false;
                    StopEmulator();
                    tbStart.Text = "Start";
                    tbStart.Image = Properties.Resources.start;
                    tbStart.ToolTipText = "Start Emulator Service";
                    tsslCpuUsage.Text = tsslMemoryUsage.Text = string.Empty;
                    ClearChannelStatuses();
                    _EmulatorStarted = false;
                    LockUI(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                tbStart.Enabled = true;
            }
        }

        private void LockUI(bool disableConfiguration)
        {
            if (disableConfiguration)
            {
                tbAdd.Enabled = tbEdit.Enabled = tbDelete.Enabled = tbSettings.Enabled = sbGenerate.Enabled = sbClearChannel.Enabled = false;
            }
            else
            {

                tbAdd.Enabled = tbSettings.Enabled = sbGenerate.Enabled = sbClearChannel.Enabled = true;
                tbEdit.Enabled = tbDelete.Enabled = (lvMain.SelectedItems.Count == 1);
            }
        }

        private void StartEmulator()
        {
            try
            {
                if (_Channels != null)
                {
                    Task.Run(() =>
                    {
                        foreach (var channel in _Channels)
                        {
                            if (!channel.Enabled)
                                continue;

                            channel.Engine = new EmulatorEngine(channel.Name, channel.MediaPath, channel.RtspPort);
                            channel.Engine.Start();
                        }
                    });
                }
            }
            catch
            {
                throw;
            }
        }

        private void StopEmulator()
        {
            try
            {
                if (_Channels != null)
                {
                    foreach (EmulatorChannel channel in _Channels)
                    {
                        if (channel.Engine != null)
                        {
                            Task.Run(delegate
                            {
                                channel.Engine.Stop();
                            });
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private void tmrGetStatus_Tick(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvMain.Items)
            {
                if (!(item.Tag is EmulatorChannel))
                    continue;
                
                    var channel = (EmulatorChannel)item.Tag;

                    if (channel.Engine != null && channel.Enabled)
                    {
                        channel.Status = channel.Engine.GetChannelStatus();
                        lvMain.BeginUpdate();
                        try
                        {
                            item.SubItems[5].Text = channel.Status.ToString();
                            item.SubItems[5].ForeColor = channel.Status != ChannelStatus.Playing ? Color.Red :  Color.Green;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        finally
                        {
                            lvMain.EndUpdate();
                        }
                    }
                    else
                    {
                        item.SubItems[5].Text = string.Empty;
                    }
            }

            //if (_AppSettings.ShowSystemResourceUsage)
            //{
            //    tsslCpuUsage.Text = OsMetrics.GetCpuUsage().ToString() + " %";
            //    tsslMemoryUsage.Text = OsMetrics.GetMemoryUsage().ToString() + " %";
            //}
        }

        private void ClearChannelStatuses()
        {
            foreach (ListViewItem item in lvMain.Items)
            {
                item.SubItems[5].Text = string.Empty;
            }
        }

        private void DisplayPendingChannelStatuses()
        {
            foreach (ListViewItem item in lvMain.Items)
            {
                if (item.Tag is EmulatorChannel && ((EmulatorChannel)item.Tag).Enabled)
                {
                    item.SubItems[5].ForeColor = Color.Silver;
                    item.SubItems[5].Text = "Initalizing...";
                }
                else
                {
                    item.SubItems[5].Text = string.Empty;
                }
            }
        }

        private void lvMain_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && !_EmulatorStarted)
            {
                if (lvMain.FocusedItem.Bounds.Contains(e.Location) == true)
                {
                    cmsMain.Show(Cursor.Position);
                }
            }
        }

        private void tsmiEnableChannel_Click(object sender, EventArgs e)
        {
            if (lvMain.SelectedItems.Count > 0)
            {
                foreach (ListViewItem selectedItem in lvMain.SelectedItems)
                {
                    if (selectedItem.Tag is EmulatorChannel)
                    {
                        EmulatorChannel selectedChannel = (EmulatorChannel)selectedItem.Tag;
                        selectedChannel.Enabled = true;
                    }
                }
                RefreshChannelsList();
                SaveApplicationSettings();
            }
        }

        private void tsmiDisableChannel_Click(object sender, EventArgs e)
        {
            if (lvMain.SelectedItems.Count > 0)
            {
                foreach (ListViewItem selectedItem in lvMain.SelectedItems)
                {
                    if (selectedItem.Tag is EmulatorChannel)
                    {
                        EmulatorChannel selectedChannel = (EmulatorChannel)selectedItem.Tag;
                        selectedChannel.Enabled = false;
                    }
                }
                RefreshChannelsList();
                SaveApplicationSettings();
            }
        }

        private void tbSettings_Click(object sender, EventArgs e)
        {
            using (FormSettings settingsForm = new FormSettings(_AppSettings))
            {
                if (settingsForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SaveApplicationSettings();
                    DisplaySystemResourceUsage(_AppSettings.ShowSystemResourceUsage);
                }
            }
        }

        private void DisplaySystemResourceUsage(bool enable)
        {
            tsslCpuLabel.Visible = tsslCpuUsage.Visible = tsslMemLabel.Visible = tsslMemoryUsage.Visible =
              tsslNetLabel.Visible = tsslNwUsage.Visible = enable;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox aboutBox = new AboutBox())
            {
                aboutBox.ShowDialog();
            }
        }

        private void tssbInfo_ButtonClick(object sender, EventArgs e)
        {
            tssbInfo.ShowDropDown();
        }

        private void sbGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                var containsOne = _Channels.Any() || AddOneChannel();
                if (!containsOne)
                    throw new Exception("No contains a channel to generate");

                AddChannels(Enumerable.Repeat(_Channels[0], DEFAULT_CAMERAS_GENERATION).ToArray(), true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void sbClearChannel_Click(object sender, EventArgs e)
        {
            ReturnOneChannel();
        }
    }
}
