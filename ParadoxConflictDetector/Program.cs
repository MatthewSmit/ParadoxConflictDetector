using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;

namespace ParadoxConflictDetector
{
    internal static class Program
    {
        private sealed class Mod
        {
            public string Name;
            public FileInfo Path;
            public List<string> Files;
        }

        private sealed class Conflict
        {
            public readonly Mod ModA;
            public readonly Mod ModB;
            public readonly string FileName;

            public Conflict(Mod modA, Mod modB, string file)
            {
                ModA = modA;
                ModB = modB;
                FileName = file;
            }
        }

        private static void Main()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var stellarisDirectory =  Path.Combine(documents, @"Paradox Interactive\Stellaris\");

            var settings = ParadoxFile.Parse(Path.Combine(stellarisDirectory, "settings.txt"));

            var mods = settings["last_mods"].Children.Select(mod => new Mod {Path = new FileInfo(Path.Combine(stellarisDirectory, mod.Value))}).ToArray();

            foreach (var mod in mods)
            {
                var modFile = ParadoxFile.Parse(mod.Path.OpenText());
                mod.Name = modFile["name"].Value;
                if (mod.Path.Name.StartsWith("ugc_", StringComparison.OrdinalIgnoreCase))
                    mod.Name += " (" + modFile["remote_file_id"].Value + ")";

                if (modFile["path"] != null)
                {
                    var path = modFile["path"].Value;
                    var directory = new DirectoryInfo(Path.Combine(stellarisDirectory, path));

                    mod.Files = directory.EnumerateFiles("*", SearchOption.AllDirectories).Select(x => x.FullName.Substring(directory.FullName.Length).ToLowerInvariant().Replace('\\', '/')).ToList();
                }
                else if (modFile["archive"] != null)
                {
                    var archive = modFile["archive"].Value;
                    var directory = new DirectoryInfo(Path.Combine(stellarisDirectory, archive));
                    var zip = ZipFile.Read(directory.FullName);
                    mod.Files = zip.EntryFileNames.Select(x => "/" + x.ToLowerInvariant().Replace('\\', '/')).ToList();
                }
                else throw new InvalidOperationException();
            }

            var conflicts = new List<Conflict>();
            for (var i = 0; i < mods.Length; i++)
            {
                var modA = mods[i];
                for (var j = i + 1; j < mods.Length; j++)
                {
                    var modB = mods[j];
                    conflicts.AddRange(modA.Files.Intersect(modB.Files).Where(x => x != "/descriptor.mod").Select(file => new Conflict(modA, modB, file)));
                }
            }

            using (var output = File.CreateText("dump.txt"))
            {
                foreach (var conflict in conflicts)
                {
                    output.WriteLine($"{conflict.ModA.Name} CONFLICTS WITH {conflict.ModB.Name} = {conflict.FileName}");
                }
            }
        }
    }
}
