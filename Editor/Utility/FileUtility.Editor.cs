using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TapEmpire.Utility
{
    public static class EditorFileUtility
    {
        public static void DeleteFiles(string folder, string substring = "")
        {
            if (Directory.Exists(folder))
            {
                string[] files = Directory.GetFileSystemEntries(folder);

                files.Where(file => file.Contains(substring)).ForEach(file =>
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                    else
                    {
                        Directory.Delete(file, true);
                    }
                });
            }
        }

        public static IEnumerable<string> GetFiles(string folder, string substring = "")
        {
            if (Directory.Exists(folder))
            {
                string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);

                return files
                    .Where(file => file.Contains(substring))
                    .Select(file => Path.GetRelativePath(Directory.GetCurrentDirectory(), file));
            }

            return new string[0];
        }

    }
}