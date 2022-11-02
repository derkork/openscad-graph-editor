using System;
using System.IO;
using System.Linq;
using OpenScadGraphEditor.Widgets;
using Serilog;

namespace OpenScadGraphEditor.Library.IO
{
    public static class FileBackups
    {
        /// <summary>
        /// Creates a backup of the specified file. The backup file will be named with the same name as the original file,
        /// but with a number (e.g. ".1", ".10") appended to the end. The number will increase on every backup. After the backup is created,
        /// the number of backups will be reduced to the specified maximum. This is done by deleting the oldest backup
        /// (as in, the one with the lowest number) until the number of backups is less or equal to the specified maximum.
        /// </summary>
        public static void BackupFile(string filePath, int numberOfBackups)
        {
            try
            {
                // Get the directory of the file
                var directory = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(directory))
                {
                    Log.Error("Could not get directory of file {filePath}", filePath);
                    NotificationService.ShowError("Error while creating backup. See log for details.");

                    return;
                }

                // list all existing backups (files with the same name as the original file, but with a number appended to the end)
                var backups = Directory.GetFiles(directory, Path.GetFileName(filePath) + ".*")
                    // make pairs of the backup file name and the number
                    .Select(b => new {Backup = b, Number = SafeParseInt(Path.GetExtension(b).Substring(1))})
                    // filter out the ones that don't have a parsable number (-1)
                    .Where(b => b.Number != -1)
                    // sort by the number (descending)
                    .OrderByDescending(b => b.Number)
                    .ToList();

                // rename the backups to the next number (starting with the highest number so we don't overwrite any files,
                // hence the reverse order)
                for (var i = 0; i < backups.Count; i++)
                {
                    var backup = backups[i];
                    var newNumber = backup.Number + 1;
                    var newBackup = Path.Combine(directory,
                        Path.GetFileNameWithoutExtension(backup.Backup) + "." + newNumber);
                    File.Move(backup.Backup, newBackup);
                    // save changes to the list
                    backups[i] = new {Backup = newBackup, Number = newNumber};
                }

                if (numberOfBackups > 0)
                {
                    // copy the original file to the backup file with the lowest number (1)
                    var backupFile = Path.Combine(directory, Path.GetFileName(filePath) + ".1");
                    File.Copy(filePath, backupFile);
                }

                // delete the oldest backups until the number of backups is less or equal to the specified maximum
                var backupsToDelete = backups
                    .Where(it => it.Number > numberOfBackups)
                    .ToList();

                foreach (var backup in backupsToDelete)
                {
                    File.Delete(backup.Backup);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while creating backup");
                NotificationService.ShowError("Error while creating backup. See log for details.");
            }
        }

        private static int SafeParseInt(string s)
        {
            if (int.TryParse(s, out var result))
            {
                return result;
            }

            return -1;
        }
    }
}