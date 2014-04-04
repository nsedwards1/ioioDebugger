using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace ioioDebuger
{
    public partial class ioioForm : Form
    {
        BindingList<pinObject> pinList = new BindingList<pinObject>(); //List to display in datagridview
        ioioIO myIoioIo = new ioioIO(); //may need to rename something here...
        bool run = false;

        List<DHControl> m_inputControls = new List<DHControl>();
        
        public ioioForm()
        {
            InitializeComponent();
            timer1.Enabled = true;

            m_inputControls.Add(new DHControl("RPM Lever", FloatIntBoolNone.Float, textBox1, false, -0.01f));
            m_inputControls.Add(new DHControl("Green Button", FloatIntBoolNone.Int, tb2, false));
        }
        
        class pinObject
        {
            public string number { get; set; }
            public string name { get; set; }
            public string status { get; set; }
            public string calibrated { get; set; }
            public string type { get; set; }
        }

        private BindingList<pinObject> parseXml (string xmlString)
        {
            richTextBox1.Text = xmlString;
            pinList.Clear();
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                while (reader.Read())
                {
                    if (reader.Name == "pin")
                    {
                        pinObject pin = new pinObject();
                        pin.number = reader.GetAttribute("num");
                        pin.name = reader.GetAttribute("name");
                        pin.status = reader.GetAttribute("status");
                        pin.calibrated = reader.GetAttribute("calibrated");
                        pin.type = reader.GetAttribute("type");
                        if (pin.type == "din")
                        {
                            DataHolderIface.SetIntVal(pin.name, Convert.ToInt32(Math.Floor(Convert.ToDouble(pin.calibrated))));
                        }
                        else if (pin.type == "ain")
                        {
                            DataHolderIface.SetFloatVal(pin.name, float.Parse(pin.calibrated));
                        }
                        pinList.Add(pin);
                    }
                }
            }
            dataGridView1.DataSource = pinList;
            return pinList;
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            if (run)
            {
                run = false;
                runButton.Text = "Start";
                tbStatusUrl.Enabled = true;
                runLabel.Text = "stopped";
            }
            else
            {
                run = true;
                runButton.Text = "Stop";
                tbStatusUrl.Enabled = false;
                runLabel.Text = "running...";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (run)
            {
                string xmlString = myIoioIo.webRequest(tbStatusUrl.Text);
                parseXml(xmlString);
            }

            foreach (DHControl dh in m_inputControls)
            {
                dh.readFromDataHolder();
            }
        }

        private void send_button_Click(object sender, EventArgs e)
        {
            try
            {
                myIoioIo.setState(int.Parse(tbPinNum.Text), int.Parse(tbPinValue.Text));
            }
            catch { }
        }

        private void tbPinValue_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                send_button_Click(sender, e);
            }
        }
    }
}
