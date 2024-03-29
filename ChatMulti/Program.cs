﻿using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace ChatMulti
{
  


  public class Program
    {
        public static bool serverstatus = false;
        public static Dictionary<string,TcpClient> Clients = new Dictionary<string,TcpClient>();
        public static Stack<string> GroupMessage = new Stack<string>();
        public static  void Main(string[] args)
        {
            try
            {
                IPAddress ServerAddress = IPAddress.Parse("192.168.10.189");
                TcpListener serversocket = new TcpListener(ServerAddress, 8888);
                int counter = 0;
                TcpClient clientSocket = default(TcpClient);
                serversocket.Start();
                Console.WriteLine("Chat Room ");
                serverstatus = true;
                while (true)
                {
                    
                    counter += 1;
                    clientSocket = serversocket.AcceptTcpClient();
                    HandleClient Client = new HandleClient();
                    Client.StartClient(clientSocket, counter);
                }
            }
            catch
            {
                Console.WriteLine("Error please Restart the server");
            }
        }


    }
    class HandleClient:Program
    {
        TcpClient Client;
        public string ClientName;
        public Stack<string> UserMessages = new Stack<string>();
        public void StartClient(TcpClient inClient,int count)
        {
            this.Client = inClient;
            Console.WriteLine($"Client {count}  ");
            AssignName();
            try
            {
                Clients.Add(ClientName, inClient);
            }
            catch
            {
                ClientName = "Client" + count;
                Clients.Add(ClientName, inClient);
            }

            Thread thread1 = new Thread(Chat);
            thread1.Start();

        }

        private void AssignName()
        {
            string returndata;
            byte[] bytesFrom = new byte[500];
            NetworkStream networkStream = Client.GetStream();
            int reply = networkStream.Read(bytesFrom, 0, bytesFrom.Length);
            if (reply > 0)
            {

                returndata = System.Text.Encoding.ASCII.GetString(bytesFrom);
                ClientName = returndata;
                networkStream.Flush();

            }
        }
        void Speak()
        {
            //This segment of code stops the for loop once GroupMessage has ended, If the try is omitted the broadcast is sent on an infinite loop
            try
            {
                foreach (string entry in GroupMessage)
                {

                    GroupMessage.Pop();
                    foreach (KeyValuePair<string, TcpClient> entry1 in Clients)
                    {
                        try
                        {
                            byte[] bytesTo = new byte[500];
                            NetworkStream stream = entry1.Value.GetStream();
                            bytesTo = System.Text.Encoding.ASCII.GetBytes(entry);
                            stream.Write(bytesTo, 0, bytesTo.Length);
                            stream.Flush();


                        }
                        catch
                        {
                            Console.WriteLine("Tcp Client is missing");
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("End of Message Stack");
            }
                }
            
        
        private void Chat()
        {
            string returndata;
            byte[] bytesFrom = new byte[500];
            byte[] bytesTo = new byte[500];
            NetworkStream networkStream = this.Client.GetStream();

            while (true)
            {
                try
                {

              
                    int reply = networkStream.Read(bytesFrom, 0, bytesFrom.Length);

                    if (reply > 0)
                    {

                        returndata = System.Text.Encoding.ASCII.GetString(bytesFrom);
                        string[] Message1 = returndata.Split('*');
                        string[] Message = Message1[1].Split('/');
                        Console.WriteLine(Message[0]);
                        bytesFrom = new byte[500];
                        if(Message.Length == 1)
                        {
                          string check= Message[0];
                            Console.WriteLine(check);
                            if(check.ToLower().Equals("all",StringComparison.CurrentCulture))
                            {
                                 ReturnClients(networkStream);
                                bytesTo = new byte[500];
                            }
                            else if(check.Equals("ByeFromChatRoom", StringComparison.CurrentCulture))
                            {
                             RemoveClient(networkStream,Message1[0]);  
                            }
                            else
                            {
                                GroupMessage.Push("BroadCast " + Message1[0] + ':' + Message[0]);
                            }
                        }
                        else if (Message.Length > 1)
                        {
                            foreach (KeyValuePair<string, TcpClient> entry in Clients)
                            {
                                if (Message[0].Equals(entry.Key, StringComparison.CurrentCulture))
                                {
                                    Console.WriteLine("Check");
                                    NetworkStream stream = entry.Value.GetStream();
                                    bytesTo = System.Text.Encoding.ASCII.GetBytes(Message1[0] + ":" + Message[1]);
                                    stream.Write(bytesTo, 0, bytesTo.Length);

                                }
                            }

                        }

                    }
                    Speak();
                    //bytesTo = System.Text.Encoding.ASCII.GetBytes(Console.ReadLine());
                    //networkStream.Write(bytesTo, 0, bytesTo.Length);
                    //networkStream.Flush();

                }
                catch(Exception)
                {
                    Client.Close();
                }
            }
        }

        private static void ReturnClients(NetworkStream networkStream)
        {
            byte[] bytesTo=new byte[500];
            string message = "Please Find the name of the connected entered below";
            bytesTo = System.Text.Encoding.ASCII.GetBytes(message);
            networkStream.Write(bytesTo, 0, bytesTo.Length);
            networkStream.Flush();
            int count=0;
            foreach (KeyValuePair<string, TcpClient> entry in Clients)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                while (true)
                {
                    if (watch.ElapsedMilliseconds == 300)
                    { break; }
                }
                watch.Stop();
                    try
                    {
                        ++count;
                        string Message = entry.Key + "/";
                        bytesTo = System.Text.Encoding.ASCII.GetBytes(Message);
                        networkStream.Write(bytesTo, 0, bytesTo.Length);
                        networkStream.Flush();
                        if (count == Clients.Count + 1) { break; }
                    }
                    catch { Console.WriteLine("Missing TCP Client"); }
                
               
            }
            
         
        }
        private static void RemoveClient(NetworkStream networkStream,string Name)
        {
           
            foreach (KeyValuePair<string, TcpClient> entry in Clients)
            {
                try
                {
                    if (Name.Equals(entry.Key, StringComparison.CurrentCulture))
                    {

                        Clients.Remove(entry.Key);
                        Console.WriteLine($"{Name} just disconnected");
                    }
                }
                catch
                {
                    Console.WriteLine("Couldnt Remove the Client");
                }
            }

           
        }
    }
}
