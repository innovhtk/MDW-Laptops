using HTKCSL;
using HTKSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace MDW
{
    class Program
    {
        public static CS203 entrada;
        public static CS203 salida;
        public static string IP = "191.9.6.199";
        public static int Puerto = 10008;
        public static int EsperaHID = 1000;
        public static int EsperaRFID = 4000;
        public static int timeRevision = 5;
        public static bool ok = true;
        public static System.Timers.Timer timer = new System.Timers.Timer(600000);
        public static ReaderList x;
        public static List<string> Correos = new List<string>();
        static void Main(string[] args)
        {
            bool correct = false;
            while (!correct)
            {
                try
                {
                    if (args.Length == 5)
                    {
                        try
                        {
                            timeRevision = Convert.ToInt32(args[0]);
                            IP = args[1];
                            Puerto = Convert.ToInt32(args[2]);
                            EsperaHID = Convert.ToInt32(args[3]);
                            EsperaRFID = Convert.ToInt32(args[4]);
                        }
                        catch (Exception)
                        {

                            correct = false;
                            ok = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Leyendo lista de correos en: ...\\MDW+\\correos.txt");
                        string[] lines = File.ReadAllLines(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\correos.txt");
                        Correos = new List<string>(lines);
                        foreach (string correo in Correos)
                        {
                            Console.WriteLine(correo);
                        }
                        Console.Write("\nIntroduzca el tiempo entre revisiones del sistema [minutos]: ");
                        timeRevision = Convert.ToInt32(Console.ReadLine());
                        Console.Write("\nIntroduzca la IP del servidor: ");
                        IP = Console.ReadLine();
                        Console.Write("\nIntroduzca el puerto: ");
                        Puerto = Convert.ToInt32(Console.ReadLine());
                        Console.Write("\nIntroduzca el tiempo de espera entre lecturas HID [milisegundos]: ");
                        EsperaHID = Convert.ToInt32(Console.ReadLine());
                        Console.Write("\nIntroduzca el tiempo de espera entre lecturas RFID [milisegundos]: ");
                        EsperaRFID = Convert.ToInt32(Convert.ToInt32(Console.ReadLine()) / 1000);
                        correct = true;
                        ok = true;
                    }

                }
                catch (Exception)
                {
                    correct = false;
                    ok = false;
                }
            }

            try
            {
                Console.WriteLine("\nConectando con los siguientes datos:");
                Console.WriteLine("\tEntrada: \t192.168.25.1");
                Console.WriteLine("\tSalida: \t192.168.25.2");
                Console.WriteLine("\tServidor:\t" + IP);
                Console.WriteLine("\tPuerto:\t\t" + Puerto.ToString());
                Console.WriteLine("");
                x = new ReaderList();


                entrada = new CS203("192.168.25.1");
                entrada.Connect(true, true);
                salida = new CS203("192.168.25.2");
                salida.Connect(true, true);
                entrada.EraseTime = EsperaRFID;
                salida.EraseTime = EsperaRFID;
                if (entrada.Connected && salida.Connected)
                {
                    Console.WriteLine("Conexión establecida");
                }
                else
                {
                    if (!entrada.Connected) Console.WriteLine("No se pudo conectar la antena de entrada con ip " + entrada.IP);
                    if (!salida.Connected) Console.WriteLine("No se pudo conectar la antena de salida con ip " + salida.IP);
                    Console.WriteLine("Conexión fallida");
                    ok = false;
                }
                x.CardPresented += x_CardPresented;
                x.Refresh();
                Thread.Sleep(1500);
                Console.WriteLine("Lectores HID:");
                bool EntradaHIDConectada = false;
                bool SalidaHIDConectada = false;
                foreach (var item in ReaderList.ConcurrentReaders)
                {
                    string name = "";
                    if (item.Key.Substring(item.Key.Length - 1, 1) == "0")
                    {
                        name = "Lector de entrada";
                        EntradaHIDConectada = item.Value;
                    }
                    else if (item.Key.Substring(item.Key.Length - 1, 1) == "1")
                    {
                        name = "Lector de salida";
                        SalidaHIDConectada = item.Value;
                    }
                    else
                    {
                        name = item.Key;
                    }
                    Console.WriteLine("\t" + name + ":\t" + (item.Value ? "Conectado" : "Desconectado"));
                }
                if (!EntradaHIDConectada) { Console.WriteLine("\nConecte el cable USB de la antena de entrada"); ok = false; };
                if (!SalidaHIDConectada) { Console.WriteLine("\nConecte el cable USB de la antena de salida\n"); ok = false; };

                Ping Pings = new Ping();
                int timeout = 10;

                if (Pings.Send(IP, timeout).Status == IPStatus.Success)
                {
                    Console.WriteLine("El servidor respode correctamente.");
                }
                else
                {
                    Console.WriteLine("El servidor no responde");
                    ok = false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\nRevise las conexiones del equipo. Presione Enter para cerrar la aplicación...");
                Console.ReadLine();
                Environment.Exit(0);
            }
            if (!ok)
            {
                Console.WriteLine("\nRevise las conexiones del equipo. Presione Enter para cerrar la aplicación...");
                Console.ReadLine();
                Environment.Exit(0);
            }
            timer = new System.Timers.Timer(timeRevision * 60 * 1000);
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            while (true)
            {

            }
        }
        public static bool EntradaHIDConectada = false;
        public static bool SalidaHIDConectada = false;
        public static bool ServidorConectado = false;
        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                x.Refresh();
                
                foreach (var item in ReaderList.ConcurrentReaders)
                {
                    string name = "";
                    if (item.Key.Substring(item.Key.Length - 1, 1) == "0")
                    {
                        name = "Lector de entrada";
                        EntradaHIDConectada = item.Value;
                    }
                    else if (item.Key.Substring(item.Key.Length - 1, 1) == "1")
                    {
                        name = "Lector de salida";
                        SalidaHIDConectada = item.Value;
                    }
                    else
                    {
                        name = item.Key;
                    }
                    if (!item.Value)
                    {
                        Console.WriteLine("\t" + name + ":\t" + (item.Value ? "Conectado" : "Desconectado"));
                    }
                }
                if (!EntradaHIDConectada) { Console.WriteLine("\nConecte el cable USB de la antena de entrada"); ok = false; };
                if (!SalidaHIDConectada) { Console.WriteLine("\nConecte el cable USB de la antena de salida\n"); ok = false; };

                Ping Pings = new Ping();
                int timeout = 10;

                if (Pings.Send(IP, timeout).Status != IPStatus.Success)
                {
                    Console.WriteLine("El servidor no responde");
                    ok = false;
                }
                else
                {
                    ServidorConectado = true;
                }
                if (!(entrada.Connected && salida.Connected && EntradaHIDConectada && SalidaHIDConectada && ServidorConectado))
                {
                    Task.Factory.StartNew(() => sendit(entrada.Connected, salida.Connected, EntradaHIDConectada, SalidaHIDConectada, ServidorConectado));
                }

            }
            catch (Exception ex)
            {
                ServidorConectado = false;
                Task.Factory.StartNew(() => sendit(ex.Message));
            }
        }
        public static void sendit(bool RFIDentrada, bool RFIDsalida, bool HIDentrada, bool HIDsalida, bool server)
        {
            foreach (string correo in Correos)
            {
                try
                {
                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(mailAddress);
                        mail.To.Add(correo);
                        mail.Subject = "Sistema HTK";
                        //mail.Body = "<h1>This is a test</h1>";
                        mail.Body = "<!doctype html> \n<html> \n<head> \n<title>Mensaje de sistema HTK</title> \n</head> \n<body> \n<h1><span style=\"font-family:trebuchet ms,helvetica,sans-serif;\"><span style=\"color:#000080;\"><span style=\"font-size:24px;\">Notificaciones HTK -&nbsp;<span style=\"line-height: 20.8px;\">MDW+ (Middleware)</span></span></span></span></h1> \n \n<p><span style=\"font-size:20px;\"><span style=\"font-family:trebuchet ms,helvetica,sans-serif;\"><span style=\"color:#000080;\"><span style=\"line-height: 20.8px;\">Datos</span></span></span></span></p> \n \n<p>&nbsp;</p> \n \n<table align=\"left\" border=\"0\" cellpadding=\"1\" cellspacing=\"1\" style=\"width:500px;\"> \n<tbody> \n<tr> \n<td>Antena RFID de entrada</td> \n<td>" + (RFIDentrada ? "Conectada" : "Desconectada")  + "</td> \n</tr> \n<tr> \n<td>Antena RFID de salida</td> \n<td>" + (RFIDsalida ? "Conectada" : "Desconectada") + "</td> </tr> \n<tr> \n<td>Lector HID de entrada</td> \n<td>" + (HIDentrada ? "Conectada" : "Desconectada") + "</td> \n</tr> \n<tr> \n<td>Lector HID de salida</td> \n<td>" + (HIDsalida ? "Conectado" : "Desconectado") + "</td> \n</tr> \n<tr> \n<td>Servidor</td> \n<td>" + (server ? "Conectado" : "Desconectado") + "</td> \n</tr> \n</tbody> \n</table> \n \n<p>&nbsp;</p> \n \n<p>&nbsp;</p> \n \n<p>&nbsp;</p> \n \n<p>&nbsp;</p> \n \n<p>&nbsp;</p> \n<p><strong><span style=\"font-size:18px;\">Por favor revise las conexiones del sistema.</span></strong></p> \n</body> \n</html>";
                        mail.IsBodyHtml = true;
                        //mail.Attachments.Add(new Attachment("C:\\file.zip"));

                        ServicePointManager.ServerCertificateValidationCallback =
                            delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                            { return true; };
                        using (SmtpClient smtp = new SmtpClient(mailHost, mailPort))
                        {
                            smtp.UseDefaultCredentials = false;
                            smtp.Credentials = new NetworkCredential(mailAddress, mailPass);
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }

                }
                catch (Exception ex)
                {
                    // Response.Write(ex.ToString());

                    Console.WriteLine("No se envió el correo a " + correo + ". " + ex.Message);

                }
            }

        }

        public static string mailAddress = "no-reply@mail.telcel.com";
        public static string mailHost = "imss1.telcel.com";
        public static int mailPort = 25;
        public static string mailPass = "";
        //public string mail = "noreply@htk-id.com")
        //public string host = "mail.htk-id.com";
        //public int mailPort = 26;
        //public static string mailPass = "KRVsdtb#7684";
        public static void sendit(string message)
        {
            foreach (string correo in Correos)
            {
                try
                {
                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(mailAddress);
                        mail.To.Add(correo);
                        mail.Subject = "Sistema HTK";
                        //mail.Body = "<h1>This is a test</h1>";
                        mail.Body = "<!doctype html> \n<html> \n<head> \n<title>Mensaje de Sistema HTK</title> \n</head> \n<body> \n<h1><span style=\"font-family:trebuchet ms,helvetica,sans-serif;\"><span style=\"color:#000080;\"><span style=\"font-size:24px;\">Notificaciones HTK -&nbsp;<span style=\"line-height: 20.8px;\">MDW+ (Middleware)</span></span></span></span></h1> \n \n<p><span style=\"font-size:20px;\"><span style=\"font-family:trebuchet ms,helvetica,sans-serif;\"><span style=\"color:#000080;\"><span style=\"line-height: 20.8px;\">Datos</span></span></span></span></p> \n \n<p>&nbsp;</p> \n \n<table align=\"left\" border=\"0\" cellpadding=\"1\" cellspacing=\"1\" style=\"width:500px;\"> \n<tbody> \n<tr> \n<td>Antena RFID de entrada</td> \n<td>" + (entrada.Connected ? "Conectada" : "Desconectada") + "</td> \n</tr> \n<tr> \n<td>Antena RFID de salida</td> \n<td>" + (salida.Connected ? "Conectada" : "Desconectada") + "</td> </tr> \n<tr> \n<td>Lector HID de entrada</td> \n<td>" + (EntradaHIDConectada ? "Conectada" : "Desconectada") + "</td> \n</tr> \n<tr> \n<td>Lector HID de salida</td> \n<td>" + (SalidaHIDConectada? "Conectado" : "Desconectado") + "</td> \n</tr> \n<tr> \n<td>Servidor</td> \n<td>" + (ServidorConectado ? "Conectado" : "Desconectado") + "</td> \n</tr> \n</tbody> \n</table> \n \n<p>&nbsp;</p> \n \n<p>&nbsp;</p> \n \n<p>&nbsp;</p> \n \n<p>&nbsp;</p> \n \n<p>&nbsp;</p> \n<p><strong><span style=\"font-size:18px;\">"+message+"</span></strong></p> \n</body> \n</html>";
                        mail.IsBodyHtml = true;
                        //mail.Attachments.Add(new Attachment("C:\\file.zip"));

                        ServicePointManager.ServerCertificateValidationCallback =
                            delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                            { return true; };
                        using (SmtpClient smtp = new SmtpClient(mailHost, mailPort))
                        {
                            smtp.UseDefaultCredentials = false;
                            smtp.Credentials = new NetworkCredential(mailAddress, mailPass);
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }

                }
                catch (Exception ex)
                {
                    // Response.Write(ex.ToString());

                    Console.WriteLine("No se envió el correo a " + correo + ". " + ex.Message);

                }
            }

        }
        static void x_CardPresented(string reader, string uid, byte[] cardData)
        {

            try
            {
                var hex = SCARD.ToHex(cardData, "");
                var bin = hex2bin(hex.Substring(hex.Length - 8));
                bin = bin.Substring(bin.Length - 26);

                var f1 = Convert.ToInt32(bin.Substring(1, 8), 2);
                var c1 = Convert.ToInt32(bin.Substring(9, 16), 2);

                var raw = new BitArray(cardData);
                var x = raw.Length - 25;

                BitArray fac = new BitArray(8);
                BitArray car = new BitArray(16);
                for (int i = 0; i < 8; i++) { fac[i] = raw[x + i]; }
                for (int f = 0; f < 16; f++) { car[f] = raw[x + 8 + f]; }
                var facility = ToNumeral(fac);
                var cardNum = ToNumeral(car);
                List<string> epcs = new List<string>();
                //var hexuid = SCARD.ToHex(uid, "");

                string currentAntena = "";
                if (reader.Substring(reader.Length - 1, 1) == "0")
                {
                    currentAntena = entrada.IP;
                    foreach (var item in entrada.TagList)
                    {
                        epcs.Add(item.EPC);
                    }
                }
                else
                {
                    currentAntena = salida.IP;
                    foreach (var item in salida.TagList)
                    {
                        epcs.Add(item.EPC);
                    }
                }
                string empleado = Empleado.Serializar(new Empleado(uid, currentAntena, epcs.ToArray()))
                    .Replace("\n", "").Replace("\r", "").Replace("\t", "");
                Console.WriteLine(empleado);
                entrada.TagList.Clear();
                salida.TagList.Clear();

                Task.Factory.StartNew(() => Publish(empleado));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex);
                    Task.Factory.StartNew(() => sendit(ex.Message));
            }
        }
        public static void Publish(string empleado)
        {
            try
            {
                using (Client client = new Client())
                {
                    client.Start(IP, Puerto);
                    Thread.Sleep(200);
                    client.SendMessage(empleado);
                    Thread.Sleep(EsperaHID);
                }

            }
            catch (Exception ex)
            {
                    Task.Factory.StartNew(() => sendit(ex.Message));
                Console.WriteLine(ex.Message);
            }
        }
        public static int ToNumeral(BitArray binary)
        {
            if (binary == null)
                throw new ArgumentNullException("binary");
            if (binary.Length > 32)
                throw new ArgumentException("must be at most 32 bits long");

            var result = new int[1];
            binary.CopyTo(result, 0);
            return result[0];
        }

        public static string hex2bin(string value)
        {
            return Convert.ToString(Convert.ToInt32(value, 16), 2).PadLeft(value.Length * 4, '0');
        }
    }
}
