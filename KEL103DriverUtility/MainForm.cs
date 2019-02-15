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
        Queue<double> chart_values = new Queue<double>();

        KEL103State latest_state;

        public MainForm()
        {
            InitializeComponent();

            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "dd.hh.mm.ss.fff";
            chart1.Series[0].XValueType = ChartValueType.DateTime;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var address = await KEL103Driver.KEL103Tools.FindLoadAddress();

            //textBox1.Text = address.ToString();
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //start the state tracker

            

            Console.WriteLine("here");

            KEL103StateTracker.NewKEL103StateAvailable += a =>
            {
                latest_state = a;

                chart_values.Enqueue(a.Voltage);

                Invoke((MethodInvoker)(() =>
                {
                    var max = chart_values.Max();
                    var min = chart_values.Min();

                    button2.BackColor = a.input_state ? Color.Red : Color.Green;
                    button2.Text = a.input_state ? "Load Active" : "Load Inactive";

                    Console.WriteLine(max);
                    Console.WriteLine(min);

                    try
                    {
                        if(max != min)
                        {
                            chart1.ChartAreas[0].AxisY.Maximum = max;
                            chart1.ChartAreas[0].AxisY.Minimum = min;
                        }
                        else
                        {
                            chart1.ChartAreas[0].AxisY.Maximum = 1 < max ? max : 1;
                            chart1.ChartAreas[0].AxisY.Minimum = -1 > min ? min : -1;
                        }
                        

                        chart1.Series[0].Points.AddXY(a.time_stamp, a.Voltage);
                    }
                    catch(System.InvalidOperationException ex)
                    {
                        chart1.ChartAreas[0].AxisY.Maximum = 1 < max ? max : 1;
                        chart1.ChartAreas[0].AxisY.Minimum = -1 > min ? min : -1;
                    }
                    
                    
                }));
                
                
            };

            KEL103StateTracker.Start();

            Console.WriteLine("here");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //enable channel 2 monitor
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            //switch load input on and off

            var address = KEL103Persistance.Configuration.LoadAddress;

            await KEL103Command.SetLoadInputSwitchState(address, !latest_state.input_state);
        }
    }
}
