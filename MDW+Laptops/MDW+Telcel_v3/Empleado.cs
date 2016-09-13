using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MDW
{
    public class Empleado
    {
        public string Id { get; set; }
        public string IP { get; set; }
        public string[] Laptops { get; set; }
        public string TimeStamp { get; set; }
        public Empleado()
        {
            Id = "";
            IP = "";
            Laptops = new string[] { };
            TimeStamp = "";
        }
        public Empleado(string id, string ip, string[] laptops)
        {
            Id = id;
            IP = ip;
            Laptops = laptops;
            TimeStamp = DateTime.Now.ToString();
        }

        public static string Serializar(Empleado toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }
    }

    public class Respuesta
    {
        public string IP { get; set; }
        public bool Permitido { get; set; }
    }
}
