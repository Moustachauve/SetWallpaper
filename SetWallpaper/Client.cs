using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SetWallpaper
{
    public class Client
    {


        public static IPAddress FindServer(int port)
        {
            foreach (NetworkInterface netwIntrf in NetworkInterface.GetAllNetworkInterfaces())
            {
                //if the current interface doesn't have an IP, skip it
                if (!(netwIntrf.GetIPProperties().GatewayAddresses.Count > 0))
                {
                    continue;
                }

                //get current IP Address(es)
                foreach (UnicastIPAddressInformation uniIpInfo in netwIntrf.GetIPProperties().UnicastAddresses)
                {
                    if (uniIpInfo.Address.IsIPv6LinkLocal)
                        continue;

                    //get the subnet mask and the IP address as bytes
                    byte[] subnetMask = uniIpInfo.IPv4Mask.GetAddressBytes();
                    byte[] ipAddr = uniIpInfo.Address.GetAddressBytes();

                    // we reverse the byte-array if we are dealing with littl endian.
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(subnetMask);
                        Array.Reverse(ipAddr);
                    }

                    uint maskAsInt = BitConverter.ToUInt32(subnetMask, 0);
                    uint ipAsInt = BitConverter.ToUInt32(ipAddr, 0);

                    //we negate the subnet to determine the maximum number of host possible in this subnet
                    uint validHostsEndingMax = ~BitConverter.ToUInt32(subnetMask, 0);

                    uint validHostsStart = BitConverter.ToUInt32(ipAddr, 0) & BitConverter.ToUInt32(subnetMask, 0);

                    //we increment the startIp to the number of maximum valid hosts in this subnet and for each we check the intended port (refactoring needed)
                    for (uint i = 1; i <= validHostsEndingMax; i++)
                    {
                        uint host = validHostsStart + i;
                        byte[] hostBytes = BitConverter.GetBytes(host);
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(hostBytes);
                        }

                        //this is the candidate IP address in "readable format" 
                        String ipCandidate = Convert.ToString(hostBytes[0]) + "." + Convert.ToString(hostBytes[1]) + "." + Convert.ToString(hostBytes[2]) + "." + Convert.ToString(hostBytes[3]);
                        Console.WriteLine("Trying: " + ipCandidate);


                        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        IAsyncResult result = socket.BeginConnect(ipCandidate, port, null, null);

                        bool success = result.AsyncWaitHandle.WaitOne(40, true);

                        if (!success)
                        {
                            socket.Close();
                            Console.WriteLine("No server on " + ipCandidate + ":" + port);
                        }
                        else
                        {
                            socket.Close();
                            Console.WriteLine("Found server on " + ipCandidate + ":" + port);

                            return new IPAddress(hostBytes);
                        }

                    }
                }
            }
            Console.WriteLine("No server found");
            return null;
        }

    }
}
