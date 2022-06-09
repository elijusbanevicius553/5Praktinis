using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Siuntejas
{
    public class Siuntejas
    {
        public static void Main()
        {
            try
            {
                var ipHost = Dns.GetHostEntry(Dns.GetHostName());
                var ipAddr = ipHost.AddressList[0];
                var serverEndpoint = new IPEndPoint(ipAddr, 10101);

                Console.WriteLine ("================== SIUNTEJAS ===================== ");
                try
                {
                    while (true)
                    {
                        using var serverSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        serverSocket.Connect(serverEndpoint);

                        Console.Write("\nIVESKITE SAVO ZINUTE\n >> ");
                        string message = (Console.ReadLine() ?? "");

                        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                        byte[] signature = Array.Empty<byte>();
                        byte[] publicKey = Array.Empty<byte>();
                        using (var rsa = new RSACryptoServiceProvider(1024))
                        {
                            signature = rsa.SignData(messageBytes, SHA1.Create());
                            publicKey = rsa.ExportRSAPublicKey();
                        }

                        byte[] messageBytesLen = BitConverter.GetBytes(messageBytes.Length);
                        byte[] signatureLen = BitConverter.GetBytes(signature.Length);
                        byte[] publicKeyLen = BitConverter.GetBytes(publicKey.Length);


                        var ms = new MemoryStream();
                        ms.Write(messageBytesLen, 0, 4);
                        ms.Write(signatureLen, 0, 4);
                        ms.Write(publicKeyLen, 0, 4);

                        ms.Write(messageBytes);
                        ms.Write(signature);
                        ms.Write(publicKey);

                        ms.Flush();

                        serverSocket.Send(ms.ToArray());

                        Console.WriteLine("\nPARASAS SEKMINGAI UZDETAS, ZINUTE ISSIUSTA I SERVERI");

                        serverSocket.Shutdown(SocketShutdown.Both);
                        serverSocket.Close();
                    }
                }
                catch (ArgumentNullException ane)
                {

                    Console.WriteLine($"ArgumentNullException : {ane}");
                }

                catch (SocketException se)
                {

                    Console.WriteLine($"SocketException : {se}");
                }

                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected exception : {e}");
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
