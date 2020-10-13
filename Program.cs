using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace WaveLengthRead {
    class Program {
        static void Main(string[] args)
        {
            int port = 7802;
            string host = "10.1.1.180";

            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, port);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("Connecting to wavemeter at IP:port = {0}:{1}", host, port);
            try
            {
                clientSocket.Connect(ipe);
                clientSocket.SendTimeout = 3;
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to connect to {0}:{1}", host, port);
                Thread.Sleep(3);
                Environment.Exit(0);
            }
            //Demonstrate simple command
            clientSocket.Send(String2Byte("info\n"));
            Console.WriteLine("Connected to {0}", ReceiveString(clientSocket));

            //Initialise sensor
            clientSocket.Send(String2Byte("get,row\n"));
            int row = 0;
            try
            {
                row = Int32.Parse(ReceiveString(clientSocket));
            }
            catch (Exception)
            {
                Console.WriteLine("The Row not a number!");
                Thread.Sleep(3);
            }
            //    int rows = 2;
            int cols = 2592;
            string cmd = "cam,fov," + row + ",0,1," + (cols - 1) + "\n";
            clientSocket.Send(String2Byte(cmd));
            string ok = ReceiveString(clientSocket);
            int bufsize = cols * 2; // two bytes per pixel
                                    //    int maxy = 2 * 4096;

            // Find optimum exposure time
            // cam,atime,<max> finds optimum but with
            // maximum of <max> milliseconds
            cmd = "cam,atime,400\n";
            clientSocket.Send(String2Byte(cmd));
            string shutterStr = ReceiveString(clientSocket);
            float shutterTime = 0;
            try
            {
                shutterTime = float.Parse(shutterStr);
            }
            catch (Exception)
            {
                Console.WriteLine("ShutterTime is not a number!");
                Thread.Sleep(3);
            }
            Console.WriteLine("Exposure time:{0}ms", shutterTime);
            if (400 - shutterTime < 1)
            {
                Console.WriteLine("Insufficient input power");
                Thread.Sleep(3);
                //Environment.Exit(0);
            }

            // Report measured wavelength
            clientSocket.Send(String2Byte("wavelength\n"));
            Console.WriteLine("Wavelength: {0}", ReceiveString(clientSocket));

            double wavelength;
            for (int i = 1; i < 50; i++)
            {
                wavelength = GetWavelength(clientSocket);
                Console.WriteLine("Wavelength: {0}", wavelength);
            }

            Console.WriteLine("End!");
        }

        static byte[] String2Byte(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            return bytes;
        }
        static string ReceiveString(Socket clientSocket)
        {
            string recStr = "";
            byte[] recBytes = new byte[4096];
            int bytes = clientSocket.Receive(recBytes, recBytes.Length, 0);
            recStr += Encoding.ASCII.GetString(recBytes, 0, bytes);
            return recStr;
        }

        static void SpectrumLoop(Socket clientSocket)
        {

        }
        static double GetWavelength(Socket clientSocket)
        {
            clientSocket.Send(String2Byte("wavelength\n"));
            return Double.Parse(ReceiveString(clientSocket));
        }
    }
}

