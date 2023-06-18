using KEL103Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Concurrent;

namespace KEL103DriverUtility
{
    public partial class MainForm : Form
    {
        int max_chart_points = 1000;

        ConcurrentQueue<KEL103State> new_kel103_states = new ConcurrentQueue<KEL103State>();
        Queue<KEL103State> kel103_states = new Queue<KEL103State>();

        Task uiRefresh = null;

        Chart[] charts;
        TextBox[][] text_boxes;
        dynamic[] chart_values = new dynamic[]
        {
            new dynamic[] { new Queue<DateTime>(),  new Queue<double>()},
            new dynamic[] { new Queue<DateTime>(), new Queue<double>() }
        };

        int[] channel_value_type = new int[] {0, 1};
        bool[] channel_value_type_invalid = new bool[] { false, false };

        public MainForm()
        {
            InitializeComponent();

            foreach (Control c in Controls)
                c.Enabled = false;

            menuStrip1.Enabled = true;

            charts = new Chart[]{chart1, chart2 };

            text_boxes = new TextBox[][] { new TextBox[] {textBox1, textBox2, textBox3 }, new TextBox[] { textBox4 , textBox5 , textBox6 } };

            for(int i = 0; i< 2; i++)
            {
                Chart c = charts[i];
                
                c.ChartAreas[0].AxisX.LabelStyle.Format = "dd.hh.mm.ss";
                c.Series[0].XValueType = ChartValueType.DateTime;
            }

            Show();

            uiRefresh = Task.Run(async () => {
                while (true)
                {
                    if (new_kel103_states.Count() > 0)
                    {
                        refreshTask();
                        //Thread.Sleep(100);
                    }
                    else
                        await Task.Delay(10);
                }
            });
            //uiRefresh.Start();
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
        

        private async void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //start the state tracker
            await Task.Run(async () => {

                KEL103StateTracker.NewKEL103StateAvailable += a => onNewState_refresh(a);

                KEL103StateTracker.Start();

                while(!KEL103StateTracker.IsInitComplete)
                    await Task.Delay(1);

                var client = KEL103StateTracker.CheckoutClient();
                var mode = await KEL103Command.GetSystemMode(client);
                KEL103StateTracker.CheckinClient();

                Invoke((MethodInvoker)(() =>
                {
                    comboBox1.SelectedIndex = mode;
                    comboBox2.SelectedIndex = 1;
                    comboBox3.SelectedIndex = 0;

                    foreach (Control c in Controls)
                        c.Enabled = true;
                }));
            });
        }

        private void onNewState_refresh(KEL103State a)
        {
            new_kel103_states.Enqueue(a);
            Console.WriteLine(new_kel103_states.Count());
        }

        private void refreshTask()
        {
            while (new_kel103_states.Count() > 0)
            {
                KEL103State result = null;
                bool safe = new_kel103_states.TryDequeue(out result);
                if (safe)
                    kel103_states.Enqueue(result);
            }

            var a = kel103_states.Last();

            Invoke((MethodInvoker)(() =>
            {
                button2.BackColor = a.InputState ? Color.Red : Color.Green;
                button2.Text = a.InputState ? "Load Active" : "Load Inactive";

                toolStripStatusLabel1.Text = a.ValueAquisitionTimespan.ToString();
            }));

            Parallel.For(0, 2, i =>
            {
                Chart c = charts[i];
                TextBox[] t = text_boxes[i];
                Queue<DateTime> timestamp_queue = chart_values[i][0];
                Queue<double> value_queue = chart_values[i][1];

                if (channel_value_type_invalid[i])
                {
                    value_queue.Clear();
                    timestamp_queue.Clear();

                    //Invoke((MethodInvoker)(() => c.Series[0].Points.Clear()));

                    var cvt = channel_value_type[i];
                    foreach (var kel103val in kel103_states)
                    {
                        var value = new Func<dynamic>(() =>
                        {
                            switch (cvt)
                            {
                                case 0: return a.Voltage;
                                case 1: return a.Current;
                                case 2: return a.Power;
                                default: throw new Exception("invalid field type");
                            }
                        })();

                        value_queue.Enqueue(value);
                        timestamp_queue.Enqueue(kel103val.TimeStamp);
                    }

                    channel_value_type_invalid[i] = false;
                }

                while (kel103_states.Count() > max_chart_points)
                    kel103_states.Dequeue();

                timestamp_queue.Enqueue(a.TimeStamp);

                switch (channel_value_type[i])
                {
                    case 0: value_queue.Enqueue(a.Voltage); break;
                    case 1: value_queue.Enqueue(a.Current); break;
                    case 2: value_queue.Enqueue(a.Power); break;
                }

                Invoke((MethodInvoker)(() =>
                {
                    t[0].Text = KEL103Tools.FormatString(value_queue.Max()); //max
                    t[1].Text = KEL103Tools.FormatString(value_queue.Min()); //min
                    t[2].Text = KEL103Tools.FormatString(value_queue.Average()); //avg

                    c.Series[0].Points.DataBindXY(timestamp_queue, value_queue);

                    var max = value_queue.Max();
                    var min = value_queue.Min();

                    if (max != min)
                    {
                        c.ChartAreas[0].AxisY.Maximum = max + max * 0.1;
                        c.ChartAreas[0].AxisY.Minimum = min - min * 0.1;
                    }
                    else
                    {
                        if (max != 0)
                        {
                            c.ChartAreas[0].AxisY.Maximum = max + max * 0.5;
                            c.ChartAreas[0].AxisY.Minimum = max - max * 0.5;
                        }
                        else
                        {
                            c.ChartAreas[0].AxisY.Maximum = 1;
                            c.ChartAreas[0].AxisY.Minimum = -1;
                        }
                    }
                }));
                //});

                while (timestamp_queue.Count() > max_chart_points)
                {
                    timestamp_queue.Dequeue();
                    value_queue.Dequeue();
                }

                Invoke((MethodInvoker)(() =>
                {
                    Update();
                }));
            });
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            //switch load input on and off

            var client = KEL103StateTracker.CheckoutClient();

            await KEL103Command.SetLoadInputSwitchState(client, !kel103_states.Last().InputState);

            KEL103StateTracker.CheckinClient();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            //enable channel 1 value
            channel_value_type[0] = comboBox3.SelectedIndex > -1 ? comboBox3.SelectedIndex : 0;
            channel_value_type_invalid[0] = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //enable channel 2 value
            channel_value_type[1] = comboBox2.SelectedIndex > -1 ? comboBox2.SelectedIndex : 0;
            channel_value_type_invalid[1] = true;
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
