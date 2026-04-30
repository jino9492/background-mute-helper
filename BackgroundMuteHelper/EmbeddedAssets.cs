using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BackgroundMuteHelper
{
    internal static class EmbeddedAssets
    {
        private const string SettingResourceName = "BackgroundMuteHelper.setting.json";
        private const string ResourcesPrefix = "BackgroundMuteHelper.Resources.";

        private static string SettingDirectory
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BackgroundMuteHelper");
            }
        }

        public static string SettingFilePath
        {
            get { return Path.Combine(SettingDirectory, "setting.json"); }
        }

        public static void EnsureSettingDirectory()
        {
            Directory.CreateDirectory(SettingDirectory);
        }

        public static string ReadSettingJson()
        {
            string path = SettingFilePath;
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            return ReadEmbeddedText(SettingResourceName);
        }

        public static Icon LoadIcon(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            string diskPath = Path.Combine(Application.StartupPath, "Resources", fileName);
            if (File.Exists(diskPath))
            {
                return new Icon(diskPath);
            }

            string resourceName = ResourcesPrefix + fileName;
            using (Stream stream = typeof(EmbeddedAssets).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return null;
                }
                return new Icon(stream);
            }
        }

        private static string ReadEmbeddedText(string resourceName)
        {
            using (Stream stream = typeof(EmbeddedAssets).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException("포함된 리소스를 찾을 수 없습니다: " + resourceName);
                }
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
