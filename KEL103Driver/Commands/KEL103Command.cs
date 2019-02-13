using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KEL103Driver
{
    public static partial class KEL103Command
    {
        private static readonly dynamic[] commands =
        {
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*0*/

            new Func<IPAddress, int, Task>(async (address, location_index) => {await StoreToUnit(address, location_index);}), /*1*/
            new Func<IPAddress, int, Task>(async (address, location_index) => {await RecallToUnit(address, location_index);}), /*2*/

            new Func<IPAddress, Task>(async address => {await SimulateTrigger(address);}), /*3*/

            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*4*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*5*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*6*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*7*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*8*/

            new Func<IPAddress, double, Task>(async (address, target_voltage) => {await SetConstantVoltageTarget(address, target_voltage);}), /*9*/
            new Func<IPAddress, Task<double>>(async address => {return await GetConstantVoltageTarget(address);}), /*10*/

            new Func<IPAddress, Task<double>>(async address => {return await GetMaximumSupportedSystemInputVoltage(address);}), /*11*/
            new Func<IPAddress, Task<double>>(async address => {return await GetMinimumSupportedSystemInputVoltage(address);}), /*12*/

            new Func<IPAddress, double, Task>(async (address, target_current) => {await SetConstantCurrentTarget(address, target_current);}), /*13*/
            new Func<IPAddress, Task<double>>(async address => {return await GetConstantCurrentTarget(address);}), /*14*/

            new Func<IPAddress, Task<double>>(async address => {return await GetMaximumSupportedSystemInputCurrent(address);}), /*15*/
            new Func<IPAddress, Task<double>>(async address => {return await GetMinimumSupportedSystemInputCurrent(address);}), /*16*/

            new Func<IPAddress, double, Task>(async (address, target_resistance) => {await SetConstantResistanceTarget(address, target_resistance);}), /*17*/ 
            new Func<IPAddress, Task<double>>(async address => {return await GetConstantResistanceTarget(address);}), /*18*/ 

            new Func<IPAddress, Task<double>>(async address => {return await GetMaximumSupportedSystemInputResistance(address);}), /*19*/
            new Func<IPAddress, Task<double>>(async address => {return await GetMinimumSupportedSystemInputResistance(address);}), /*20*/

            new Func<IPAddress, double, Task>(async (address, target_power) => {await SetConstantPowerTarget(address, target_power);}), /*21*/  
            new Func<IPAddress, Task<double>>(async address => {return await GetConstantPowerTarget(address);}), /*22*/  

            new Func<IPAddress, Task<double>>(async address => {return await GetMaximumSupportedSystemInputPower(address);}), /*23*/
            new Func<IPAddress, Task<double>>(async address => {return await GetMinimumSupportedSystemInputPower(address);}), /*24*/

            new Func<IPAddress, Task<double>>(async address => {return await MeasureCurrent(address);}), /*25*/
            new Func<IPAddress, Task<double>>(async address => {return await MeasureVoltage(address);}), /*26*/
            new Func<IPAddress, Task<double>>(async address => {return await MeasurePower(address);}), /*27*/

            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*28*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*29*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*30*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*31*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*32*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*33*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*34*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*35*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*36*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*37*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*38*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*39*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*40*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}), /*41*/
            new Func<IPAddress, Task<string>>(async address => {return await Identify(address);}),  /*42*/

            new Func<IPAddress, int, Task>(async (address, mode) => {await SetSystemMode(address, mode);}), /*43*/
            new Func<IPAddress, Task<int>>(async address => {return await GetSystemMode(address);}), /*44*/
        };

        public static readonly int IDENTIFY = 0; //get product information

        public static readonly int STORE = 1; //store to unit
        public static readonly int RECALL = 2; //recall from unit storage

        public static readonly int TRIGGER = 3; //simulate an external trigger; only valid in pulse and flip mode

        public static readonly int SET_SYSTEM_PARAMETER = 4;
        public static readonly int GET_SYSTEM_PARAMETER = 5;

        public static readonly int GET_SYSTEM_STATUS = 6;

        public static readonly int SET_LOAD_INPUT_SWITCH_STATE = 7; //turn the load on or off
        public static readonly int GET_LOAD_INPUT_SWITCH_STATE = 8; //is the input loaded down?

        public static readonly int SET_CV_VOLTAGE = 9; //set the constant voltage mode voltage
        public static readonly int GET_CV_VOLTAGE = 10; //get the constant voltage mode voltage

        public static readonly int VOLTAGE_UPPER = 11; //get the maxium voltage supported by the device
        public static readonly int VOLTAGE_LOWER = 12; //get the minimum voltage supported by the device

        public static readonly int SET_CC_CURRENT = 13; //set the constant current mode current
        public static readonly int GET_CC_CURRENT = 14; //get the constant current mode current

        public static readonly int CURRENT_UPPER = 15;
        public static readonly int CURRENT_LOWER = 16;

        public static readonly int SET_CR_RESISTANCE = 17; //set the constant resistance mode resistance
        public static readonly int GET_CR_RESISTANCE = 18; //get the constant resistance mode resistance

        public static readonly int RESISTANCE_UPPER = 19;
        public static readonly int RESISTANCE_LOWER = 20;

        public static readonly int SET_CW_POWER = 21; //set the constant wattage mode wattage
        public static readonly int GET_CW_POWER = 22; //get the constant wattage mode wattage

        public static readonly int POWER_UPPER = 23;
        public static readonly int POWER_LOWER = 24;

        public static readonly int MEASURE_CURRENT = 25;
        public static readonly int MEASURE_VOLTAGE = 26;
        public static readonly int MEASURE_POWER = 27;

        public static readonly int LIST = 28; //output all steps in order

        public static readonly int RCL_LIST = 29;

        public static readonly int OCP = 30;

        public static readonly int SET_RCL_OCP = 31;
        public static readonly int GET_RCL_OCP = 32;

        public static readonly int OPP = 33;

        public static readonly int SET_RCL_OPP = 34;
        public static readonly int GET_RCL_OPP = 35;

        public static readonly int BATTERY = 36;

        public static readonly int SET_RCL_BATTERY = 37;
        public static readonly int GET_RCL_BATTERY = 38;

        public static readonly int BATTERY_TIME = 39;

        public static readonly int BATTERY_CAPACITY = 40;

        public static readonly int SET_DYNAMIC = 41;
        public static readonly int GET_DYNAMIC = 42;

        public static readonly int SET_OPERATING_MODE = 43;
        public static readonly int GET_OPERATING_MODE = 44;

        public static dynamic GetCommandFunc(int command_index)
        {
            return commands[command_index];
        }
    }
}
