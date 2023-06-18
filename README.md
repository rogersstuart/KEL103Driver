# KEL103Driver
A library for managing network interactions with the KEL-103 programmable DC load.

The utility for testing the library code is incomplete. It's there to give you a general idea about how to integrate this code into your own application.

1/22/2021 Updates

I haven't used this for a while, but I have been thinking about it from time to time. Turns out, today was the day for some changes. One cool thing that I've thought of is adding KEL103 support to Instrumental. Maybe. Well, this is C# and the KEL103 uses UDP, so how's that going to happen? One easy way to make it work is through a UDP to TCP server application. It would be better if the entire thing was just rewritten in Python but I'm not very good at Python and I've got stuff to do. So, if you want to access the KEL103 as a VISA SOCKET device just use the new server application.

The commands to test it in PyVISA are (you should have already added the resource):<br/>
<br/>
import pyvisa<br/>
rm = pyvisa.ResourceManager()<br/>
rm.list_resources('?*')<br/>
#prints a list of available resources<br/>
my_instr = rm.open_resource('TCPIP0::your_ip_address::5025::SOCKET')<br/>
my_instr.read_termination = '\n'<br/>
my_instr.write_termination = '\n'<br/>
<br/>
#the test codes. it should respond to the IDN and change to CV mode<br/>
print(my_instr.query('*IDN?'))<br/>
my_instr.write(':FUNC CV')<br/>
