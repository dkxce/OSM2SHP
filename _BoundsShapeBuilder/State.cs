using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace KMZRebuilder
{
    [Serializable]
    public class State : XMLSaved<State>
    {
        public string SASCacheDir = @"C:\Program Files\SASPlanet\cache";
        public int MapID = -1;
        public string SASDir = null;
        public string URL = null;
        public double X;
        public double Y;
        public byte Z;
    }

    [Serializable]
    public class XMLSaved<T>
    {
        public static string ToUpper(string str)
        {
            if (String.IsNullOrEmpty(str)) return "";
            return str.ToUpper();
        }

        /// <summary>
        ///     Сохранение структуры в файл
        /// </summary>
        /// <param name="file">Полный путь к файлу</param>
        /// <param name="obj">Структура</param>
        public static void Save(string file, T obj)
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
            System.IO.StreamWriter writer = System.IO.File.CreateText(file);
            xs.Serialize(writer, obj);
            writer.Flush();
            writer.Close();
        }

        public static string Save(T obj)
        {
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
            System.IO.MemoryStream ms = new MemoryStream();
            System.IO.StreamWriter writer = new StreamWriter(ms);
            xs.Serialize(writer, obj);
            writer.Flush();
            ms.Position = 0;
            byte[] bb = new byte[ms.Length];
            ms.Read(bb, 0, bb.Length);
            writer.Close();
            return System.Text.Encoding.UTF8.GetString(bb); ;
        }

        /// <summary>
        ///     Подключение структуры из файла
        /// </summary>
        /// <param name="file">Полный путь к файлу</param>
        /// <returns>Структура</returns>
        public static T Load(string file)
        {
            // if couldn't create file in temp - add credintals
            // http://support.microsoft.com/kb/908158/ru
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
            System.IO.StreamReader reader = System.IO.File.OpenText(file);
            T c = (T)xs.Deserialize(reader);
            reader.Close();
            return c;
        }

        public static string CurrentDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
            // return Application.StartupPath;
            // return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // return System.IO.Directory.GetCurrentDirectory();
            // return Environment.CurrentDirectory;
            // return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            // return System.IO.Path.GetDirectory(Application.ExecutablePath);
        }
    }

    [Serializable]
    public class MapStore
    {
        public string Name;
        public string Url;
        public string CacheDir;
        public NaviMapNet.NaviMapNetViewer.MapServices Service = NaviMapNet.NaviMapNetViewer.MapServices.Custom_UserDefined;
        public NaviMapNet.NaviMapNetViewer.ImageSourceTypes Source = NaviMapNet.NaviMapNetViewer.ImageSourceTypes.tiles;
        public NaviMapNet.NaviMapNetViewer.ImageSourceProjections Projection = NaviMapNet.NaviMapNetViewer.ImageSourceProjections.EPSG3857;

        public override string ToString()
        {
            return Name;
        }

        public MapStore() { }
        public MapStore(string Name) { this.Name = Name; }
        public MapStore(string Name, string Url, string Cache)
        {
            this.Name = Name;
            this.Url = Url;
            this.CacheDir = Cache;
        }
    }
}
