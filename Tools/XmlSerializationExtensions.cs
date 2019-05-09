using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace Tools
{
    public static class XmlSerializationExtensions
    {
        public static string ToXmlString(this object source)
        {
            string result = String.Empty;
            if (source != null)
            {
                XmlSerializer serializer = new XmlSerializer(source.GetType());
                StringBuilder sb = new StringBuilder();

                using (StringWriter writer = new StringWriter(sb))
                {
                    serializer.Serialize(writer, source);
                    result = sb.ToString();
                }
            }
            return result;
        }

        public static XmlDocument ToXmlDocument(this object source)
        {
            XmlDocument result = null;
            if (source != null)
            {
                XmlSerializer serializer = new XmlSerializer(source.GetType());
                StringBuilder sb = new StringBuilder();

                using (StringWriter writer = new StringWriter(sb))
                {
                    serializer.Serialize(writer, source);
                    result = new XmlDocument();
                    result.LoadXml(sb.ToString());
                }
            }
            return result;
        }

        public static void ToXmlFile(this object source, string path)
        {
            if (source != null)
            {
                XmlSerializer serializer = new XmlSerializer(source.GetType());
                using (StreamWriter writer = new StreamWriter(path))
                {
                    serializer.Serialize(writer, source);
                }
            }
        }

        public static T FromXmlString<T>(this string source)
        {
            T result = default(T);
            if (!String.IsNullOrEmpty(source))
            {
                using (StringReader reader = new StringReader(source))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    result = (T)serializer.Deserialize(reader);
                }
            }
            return result;
        }

        public static T FromXmlFile<T>(this string path)
        {
            T result = default(T);
            if (!String.IsNullOrEmpty(path))
            {
                using (TextReader textReader = new StreamReader(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    result = (T)serializer.Deserialize(textReader);
                }
            }
            return result;
        }

        public static T FromXmlDocument<T>(this XmlDocument source)
        {
            T result = default(T);
            if (source != null)
            {
                XmlNodeReader reader = new XmlNodeReader(source.DocumentElement);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                result = (T)serializer.Deserialize(reader);
                reader.Close();
            }
            return result;
        }

    }
}
