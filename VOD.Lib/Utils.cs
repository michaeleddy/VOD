using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using VOD.Lib.Libs;
using VOD.Lib.Models;

namespace VOD.Lib
{
    public static class Utils
    {
        static string ConfigFile { get; set; }
        static Utils()
        {
            ConfigFile = Assembly.GetExecutingAssembly().Location + ".config";
        }
        public static MusicModel Get(this ItemCollection collection, int index)
        {
            try
            {
                return collection[index] as MusicModel;
            }
            catch { }
            return null;
        }
        public static MusicModel Find(this ItemCollection collection, Func<MusicModel, bool> predicate)
        {
            try
            {
                List<MusicModel> models = new List<MusicModel>();
                foreach (var item in collection)
                {
                    if (item is MusicModel row)
                        models.Add(row);
                }
                return models.FirstOrDefault(predicate);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("Find", ex);
            }
            return null;
        }
        public static string ReplaceSpace(this string str)
        {
            return str.Replace(" ", "");
        }
        public static async Task ReadBAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            try
            {
                if (offset + count > buffer.Length) throw new ArgumentException();
                int read = 0;
                while (read < count)
                {
                    var available = await stream.ReadAsync(buffer, offset, count - read);
                    if (available == 0) throw new ObjectDisposedException(null);
                    read += available;
                    offset += available;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("ReadBAsync", ex);
            }
        }
        public static int ToInt32(this string num)
        {
            try
            {
                return Convert.ToInt32(num);
            }
            catch
            {
                return 0;
            }
        }
        public static long ToInt64(this string num)
        {
            try
            {
                return Convert.ToInt64(num);
            }
            catch
            {
                return 0;
            }
        }
        public static string ToMD5(this string input)
        {
            try
            {
                byte[] result = Encoding.UTF8.GetBytes(input.Trim());
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] output = md5.ComputeHash(result);
                return BitConverter.ToString(output).Replace("-", "");
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("ToMD5", ex);
                return string.Empty;
            }
        }
        public static string GetConfig(this string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch
            {
                return string.Empty;
            }
        }
        public static bool IsEmpty<T>(this T obj)
        {
            try
            {
                if (obj == null) return true;
                if (obj is string)
                    return string.IsNullOrEmpty(obj.ToString());
                return obj.Equals(default);
            }
            catch
            {
                return false;
            }
        }
        public static bool IsEmptyArray<T>(this IEnumerable<T> objs)
        {
            if (objs == null) return true;
            if (objs.Count() == 0) return true;
            return false;
        }
        public static bool IsNotEmpty<T>(this T obj)
        {
            return !IsEmpty(obj);
        }
        public static SaveState SaveConfig(this string key, string value)
        {
            if (key.GetConfig() == value)
                return SaveState.NoChange;
            XmlDocument doc = new XmlDocument();
            doc.Load(ConfigFile);
            XmlNode node = doc.SelectSingleNode("//appSettings");
            if (node == null)
                throw new InvalidOperationException("appSettings section not found in config file.");
            try
            {
                XmlElement elem = (XmlElement)node.SelectSingleNode(string.Format("//add[@key='{0}']", key));
                if (elem != null)
                    elem.SetAttribute("value", value);
                else
                {
                    elem = doc.CreateElement("add");
                    elem.SetAttribute("key", key);
                    elem.SetAttribute("value", value);
                    node.AppendChild(elem);
                }
                doc.Save(ConfigFile);
                return SaveState.Update;
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError("SaveConfig", ex);
                return SaveState.Failed;
            }
        }
    }
}