using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Serveris
{
    public class Server
    {
        public static void Main()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[0];

            //Apsirasom siunteja/gaveja, sudarom rysi tarp ju.
            //Siuntejas - 10101
            //Gavejas - 20202
            var senderEndpoint = new IPEndPoint(ipAddr, 10101);
            using var senderListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var receiverEndpoint = new IPEndPoint(ipAddr, 20202);
            using var receiverListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                senderListener.Bind(senderEndpoint);
                senderListener.Listen(10);

                receiverListener.Bind(receiverEndpoint);
                receiverListener.Listen(10);

                Console.WriteLine("================== SERVERIS ====================");

                byte[] data = Array.Empty<byte>();
                while (true)
                {
                    using Socket senderSocket = senderListener.Accept();

                    var bytes = new byte[2048];
                    senderSocket.Receive(bytes, 0, 2048, SocketFlags.None);

                    byte[] messageLenBytes = new byte[4];
                    byte[] signatureLenBytes = new byte[4];
                    byte[] publicKeyLenBytes = new byte[4];

                    using (var ms = new MemoryStream(bytes))
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.Read(messageLenBytes, 0, 3);
                        ms.Seek(4, SeekOrigin.Begin);
                        ms.Read(signatureLenBytes, 0, 3);
                        ms.Seek(8, SeekOrigin.Begin);
                        ms.Read(publicKeyLenBytes, 0, 3);

                        int messageLen = BitConverter.ToInt32(messageLenBytes, 0);
                        int signatureLen = BitConverter.ToInt32(signatureLenBytes, 0);
                        int publicKeyLen = BitConverter.ToInt32(publicKeyLenBytes, 0);

                        byte[] messageBytes = new byte[messageLen];
                        byte[] signatureBytes = new byte[signatureLen];
                        byte[] publicKeyBytes = new byte[publicKeyLen];

                        // Read the data.
                        ms.Seek(12, SeekOrigin.Begin);
                        ms.Read(messageBytes, 0, messageLen);
                        ms.Seek(12 + messageLen, SeekOrigin.Begin);
                        ms.Read(signatureBytes, 0, signatureLen);
                        ms.Seek(12 + messageLen + signatureLen, SeekOrigin.Begin);
                        ms.Read(publicKeyBytes, 0, publicKeyLen);

                        Console.WriteLine("\n!!! GAUTA ZINUTE !!!");
                        Console.Write("AR NORITE PAKEISTI PARASA? (Y/N)\n >> ");
                        string opt = (Console.ReadLine() ?? "").ToLower();
                        while (opt != "y" && opt != "n")
                        {
                            Console.WriteLine("\nKLAIDINGA IVESTIS!");
                            Console.Write(" >> ");
                            opt = (Console.ReadLine() ?? "").ToLower();
                        }

                        if (opt == "y")
                        {
                            Console.Write("\nIVESKITE NAUJA PARASA\n >> ");
                            signatureBytes = Encoding.UTF8.GetBytes(Console.ReadLine() ?? "");
                            signatureLenBytes = BitConverter.GetBytes(signatureBytes.Length);

                            var updatedBytes = new byte[2048];
                            using (var newMs = new MemoryStream())
                            {
                                newMs.Write(BitConverter.GetBytes(messageLen), 0, 4);
                                newMs.Write(BitConverter.GetBytes(signatureBytes.Length), 0, 4);
                                newMs.Write(BitConverter.GetBytes(publicKeyLen), 0, 4);

                                newMs.Write(messageBytes);
                                newMs.Write(signatureBytes);
                                newMs.Write(publicKeyBytes);

                                bytes = newMs.ToArray();
                            }

                            Console.WriteLine("\nPARASAS PAKEISTAS!");
                        }
                    }


                    using Socket receiverSocket = receiverListener.Accept();

                    receiverSocket.Send(bytes);

                    Console.WriteLine("\nZINUTE ISSIUSTA GAVEJUI.");

                    senderSocket.Shutdown(SocketShutdown.Both);
                    senderSocket.Close();
                    receiverSocket.Shutdown(SocketShutdown.Both);
                    receiverSocket.Close();
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}