using KEL103Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace KEL103DriverUtility
{
    public partial class MainForm : Form
    {
        int max_chart_points = 1000;

        KEL103State latest_state;

        Chart[] charts;
        TextBox[][] text_boxes;
        dynamic[] chart_values = new dynamic[]
        {
            new dynamic[] { new Queue<DateTime>(),  new Queue<double>()},
            new dynamic[] { new Queue<DateTime>(), new Queue<double>() }
        };

        public MainForm()
        {
            InitializeComponent();

            charts = new Chart[]{chart1, chart2 };

            text_boxes = new TextBox[][] { new TextBox[] {textBox1, textBox2, textBox3 }, new TextBox[] { textBox4 , textBox5 , textBox6 } };

            for(int i = 0; i< 2; i++)
            {
                Chart c = charts[i];
                
                c.ChartAreas[0].AxisX.LabelStyle.Format = "dd.hh.mm.ss";
                c.Series[0].XValueType = ChartValueType.DateTime;
            }
        }

        //helps to prevent flicker
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var address = await KEL103Driver.KEL103Tools.FindLoadAddress();

            //textBox1.Text = address.ToString();
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //start the state tracker

            KEL103StateTracker.NewKEL103StateAvailable += a =>
            {
                latest_state = a;

                button2.BackColor = a.InputState ? Color.Red : Color.Green;
                button2.Text = a.InputState ? "Load Active" : "Load Inactive";

                Parallel.For(0, 2, i =>
                {
                    Chart c = charts[i];
                    TextBox[] t = text_boxes[i];
                    Queue<DateTime> timestamp_queue = chart_values[i][0];
                    Queue<double> value_queue = chart_values[i][1];

                   
                    timestamp_queue.Enqueue(a.TimeStamp);
                    value_queue.Enqueue(a.Voltage);

                    Invoke((MethodInvoker)(() =>
                    {
                        t[0].Text = KEL103Tools.FormatString(value_queue.Average()); //average
                        t[1].Text = value_queue.Last().ToString(); //latest value
                        t[2].Text = a.ValueAquisitionTimespan.ToString(); //retreival time

                        c.Series[0].Points.DataBindXY(timestamp_queue, value_queue);

                        var max = value_queue.Max();
                        var min = value_queue.Min();

                        try
                        {
                            if (max != min)
                            {
                                c.ChartAreas[0].AxisY.Maximum = max;
                                c.ChartAreas[0].AxisY.Minimum = min;
                            }
                            else
                            {
                                c.ChartAreas[0].AxisY.Maximum = 1 < max ? max : 1;
                                c.ChartAreas[0].AxisY.Minimum = -1 > min ? min : -1;
                            }

                            
                            while (timestamp_queue.Count() > max_chart_points)
                            {
                                timestamp_queue.Dequeue();
                                value_queue.Dequeue();
                            }
                        }
                        catch (System.InvalidOperationException ex)
                        {
                            c.ChartAreas[0].AxisY.Maximum = 1 < max ? max : 1;
                            c.ChartAreas[0].AxisY.Minimum = -1 > min ? min : -1;
                        }
                    }));
                });

                Invoke((MethodInvoker)(() =>
                    {
                        Refresh();
                    }));
            };

            KEL103StateTracker.Start();
        }

        

        private async void button2_Click(object sender, EventArgs e)
        {
            //switch load input on and off

            var client = KEL103StateTracker.CheckoutClient();

            await KEL103Command.SetLoadInputSwitchState(client, !latest_state.InputState);

            KEL103StateTracker.CheckinClient();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            //enable channel 1 monitor
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //enable channel 2 monitor
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            //set mode button

            var selected_index = comboBox1.SelectedIndex;
            if(selected_index > -1)
            {
                var client = KEL103StateTracker.CheckoutClient();
                await KEL103Command.SetSystemMode(client, selected_index);
                KEL103StateTracker.CheckinClient();
            }
        }
    }
}
