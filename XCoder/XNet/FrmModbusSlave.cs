﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using NewLife;
using NewLife.Log;
using NewLife.Net;
using NewLife.Security;
using NewLife.Threading;
using XCoder;
using XCoder.Common;
using XCoder.XNet;

namespace XNet
{
    [Category("网络通信")]
    [DisplayName("ModbusSlave")]
    public partial class FrmModbusSlave : Form, IXForm
    {
        private ControlConfig _config;
        private ILog _log;
        private NetServer _Server;
        private RegisterUnit[] _data;

        #region 窗体
        public FrmModbusSlave()
        {
            InitializeComponent();

            // 动态调节宽度高度，兼容高DPI
            this.FixDpi();

            Icon = IcoHelper.GetIcon("Modbus");
        }

        private void FrmMain_Load(Object sender, EventArgs e)
        {
            _log = new TextControlLog { Control = txtReceive };

            _config = new ControlConfig { Control = this, FileName = "ModbusSlave.json" };
            _config.Load();

            if (cbMode.SelectedIndex < 0) cbMode.SelectedIndex = 0;

            txtReceive.SetDefaultStyle(12);

            // 加载保存的颜色
            UIConfig.Apply(txtReceive);
        }
        #endregion

        #region 收发数据
        private void btnConnect_Click(Object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn.Text == "开始")
            {
                var svr = new NetServer((Int32)numPort.Value)
                {
                    Log = _log
                };
                svr.Received += OnReceived;
                svr.Start();

                _Server = svr;

                ShowData();

                pnlSetting.Enabled = false;
                btn.Text = "停止";

                _config.Save();
            }
            else
            {
                _Server.TryDispose();
                _Server = null;
                _timer.TryDispose();

                pnlSetting.Enabled = true;
                btn.Text = "开始";
            }
        }

        private void ShowData()
        {
            var addr = (Int32)numAddress.Value;
            var count = (Int32)numCount.Value;
            var mode = cbMode.SelectedItem + "";

            var list = new List<RegisterUnit>(count);

            switch (mode)
            {
                case "0x0000":
                    for (var i = 0; i < count; i++)
                    {
                        list.Add(new RegisterUnit { Address = addr + i, Value = 0 });
                    }
                    break;
                case "0x7777":
                    for (var i = 0; i < count; i++)
                    {
                        list.Add(new RegisterUnit { Address = addr + i, Value = 0x7777 });
                    }
                    break;
                case "0xFFFF":
                    for (var i = 0; i < count; i++)
                    {
                        list.Add(new RegisterUnit { Address = addr + i, Value = 0xFFFF });
                    }
                    break;
                case "递增":
                    for (var i = 0; i < count; i++)
                    {
                        list.Add(new RegisterUnit { Address = addr + i, Value = (UInt16)i });
                    }
                    break;
                case "静态随机":
                case "动态随机":
                default:
                    for (var i = 0; i < count; i++)
                    {
                        list.Add(new RegisterUnit { Address = addr + i, Value = (UInt16)Rand.Next(UInt16.MaxValue) });
                    }
                    break;
            }

            if (mode == "动态随机") _timer = new TimerX(DoRefreshData, null, 1_000, 1_000);

            _data = list.ToArray();

            dataGridView1.DataSource = _data;
        }

        private TimerX _timer;
        private void DoRefreshData(Object state)
        {
            if (_data == null) return;

            for (var i = 0; i < _data.Length; i++)
            {
                var x = (Rand.Next(20) - 10) / 100.0;
                _data[i].Value = (UInt16)(_data[i].Value * (1 + x));
            }
            //dataGridView1.DataSource = _data;
            dataGridView1.Refresh();
        }

        private void OnReceived(Object sender, ReceivedEventArgs e)
        {
            var session = sender as ISocketSession;
            if (session == null)
            {
                var ns = sender as INetSession;
                if (ns == null) return;
                session = ns.Session;
            }

            if (NetConfig.Current.ShowReceiveString)
            {
                var line = e.Packet.ToStr();

                _log.Info(line);
            }
        }

        private Int32 _pColor = 0;
        private void timer1_Tick(Object sender, EventArgs e)
        {
            var cfg = NetConfig.Current;
            if (cfg.ColorLog) txtReceive.ColourDefault(_pColor);
            _pColor = txtReceive.TextLength;
        }
        #endregion
    }
}