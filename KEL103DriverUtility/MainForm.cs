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
        int max_chart_points = 100;

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

        private int currentMode = -1;  // Track the current mode

        public MainForm()
        {
            InitializeComponent();

            // Debug: Check if buttons exist
            System.Diagnostics.Debug.WriteLine($"Constructor - btnCCGet exists: {btnCCGet != null}");
            System.Diagnostics.Debug.WriteLine($"Constructor - btnCCSet exists: {btnCCSet != null}");
            
            // Add debugging for textbox names
            System.Diagnostics.Debug.WriteLine("=== TEXTBOX DEBUGGING ===");
            foreach (Control c in this.Controls)
            {
                if (c is TabControl)
                {
                    foreach (TabPage page in ((TabControl)c).TabPages)
                    {
                        System.Diagnostics.Debug.WriteLine($"Tab: {page.Name} - {page.Text}");
                        foreach (Control ctrl in page.Controls)
                        {
                            if (ctrl is TextBox)
                            {
                                System.Diagnostics.Debug.WriteLine($"  TextBox: Name={ctrl.Name}, Text={ctrl.Text}");
                            }
                            else if (ctrl is Button)
                            {
                                System.Diagnostics.Debug.WriteLine($"  Button: Name={ctrl.Name}, Text={ctrl.Text}");
                            }
                        }
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("=== END TEXTBOX DEBUGGING ===");

            // Wire up the button click events
            btnCCGet.Click += btnCCGet_Click;
            btnCCSet.Click += btnCCSet_Click;
            btnCVGet.Click += btnCVGet_Click;
            btnCVSet.Click += btnCVSet_Click;
            btnCRGet.Click += btnCRGet_Click;
            btnCRSet.Click += btnCRSet_Click;
            btnCWGet.Click += btnCWGet_Click;
            btnCWSet.Click += btnCWSet_Click;

            // Add tab selection handler to prevent switching to disabled tabs
            tabControl1.Selecting += tabControl1_Selecting;

            // Debug: Confirm events are wired
            System.Diagnostics.Debug.WriteLine($"CC Get button has {btnCCGet.GetType().GetEvents().Length} events");

            foreach (Control c in Controls)
                c.Enabled = false;

            menuStrip1.Enabled = true;

            // Disable all tabs initially
            tabControl1.Enabled = false;

            charts = new Chart[]{chart1, chart2 };

            text_boxes = new TextBox[][] { 
                new TextBox[] {textBox1, textBox2, textBox3, textBox7}, // Channel 1
                new TextBox[] {textBox4, textBox5, textBox6, textBox8}  // Channel 2
            };

            for(int i = 0; i < 2; i++)
            {
                Chart c = charts[i];
                
                // Set a fixed time format that won't change
                c.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
                c.Series[0].XValueType = ChartValueType.DateTime;
                
                // Add consistent margins to create a border effect
                c.ChartAreas[0].Position = new ElementPosition(5, 5, 90, 90);
                c.ChartAreas[0].InnerPlotPosition = new ElementPosition(10, 5, 85, 85);
                
                // Set minimum interval to prevent too many labels
                c.ChartAreas[0].AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
                
                // Enable anti-aliasing for smoother appearance
                c.AntiAliasing = AntiAliasingStyles.All;
                c.TextAntiAliasingQuality = TextAntiAliasingQuality.High;
                
                // Set border style
                c.BorderlineColor = Color.Gray;
                c.BorderlineWidth = 1;
                c.BorderlineDashStyle = ChartDashStyle.Solid;
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

        private async Task RefreshValue(Func<Task<double>> getValue, TextBox display, string type)
        {
            try
            {
                using (var client = await KEL103StateTracker.CheckoutClientAsync())
                {
                    var value = await getValue();
                    KEL103StateTracker.CheckinClient();

                    BeginInvoke((MethodInvoker)(() => {
                        if (display != null)
                        {
                            display.Text = KEL103Tools.FormatString(value);
                            display.BackColor = Color.LightYellow;
                            display.Refresh();

                            Task.Delay(500).ContinueWith(_ => {
                                if (!IsDisposed)
                                {
                                    BeginInvoke((MethodInvoker)(() => {
                                        if (display != null && !display.IsDisposed)
                                        {
                                            display.BackColor = SystemColors.Window;
                                        }
                                    }));
                                }
                            });
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Failed to refresh {type}: {ex.Message}");
                throw;
            }
        }

        private async Task RefreshCCValue()
        {
            try
            {
                using (var client = await KEL103StateTracker.CheckoutClientAsync())
                {
                    var value = await KEL103Command.GetConstantCurrentTarget(client);
                    KEL103StateTracker.CheckinClient();

                    BeginInvoke((MethodInvoker)(() => {
                        if (txtCCCurrent != null)
                        {
                            txtCCCurrent.Text = KEL103Tools.FormatString(value);
                            txtCCCurrent.BackColor = Color.LightYellow;
                            txtCCCurrent.Refresh();

                            Task.Delay(500).ContinueWith(_ => {
                                if (!IsDisposed)
                                {
                                    BeginInvoke((MethodInvoker)(() => {
                                        if (txtCCCurrent != null && !txtCCCurrent.IsDisposed)
                                        {
                                            txtCCCurrent.BackColor = SystemColors.Window;
                                        }
                                    }));
                                }
                            });
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Failed to refresh current: {ex.Message}");
                throw;
            }
        }

        private async Task RefreshCVValue()
        {
            try
            {
                using (var client = await KEL103StateTracker.CheckoutClientAsync())
                {
                    var value = await KEL103Command.GetConstantVoltageTarget(client);
                    KEL103StateTracker.CheckinClient();

                    BeginInvoke((MethodInvoker)(() => {
                        if (txtCVCurrent != null)
                        {
                            txtCVCurrent.Text = KEL103Tools.FormatString(value);
                            txtCVCurrent.BackColor = Color.LightYellow;
                            txtCVCurrent.Refresh();

                            Task.Delay(500).ContinueWith(_ => {
                                if (!IsDisposed)
                                {
                                    BeginInvoke((MethodInvoker)(() => {
                                        if (txtCVCurrent != null && !txtCVCurrent.IsDisposed)
                                        {
                                            txtCVCurrent.BackColor = SystemColors.Window;
                                        }
                                    }));
                                }
                            });
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Failed to refresh voltage: {ex.Message}");
                throw;
            }
        }

        private async Task RefreshCRValue()
        {
            try
            {
                using (var client = await KEL103StateTracker.CheckoutClientAsync())
                {
                    var value = await KEL103Command.GetConstantResistanceTarget(client);
                    KEL103StateTracker.CheckinClient();

                    BeginInvoke((MethodInvoker)(() => {
                        if (txtCRCurrent != null)
                        {
                            txtCRCurrent.Text = KEL103Tools.FormatString(value);
                            txtCRCurrent.BackColor = Color.LightYellow;
                            txtCRCurrent.Refresh();

                            Task.Delay(500).ContinueWith(_ => {
                                if (!IsDisposed)
                                {
                                    BeginInvoke((MethodInvoker)(() => {
                                        if (txtCRCurrent != null && !txtCRCurrent.IsDisposed)
                                        {
                                            txtCRCurrent.BackColor = SystemColors.Window;
                                        }
                                    }));
                                }
                            });
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Failed to refresh resistance: {ex.Message}");
                throw;
            }
        }

        private async Task RefreshCWValue()
        {
            try
            {
                using (var client = await KEL103StateTracker.CheckoutClientAsync())
                {
                    var value = await KEL103Command.GetConstantPowerTarget(client);
                    KEL103StateTracker.CheckinClient();

                    BeginInvoke((MethodInvoker)(() => {
                        if (txtCWCurrent != null)
                        {
                            txtCWCurrent.Text = KEL103Tools.FormatString(value);
                            txtCWCurrent.BackColor = Color.LightYellow;
                            txtCWCurrent.Refresh();

                            Task.Delay(500).ContinueWith(_ => {
                                if (!IsDisposed)
                                {
                                    BeginInvoke((MethodInvoker)(() => {
                                        if (txtCWCurrent != null && !txtCWCurrent.IsDisposed)
                                        {
                                            txtCWCurrent.BackColor = SystemColors.Window;
                                        }
                                    }));
                                }
                            });
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Failed to refresh power: {ex.Message}");
                throw;
            }
        }

        // Add this event handler to prevent tab switching to non-active modes
        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            // Allow switching only to the tab that matches the current mode
            if (currentMode >= 0 && currentMode < 5)
            {
                TabPage allowedTab = GetTabForMode(currentMode);
                if (e.TabPage != allowedTab && allowedTab != null)
                {
                    e.Cancel = true;
                }
            }
        }

        // Helper method to get the tab for a given mode
        private TabPage GetTabForMode(int mode)
        {
            switch (mode)
            {
                case 0: return cvmode;
                case 1: return ccmode;
                case 2: return crmode;
                case 3: return cwmode;
                case 4: return ccmode; // SHORT mode uses CC tab
                default: return null;
            }
        }

        // Add this method to manage tab state based on mode
        private void UpdateTabsForMode(int mode)
        {
            // Store the current mode
            currentMode = mode;

            // First, disable all controls in all tabs
            foreach (TabPage tab in tabControl1.TabPages)
            {
                foreach (Control control in tab.Controls)
                {
                    control.Enabled = false;
                }
            }

            // Select and enable controls in the appropriate tab based on mode
            TabPage targetTab = null;
            switch (mode)
            {
                case 0: // CONSTANT_VOLTAGE_MODE
                    targetTab = cvmode;
                    break;

                case 1: // CONSTANT_CURRENT_MODE
                    targetTab = ccmode;
                    break;

                case 2: // CONSTANT_RESISTANCE_MODE
                    targetTab = crmode;
                    break;

                case 3: // CONSTANT_POWER_MODE
                    targetTab = cwmode;
                    break;

                case 4: // SHORT_MODE
                    targetTab = ccmode;
                    // Don't enable controls for SHORT mode
                    break;
            }

            if (targetTab != null)
            {
                tabControl1.SelectedTab = targetTab;
                
                // Enable controls only for non-SHORT modes
                if (mode < 4)
                {
                    foreach (Control control in targetTab.Controls)
                    {
                        control.Enabled = true;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Tabs updated for mode {mode} ({KEL103Command.mode_strings[mode]})");
        }

        private async void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //start the state tracker
            int systemMode = -1;  // Store mode outside Task.Run
            
            await Task.Run(async () => {

                KEL103StateTracker.NewKEL103StateAvailable += a => onNewState_refresh(a);

                KEL103StateTracker.Start();

                while(!KEL103StateTracker.IsInitComplete)
                    await Task.Delay(1);

                // Fix: Rename the variable to avoid conflict
                using (var client = await KEL103StateTracker.CheckoutClientAsync())
                {
                    systemMode = await KEL103Command.GetSystemMode(client);
                    KEL103StateTracker.CheckinClient();

                    Invoke((MethodInvoker)(() =>
                    {
                        comboBox1.SelectedIndex = systemMode;
                        comboBox2.SelectedIndex = 1;
                        comboBox3.SelectedIndex = 0;

                        foreach (Control c in Controls)
                            c.Enabled = true;

                        // Enable the tab control
                        tabControl1.Enabled = true;

                        // Update tabs based on current mode
                        UpdateTabsForMode(systemMode);
                    }));
                }
            });

            // Wait for connection to stabilize before refreshing values
            await Task.Delay(500);
            
            // Only refresh the appropriate tab based on current mode using the stored systemMode
            System.Diagnostics.Debug.WriteLine($"About to refresh mode {systemMode} value");
            if (systemMode >= 0 && systemMode < 4) // Don't refresh for SHORT mode
            {
                await RefreshCurrentModeValue(systemMode);
            }
        }

        // Add method to refresh only the current mode's value
        private async Task RefreshCurrentModeValue(int mode)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"RefreshCurrentModeValue called for mode {mode}");
                
                switch (mode)
                {
                    case 0: // CONSTANT_VOLTAGE_MODE
                        System.Diagnostics.Debug.WriteLine("Refreshing CV value");
                        await RefreshCVValue();
                        break;
                    case 1: // CONSTANT_CURRENT_MODE
                        System.Diagnostics.Debug.WriteLine("Refreshing CC value");
                        await RefreshCCValue();
                        break;
                    case 2: // CONSTANT_RESISTANCE_MODE
                        System.Diagnostics.Debug.WriteLine("Refreshing CR value");
                        await RefreshCRValue();
                        break;
                    case 3: // CONSTANT_POWER_MODE
                        System.Diagnostics.Debug.WriteLine("Refreshing CW value");
                        await RefreshCWValue();
                        break;
                    // No refresh for SHORT mode
                }
                
                System.Diagnostics.Debug.WriteLine($"RefreshCurrentModeValue completed for mode {mode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to refresh mode {mode}: {ex.Message}");
            }
        }

        private void onNewState_refresh(KEL103State a)
        {
            new_kel103_states.Enqueue(a);
            Console.WriteLine(new_kel103_states.Count());
        }

        private void refreshTask()
        {
            // Process all pending states at once
            var statesToProcess = new List<KEL103State>();
            while (new_kel103_states.TryDequeue(out KEL103State result))
            {
                statesToProcess.Add(result);
                kel103_states.Enqueue(result);
            }

            if (statesToProcess.Count == 0)
                return;

            // Keep queue size manageable
            while (kel103_states.Count > max_chart_points)
                kel103_states.Dequeue();

            var latestState = kel103_states.Last();

            // Prepare all chart data on background thread
            var chartUpdates = new List<Action>();
            
            for (int i = 0; i < 2; i++)
            {
                var chartIndex = i;
                var timestampQueue = chart_values[chartIndex][0] as Queue<DateTime>;
                var valueQueue = chart_values[chartIndex][1] as Queue<double>;

                // Handle invalidation
                if (channel_value_type_invalid[chartIndex])
                {
                    valueQueue.Clear();
                    timestampQueue.Clear();

                    var valueType = channel_value_type[chartIndex];
                    foreach (var state in kel103_states)
                    {
                        double value;
                        switch (valueType)
                        {
                            case 0: value = state.Voltage; break;
                            case 1: value = state.Current; break;
                            case 2: value = state.Power; break;
                            default: throw new Exception("Invalid value type");
                        }
                        valueQueue.Enqueue(value);
                        timestampQueue.Enqueue(state.TimeStamp);
                    }
                    channel_value_type_invalid[chartIndex] = false;
                }
                else
                {
                    // Add new states
                    foreach (var state in statesToProcess)
                    {
                        timestampQueue.Enqueue(state.TimeStamp);
                        switch (channel_value_type[chartIndex])
                        {
                            case 0: valueQueue.Enqueue(state.Voltage); break;
                            case 1: valueQueue.Enqueue(state.Current); break;
                            case 2: valueQueue.Enqueue(state.Power); break;
                        }
                    }
                }

                // Trim queues
                while (timestampQueue.Count > max_chart_points)
                {
                    timestampQueue.Dequeue();
                    valueQueue.Dequeue();
                }

                // Pre-calculate all values
                var max = valueQueue.Max();
                var min = valueQueue.Min();
                var avg = valueQueue.Average();
                var last = valueQueue.Last();
                var range = max - min;

                // Store update action
                chartUpdates.Add(() => UpdateChart(chartIndex, timestampQueue, valueQueue, max, min, avg, last, range));
            }

            // Single UI update for everything
            BeginInvoke((MethodInvoker)(() =>
            {
                // Update button state
                button2.BackColor = latestState.InputState ? Color.Red : Color.Green;
                button2.Text = latestState.InputState ? "Load Active" : "Load Inactive";
                toolStripStatusLabel1.Text = latestState.ValueAquisitionTimespan.ToString();

                // Update all charts
                foreach (var update in chartUpdates)
                {
                    update();
                }
            }));
        }

        private void UpdateChart(int index, Queue<DateTime> timestamps, Queue<double> values, 
    double max, double min, double avg, double last, double range)
{
    var chart = charts[index];
    var textBoxes = text_boxes[index];

    // Update text boxes
    textBoxes[0].Text = KEL103Tools.FormatString(max);
    textBoxes[1].Text = KEL103Tools.FormatString(min);
    textBoxes[2].Text = KEL103Tools.FormatString(avg);
    if (textBoxes.Length > 3)
        textBoxes[3].Text = KEL103Tools.FormatString(last);

    // Update chart
    chart.Series[0].Points.DataBindXY(timestamps, values);

    var yAxis = chart.ChartAreas[0].AxisY;
    int decimalPlaces = DetermineDecimalPlaces(max, min, range);
    yAxis.LabelStyle.Format = $"F{decimalPlaces}";

    if (range > 0)
    {
        yAxis.Minimum = min;
        yAxis.Maximum = max;
        
        var interval = CalculateNiceInterval(range);
        yAxis.Interval = interval;
        
        var minTick = Math.Floor(min / interval) * interval;
        var maxTick = Math.Ceiling(max / interval) * interval;
        
        if (min - minTick < range * 0.02)
            yAxis.Minimum = minTick;
        if (maxTick - max < range * 0.02)
            yAxis.Maximum = maxTick;
    }
    else if (max != 0)
    {
        var absValue = Math.Abs(max);
        var interval = CalculateNiceInterval(absValue * 0.1);
        
        yAxis.Minimum = Math.Floor(max / interval) * interval;
        yAxis.Maximum = Math.Ceiling(max / interval) * interval;
        
        if (yAxis.Minimum == yAxis.Maximum)
        {
            yAxis.Minimum -= interval;
            yAxis.Maximum += interval;
        }
        
        yAxis.Interval = interval;
    }
    else
    {
        yAxis.Minimum = -1;
        yAxis.Maximum = 1;
        yAxis.Interval = 0.5;
    }
}

private int DetermineDecimalPlaces(double max, double min, double range)
{
    if (Math.Abs(max) >= 100 || Math.Abs(min) >= 100) return 0;
    if (Math.Abs(max) >= 10 || Math.Abs(min) >= 10) return 1;
    if (Math.Abs(max) < 0.1 && Math.Abs(min) < 0.1 && range < 0.01) return 4;
    if (Math.Abs(max) < 1 && Math.Abs(min) < 1 && range < 0.1) return 3;
    return 2;
}

private async void button2_Click(object sender, EventArgs e)
{
    //switch load input on and off
    try
    {
        // Fix: Use CheckoutClientAsync instead of CheckoutClient
        using (var client = await KEL103StateTracker.CheckoutClientAsync())
        {
            await KEL103Command.SetLoadInputSwitchState(client, !kel103_states.Last().InputState);
            KEL103StateTracker.CheckinClient();
        }
    }
    catch (Exception ex)
    {
        await ShowErrorAsync($"Failed to switch input state: {ex.Message}");
    }
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
        try
        {
            // Disable mode switching controls
            button1.Enabled = false;
            comboBox1.Enabled = false;
            
            // Disable all tab contents during mode switch
            foreach (TabPage tab in tabControl1.TabPages)
            {
                foreach (Control control in tab.Controls)
                {
                    control.Enabled = false;
                }
            }
            
            toolStripStatusLabel1.Text = $"Switching to {KEL103Command.mode_strings[selected_index]}...";

            // Fix: Use CheckoutClientAsync instead of CheckoutClient
            using (var client = await KEL103StateTracker.CheckoutClientAsync())
            {
                await KEL103Command.SetSystemMode(client, selected_index);
                KEL103StateTracker.CheckinClient();
            }

            // Wait for mode to settle
            await Task.Delay(1000);

            // Read back the mode to confirm
            using (var client = await KEL103StateTracker.CheckoutClientAsync())
            {
                var actualMode = await KEL103Command.GetSystemMode(client);
                KEL103StateTracker.CheckinClient();

                if (actualMode != selected_index)
                {
                    await ShowErrorAsync($"Mode change failed. Expected {KEL103Command.mode_strings[selected_index]} but device is in {KEL103Command.mode_strings[actualMode]}");
                    comboBox1.SelectedIndex = actualMode;
                    selected_index = actualMode;
                }
            }

            // Update UI based on new mode
            UpdateTabsForMode(selected_index);
            
            // Refresh the value for the new mode (except SHORT)
            if (selected_index < 4)
            {
                await RefreshCurrentModeValue(selected_index);
            }

            toolStripStatusLabel1.Text = $"Mode set to {KEL103Command.mode_strings[selected_index]}";
        }
        catch (Exception ex)
        {
            await ShowErrorAsync($"Failed to set mode: {ex.Message}");
            // Re-enable the appropriate tab on error
            UpdateTabsForMode(currentMode);
        }
        finally
        {
            // Re-enable controls
            button1.Enabled = true;
            comboBox1.Enabled = true;
        }
    }
}

private async Task ShowErrorAsync(string message)
{
    try 
    {
        if (InvokeRequired)
        {
            await Task.Factory.FromAsync(
                BeginInvoke(new Action(() => ShowErrorMessage(message))),
                EndInvoke);
            return;
        }

        ShowErrorMessage(message);
    }
    catch (Exception ex)
    {
        // Fallback error handling if the UI invocation fails
        System.Diagnostics.Debug.WriteLine($"Error showing error message: {ex.Message}");
        System.Diagnostics.Debug.WriteLine($"Original error: {message}");
    }
}

private void ShowErrorMessage(string message)
{
    // Update status strip
    toolStripStatusLabel1.Text = message;
    
    // Show message box
    MessageBox.Show(this, 
        message,
        "Error",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
}

        // Add these event handlers for Constant Current tab
        private async void btnCCSet_Click(object sender, EventArgs e)
        {
            try
            {
                string inputText = txtCCTarget.Text?.Trim();
                System.Diagnostics.Debug.WriteLine($"CC Set button clicked with input: '{inputText}'");
                
                if (!string.IsNullOrEmpty(inputText) && 
                    double.TryParse(inputText, System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out double targetCurrent))
                {
                    System.Diagnostics.Debug.WriteLine($"Parsed current value: {targetCurrent}");
                    
                    btnCCSet.Enabled = false;
                    toolStripStatusLabel1.Text = "Setting constant current...";
                    
                    using (var client = await KEL103StateTracker.CheckoutClientAsync())
                    {
                        await KEL103Command.SetConstantCurrentTarget(client, targetCurrent);
                        KEL103StateTracker.CheckinClient();
                        
                        // Read back the value to confirm
                        await RefreshCCValue();
                        
                        toolStripStatusLabel1.Text = $"Set constant current target to {targetCurrent}A";
                    }
                    btnCCSet.Enabled = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse input: '{inputText}'");
                    await ShowErrorAsync($"Please enter a valid numeric value for current. Input was: '{inputText}'");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in btnCCSet_Click: {ex}");
                await ShowErrorAsync($"Failed to set constant current: {ex.Message}");
                btnCCSet.Enabled = true;
            }
        }

        private async void btnCCGet_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("btnCCGet_Click called");
                
                // Show loading state
                toolStripStatusLabel1.Text = "Retrieving constant current setting...";
                btnCCGet.Enabled = false;
                
                await RefreshCCValue();
                
                // Show success message
                toolStripStatusLabel1.Text = "Successfully retrieved constant current setting";
                System.Diagnostics.Debug.WriteLine("btnCCGet_Click completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in btnCCGet_Click: {ex}");
                await ShowErrorAsync($"Failed to get constant current: {ex.Message}");
            }
            finally
            {
                btnCCGet.Enabled = true;
            }
        }

        // Add these event handlers for Constant Voltage tab
        private async void btnCVSet_Click(object sender, EventArgs e)
        {
            try
            {
                // Store current text for debugging
                string inputText = txtCVTarget.Text?.Trim();
                System.Diagnostics.Debug.WriteLine($"CV Set button clicked with input: '{inputText}'");
                
                // Use invariant culture for parsing to handle different decimal separators
                if (!string.IsNullOrEmpty(inputText) && 
                    double.TryParse(inputText, System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out double targetVoltage))
                {
                    System.Diagnostics.Debug.WriteLine($"Parsed voltage value: {targetVoltage}");
                    
                    btnCVSet.Enabled = false;
                    toolStripStatusLabel1.Text = "Setting constant voltage...";
                    
                    using (var client = await KEL103StateTracker.CheckoutClientAsync())
                    {
                        await KEL103Command.SetConstantVoltageTarget(client, targetVoltage);
                        KEL103StateTracker.CheckinClient();
                        
                        // Read back the value to confirm
                        await RefreshCVValue();
                        
                        toolStripStatusLabel1.Text = $"Set constant voltage target to {targetVoltage}V";
                    }
                    btnCVSet.Enabled = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse input: '{inputText}'");
                    await ShowErrorAsync($"Please enter a valid numeric value for voltage. Input was: '{inputText}'");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in btnCVSet_Click: {ex}");
                await ShowErrorAsync($"Failed to set constant voltage: {ex.Message}");
                btnCVSet.Enabled = true;
            }
        }

        private async void btnCVGet_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("btnCVGet_Click called");
                
                // Show loading state
                toolStripStatusLabel1.Text = "Retrieving constant voltage setting...";
                btnCVGet.Enabled = false;
                
                await RefreshCVValue();
                
                // Show success message
                toolStripStatusLabel1.Text = "Successfully retrieved constant voltage setting";
                System.Diagnostics.Debug.WriteLine("btnCVGet_Click completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in btnCVGet_Click: {ex}");
                await ShowErrorAsync($"Failed to get constant voltage: {ex.Message}");
            }
            finally
            {
                btnCVGet.Enabled = true;
            }
        }

        // Add these event handlers for Constant Resistance tab
        private async void btnCRSet_Click(object sender, EventArgs e)
        {
            try
            {
                // Store current text for debugging
                string inputText = txtCRTarget.Text?.Trim();
                System.Diagnostics.Debug.WriteLine($"CR Set button clicked with input: '{inputText}'");
                
                // Use invariant culture for parsing to handle different decimal separators
                if (!string.IsNullOrEmpty(inputText) && 
                    double.TryParse(inputText, System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out double targetResistance))
                {
                    System.Diagnostics.Debug.WriteLine($"Parsed resistance value: {targetResistance}");
                    
                    btnCRSet.Enabled = false;
                    toolStripStatusLabel1.Text = "Setting constant resistance...";
                    
                    using (var client = await KEL103StateTracker.CheckoutClientAsync())
                    {
                        await KEL103Command.SetConstantResistanceTarget(client, targetResistance);
                        KEL103StateTracker.CheckinClient();
                        
                        // Read back the value to confirm
                        await RefreshCRValue();
                        
                        toolStripStatusLabel1.Text = $"Set constant resistance target to {targetResistance}Ω";
                    }
                    btnCRSet.Enabled = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse input: '{inputText}'");
                    await ShowErrorAsync($"Please enter a valid numeric value for resistance. Input was: '{inputText}'");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in btnCRSet_Click: {ex}");
                await ShowErrorAsync($"Failed to set constant resistance: {ex.Message}");
                btnCRSet.Enabled = true;
            }
        }

        private async void btnCRGet_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("btnCRGet_Click called");
                
                // Show loading state
                toolStripStatusLabel1.Text = "Retrieving constant resistance setting...";
                btnCRGet.Enabled = false;
                
                await RefreshCRValue();
                
                // Show success message
                toolStripStatusLabel1.Text = "Successfully retrieved constant resistance setting";
                System.Diagnostics.Debug.WriteLine("btnCRGet_Click completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in btnCRGet_Click: {ex}");
                await ShowErrorAsync($"Failed to get constant resistance: {ex.Message}");
            }
            finally
            {
                btnCRGet.Enabled = true;
            }
        }

        // Add these event handlers for Constant Power tab
        private async void btnCWSet_Click(object sender, EventArgs e)
        {
            try
            {
                // Debug all textboxes in the current tab
                System.Diagnostics.Debug.WriteLine("=== CW SET CLICK - CHECKING TEXTBOXES ===");
                var currentTab = tabControl1.SelectedTab;
                System.Diagnostics.Debug.WriteLine($"Current tab: {currentTab?.Name} - {currentTab?.Text}");
                
                // Find the actual input textbox
                TextBox targetTextBox = null;
                
                // First check if txtCWTarget is working correctly
                if (txtCWTarget != null && txtCWTarget != txtCWCurrent)
                {
                    System.Diagnostics.Debug.WriteLine($"Using txtCWTarget: Name={txtCWTarget.Name}, Text='{txtCWTarget.Text}'");
                    targetTextBox = txtCWTarget;
                }
                else
                {
                    // Search recursively through all controls to find input textboxes
                    System.Diagnostics.Debug.WriteLine("Looking for target textbox in control hierarchy...");
                    targetTextBox = FindTargetTextBox(currentTab);
                    
                    if (targetTextBox != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found target textbox: {targetTextBox.Name}, Text='{targetTextBox.Text}'");
                    }
                }
                
                // If we can't find a proper target textbox, create a fixed value as fallback
                string inputText = "0";
                if (targetTextBox != null)
                {
                    inputText = targetTextBox.Text?.Trim() ?? "";
                }
                
                System.Diagnostics.Debug.WriteLine($"CW Set button clicked with input: '{inputText}'");
                
                // Use invariant culture for parsing to handle different decimal separators
                if (!string.IsNullOrEmpty(inputText) && 
                    double.TryParse(inputText, System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out double targetPower))
                {
                    System.Diagnostics.Debug.WriteLine($"Parsed power value: {targetPower}");
                    
                    btnCWSet.Enabled = false;
                    toolStripStatusLabel1.Text = "Setting constant power...";
                    
                    using (var client = await KEL103StateTracker.CheckoutClientAsync())
                    {
                        await KEL103Command.SetConstantPowerTarget(client, targetPower);
                        KEL103StateTracker.CheckinClient();
                        
                        // Read back the value to confirm
                        await RefreshCWValue();
                        
                        toolStripStatusLabel1.Text = $"Set constant power target to {targetPower}W";
                    }
                    btnCWSet.Enabled = true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to parse input: '{inputText}'");
                    await ShowErrorAsync($"Please enter a valid numeric value for power. Input was: '{inputText}'");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in btnCWSet_Click: {ex}");
                await ShowErrorAsync($"Failed to set constant power: {ex.Message}");
                btnCWSet.Enabled = true;
            }
        }

        private async void btnCWGet_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("btnCWGet_Click called");
                
                // Show loading state
                toolStripStatusLabel1.Text = "Retrieving constant power setting...";
                btnCWGet.Enabled = false;
                
                await RefreshCWValue();
                
                // Show success message
                toolStripStatusLabel1.Text = "Successfully retrieved constant power setting";
                System.Diagnostics.Debug.WriteLine("btnCWGet_Click completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in btnCWGet_Click: {ex}");
                await ShowErrorAsync($"Failed to get constant power: {ex.Message}");
            }
            finally
            {
                btnCWGet.Enabled = true;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            System.Diagnostics.Debug.WriteLine("=== FORM SHOWN - CHECKING CONTROLS ===");
            System.Diagnostics.Debug.WriteLine($"tabControl1 exists: {tabControl1 != null}");
            
            if (tabControl1 != null)
            {
                foreach (TabPage page in tabControl1.TabPages)
                {
                    System.Diagnostics.Debug.WriteLine($"\nTab: {page.Name} - {page.Text}");
                    foreach (Control ctrl in page.Controls)
                    {
                        if (ctrl is TextBox || ctrl is Button)
                        {
                            System.Diagnostics.Debug.WriteLine($"  {ctrl.GetType().Name}: Name={ctrl.Name}, Text='{ctrl.Text}'");
                        }
                    }
                }
            }
            
            // Check specific controls
            System.Diagnostics.Debug.WriteLine($"\nSpecific control checks:");
            System.Diagnostics.Debug.WriteLine($"txtCWTarget exists: {txtCWTarget != null}");
            System.Diagnostics.Debug.WriteLine($"txtCWCurrent exists: {txtCWCurrent != null}");
            System.Diagnostics.Debug.WriteLine($"Are they the same object? {object.ReferenceEquals(txtCWTarget, txtCWCurrent)}");
            
            System.Diagnostics.Debug.WriteLine("=== END FORM SHOWN ===");
        }
        
        // Helper method to recursively find a textbox that looks like a target input
        private TextBox FindTargetTextBox(Control parent)
        {
            // First search direct children
            foreach (Control ctrl in parent.Controls)
            {
                // Case 1: It's a textbox with "Target" in its name
                if (ctrl is TextBox tb && tb.Name.Contains("Target"))
                {
                    System.Diagnostics.Debug.WriteLine($"Found target by name: {tb.Name}");
                    return tb;
                }
                
                // Case 2: It's an empty textbox (likely for input)
                if (ctrl is TextBox tb2 && string.IsNullOrEmpty(tb2.Text?.Trim()) && 
                    tb2 != txtCWCurrent && tb2 != txtCVCurrent && tb2 != txtCCCurrent && tb2 != txtCRCurrent)
                {
                    System.Diagnostics.Debug.WriteLine($"Found empty textbox: {tb2.Name}");
                    return tb2;
                }
            }
            
            // If not found, recursively search in container controls
            foreach (Control container in parent.Controls)
            {
                if (container.HasChildren)
                {
                    System.Diagnostics.Debug.WriteLine($"Searching container: {container.GetType().Name} - {container.Name}");
                    
                    // For TableLayoutPanel, check its contents
                    if (container is TableLayoutPanel tlp)
                    {
                        for (int row = 0; row < tlp.RowCount; row++)
                        {
                            for (int col = 0; col < tlp.ColumnCount; col++)
                            {
                                var cellControls = tlp.GetControlFromPosition(col, row);
                                if (cellControls is TextBox tb && tb != txtCWCurrent)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Found textbox in TableLayoutPanel: {tb.Name}, Text='{tb.Text}'");
                                    return tb;
                                }
                            }
                        }
                    }
                    
                    var found = FindTargetTextBox(container);
                    if (found != null)
                        return found;
                }
            }
            
            return null;
        }

private double CalculateNiceInterval(double range)
{
    if (range <= 0)
        return 1;

    // Find the magnitude of the range
    var magnitude = Math.Pow(10, Math.Floor(Math.Log10(range)));
    var normalized = range / magnitude;
    
    // Pick a nice interval that gives us 4-8 ticks
    double niceInterval;
    if (normalized <= 1.2)
        niceInterval = 0.2;
    else if (normalized <= 2.5)
        niceInterval = 0.5;
    else if (normalized <= 5)
        niceInterval = 1;
    else
        niceInterval = 2;
    
    return niceInterval * magnitude;
}
    }
}
