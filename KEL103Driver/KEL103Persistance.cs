using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KEL103Driver
{
    public class KEL103Configuration
    {
        public string LoadAddressString { get; set; } = "192.168.1.198";
        public string BroadcastAddressString { get; set; } = "192.168.1.255";
        public int BroadcastPort { get; set; } = 18191;
        public int CommandPort { get; set; } = 18190;
        public string SearchMessageString { get; set; } = "find_ka000";
        public int ReadTimeout { get; set; } = 2000;
        public int WriteTimeout { get; set; } = 2000;
        public bool EnableInterfaceSearch { get; set; } = true;
        public bool EnableLoadSearch { get; set; } = true;

        public KEL103Configuration() { }

        [JsonIgnore]
        public IPAddress BroadcastAddress
        {
            get
            {
                return IPAddress.Parse(BroadcastAddressString);
            }

            set
            {
                BroadcastAddressString = value.ToString();
            }
        }

        [JsonIgnore]
        public IPAddress LoadAddress
        {
            get
            {
                return IPAddress.Parse(LoadAddressString);
            }

            set
            {
                LoadAddressString = value.ToString();
            }
        }

        [JsonIgnore]
        public byte[] SearchMessage
        {
            get
            {
                return Encoding.ASCII.GetBytes(SearchMessageString);
            }

        }
    }

    public static class KEL103Persistance
    {
        private static bool needs_init = false;

        private static readonly string persistance_file = System.AppDomain.CurrentDomain.BaseDirectory + "/kel103driver_config.json";
        private static string configuration_serialization = JsonConvert.SerializeObject(new KEL103Configuration());

        private static void Init()
        {
            //read the configuration from file or create a configuration file with the defualt values

            try
            {
                if (File.Exists(persistance_file))
                {
                    var persistance_serialization_holding = File.ReadAllText(persistance_file);

                    JsonConvert.DeserializeObject<KEL103Configuration>(persistance_serialization_holding); //test

                    configuration_serialization = persistance_serialization_holding;

                    return;
                }
            }
            catch(Exception ex)
            {
                //there was some problem reading the file. attempt to create a new one
            }

            try
            {
                if (!File.Exists(persistance_file))
                {
                    File.WriteAllText(persistance_file, configuration_serialization);
                }
            }
            catch(Exception ex) { }
            
        }

        public static KEL103Configuration Configuration
        {
            get
            {
                if (needs_init)
                    Init();

                return JsonConvert.DeserializeObject<KEL103Configuration>(configuration_serialization);
            }

            set
            {
                configuration_serialization = JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.Indented);

                try
                {
                    File.WriteAllText(persistance_file, configuration_serialization);
                }
                catch(Exception ex) { }
            }
        }

        
    }
}
