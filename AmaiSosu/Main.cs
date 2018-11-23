using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AmaiSosu.Installation;
using AmaiSosu.Installation.IO;
using AmaiSosu.Resources;

namespace AmaiSosu
{
    /// <summary>
    ///     Main AmaiSosu model.
    /// </summary>
    public sealed class Main
    {
        private readonly string _path;

        public Main(string path)
        {
            _path = path;
        }

        /// <summary>
        ///     Invokes the installation procedure.
        /// </summary>
        public void Install()
        {
            var backupDir = Path.Combine(_path, FileNames.AmaiSosuBackup + '.' + Guid.NewGuid());

            CommitBackups(backupDir);
            new InstallerFactory(_path).Get().Install();
            FinishInstall(backupDir);
        }

        /// <summary>
        ///     Conducts the OpenSauce & HAC2 data backup routines.
        /// </summary>
        /// <param name="backupDir">
        ///     Backup directory to use for backing up OpenSauce & HAC2 data.
        /// </param>
        private void CommitBackups(string backupDir)
        {
            Directory.CreateDirectory(backupDir);

            new List<Move>
            {
                MoveFactory.Get(MoveFactory.Type.BackupOsFiles, _path, backupDir),
                MoveFactory.Get(MoveFactory.Type.BackupOsDirectories, _path, backupDir),
                MoveFactory.Get(MoveFactory.Type.BackupHac2Files, _path, backupDir)
            }.ForEach(move => move.Commit());
        }

        /// <summary>
        ///     Conducts optional installation finalisation routines.
        ///     - Restore the HCE shaders.
        ///     - Move the OpenSauce IDE.
        ///     - Backup directory cleanup.
        /// </summary>
        /// <param name="backupDir"></param>
        private void FinishInstall(string backupDir)
        {
            // restore backed up HCE shaders
            MoveFactory.Get(MoveFactory.Type.RestoreHceShaders, _path, backupDir).Commit();

            var source =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    FileNames.OpenSauceDeveloper, FileNames.OpenSauceDirectory, FileNames.OpenSauceIDE);

            var target = Path.Combine(_path, FileNames.OpenSauceIDE);

            Copy.All(new DirectoryInfo(source), new DirectoryInfo(target));
            Directory.Delete(source, true);

            // cleans up backup directory
            if (!Directory.EnumerateFileSystemEntries(backupDir).Any()) Directory.Delete(backupDir);
        }
    }
}