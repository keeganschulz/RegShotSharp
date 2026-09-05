using Microsoft.Win32;
using RegShotSharp.DataSource;
using RegShotSharp.DataTypes;
using RegShotSharp.Tools;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace RegShotSharp
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Window
    {
        private DataManager _dataManager;

        public MainPage()
        {
            this.SizeToContent = SizeToContent.WidthAndHeight;
            _dataManager = new DataManager();
            InitializeComponent();
        }

        //TODO: add logic to check for no selection
        /// <summary>
        /// Gets the checked registry hives
        /// </summary>
        /// <returns>An array of the checked registry hives</returns>
        /// <exception cref="Exception">Invalid registry hive....somehow....</exception>
        private RegistryKey[] GetCheckedHives()
        {
            List<RegistryKey> selectedKeyList = new List<RegistryKey>();

            List<string> selectedKeys = [..((Panel)groupbox_hives.Content).Children.OfType<CheckBox>().Where(cb => cb.IsChecked == true).Select(cb => cb.Content.ToString())];

            foreach (string sk in selectedKeys)
            {
                switch (sk)
                {
                    case "CurrentUser":
                        selectedKeyList.Add(Registry.CurrentUser);
                        break;
                    case "ClassesRoot":
                        selectedKeyList.Add(Registry.ClassesRoot);
                        break;
                    case "LocalMachine":
                        selectedKeyList.Add(Registry.LocalMachine);
                        break;
                    case "Users":
                        selectedKeyList.Add(Registry.Users);
                        break;
                    case "CurrentConfig":
                        selectedKeyList.Add(Registry.CurrentConfig);
                        break;
                    default:
                        throw new Exception($"GetCheckedHives() -> Invalid registry hive {sk}");
                }
            }

            return selectedKeyList.ToArray();
        }

        /// <summary>
        /// Takes a snapshot of current registry and saves it to as a file using the current stamp
        /// </summary>
        /// <param name="a_topLevelKeys">The Registry Hives to capture</param>
        /// <param name="a_outputTextbox">IProgress handler for textbox_output</param>
        /// <param name="a_folderPath">Directory to save the file</param>
        /// <returns>Path to the file that is created</returns>
        private string Snapshot(RegistryKey[] a_topLevelKeys, IProgress<string> a_outputTextbox, string? a_folderPath = null)
        {
            DirectoryInfo di = Directory.CreateDirectory(a_folderPath ?? MiscTools.GetCurrentTimestamp());
            a_outputTextbox.Report($"Starting snapshot, saving all data to {di.FullName}");

            string dbPath = _dataManager.InitializeConnection(di.FullName);
            _dataManager.InsertSelectedHives(a_topLevelKeys.Select(tlk => tlk.Name).ToArray());

            List<RegistryEntry> entries;

            foreach (RegistryKey topLevelKey in a_topLevelKeys)
            {
                entries = new List<RegistryEntry>();
                a_outputTextbox.Report($"Parsing {topLevelKey.Name}");
                IterateRegistryKey(topLevelKey, entries);
                _dataManager.InsertSuccesses(entries);
            }

            a_outputTextbox.Report($"Finished with snapshot. Data saved to {dbPath}");
            return dbPath;
        }

        /// <summary>
        /// Takes a snapshot of the current Registry and compares it to a previous snapshot (passed in)
        /// </summary>
        /// <param name="a_topLevelKeys">The Registry Hives to capture and compare</param>
        /// <param name="a_compareFilePath">Path to the snapshot to compare against</param>
        /// <param name="a_outputTextbox">IProgress handler for textbox_output</param>
        private void Compare(RegistryKey[] a_topLevelKeys, string a_compareFilePath, IProgress<string> a_outputTextbox)
        {
            string snapshotDirectory = Path.GetDirectoryName(a_compareFilePath);
            string snapshotPath = Snapshot(a_topLevelKeys, a_outputTextbox, snapshotDirectory);
            
            _dataManager.AttachDatabase(a_compareFilePath);
            
            a_outputTextbox.Report("Checking for added or removed (altered) entries");
            List<AlteredEntry> alteredEntries = _dataManager.FindAlteredEntries();

            a_outputTextbox.Report("Checking for edited entries");
            List<EditedEntry> editedEntries = _dataManager.FinedEditedEntries();

            if (alteredEntries.Count > 0)
            {
                string alteredCsvFilePath = Path.Combine(snapshotDirectory, "alteredRegistries.csv");
                using (StreamWriter alteredWriter = new StreamWriter(alteredCsvFilePath))
                {
                    alteredWriter.WriteLine(AlteredEntry.GetCsvHeader());
                    foreach(AlteredEntry ae in alteredEntries)
                    {
                        alteredWriter.WriteLine(ae.ToCsvString());
                    }
                }

                a_outputTextbox.Report($"Added/removed entries written to {alteredCsvFilePath}");
            } else
            {
                a_outputTextbox.Report("No Added/removed registry entries found");
            }

            if (editedEntries.Count > 0)
            {
                string editedCsvFilePath = Path.Combine(snapshotDirectory, "editedRegistries.csv");
                using (StreamWriter editedWriter = new StreamWriter(editedCsvFilePath))
                {
                    editedWriter.WriteLine(EditedEntry.CsvHeader());
                    foreach(EditedEntry ee in editedEntries)
                    {
                        editedWriter.WriteLine(ee.ToCsvString());
                    }
                }

                a_outputTextbox.Report($"Edited entries written to {editedCsvFilePath}");
            } else
            {
                a_outputTextbox.Report("No edited registry entries found");
            }
        }

        /// <summary>
        /// Recursive function to iterate through the registry entries
        /// </summary>
        /// <param name="a_registryKey">The registry path to check</param>
        /// <param name="a_entries">List of registry path/vales</param>
        private void IterateRegistryKey(RegistryKey a_registryKey, List<RegistryEntry> a_entries)
        {
            foreach (string keyValue in a_registryKey.GetValueNames())
            {
                a_entries.Add(new RegistryEntry(a_registryKey, keyValue));
            }

            if (a_registryKey.SubKeyCount > 0)
            {
                foreach (string subKeyName in a_registryKey.GetSubKeyNames())
                {
                    try
                    {
                        RegistryKey? subRegistryKey = a_registryKey.OpenSubKey(subKeyName);
                        if (subRegistryKey != null)
                        {
                            IterateRegistryKey(subRegistryKey, a_entries);
                        }
                    }
                    catch
                    {
                        a_entries.Add(new RegistryEntry(a_registryKey, subKeyName, false));
                    }
                }
            }
        }

        private async void button_compare_Click(object sender, RoutedEventArgs e)
        {
            bool captureButtonState = button_capture.IsEnabled;

            button_compare.IsEnabled = false;
            if (captureButtonState) button_capture.IsEnabled = false;

            IProgress<string> textBoxUpdate = new Progress<string>(message =>
            {
                textbox_output.AppendText(message + Environment.NewLine);
            });
            string comparePath = textbox_compareFile.Text;

            RegistryKey[] keys = GetCheckedHives();
            await Task.Run(() => Compare(keys, comparePath, textBoxUpdate));

            button_compare.IsEnabled = true;
            button_capture.IsEnabled = captureButtonState;
        }

        private async void button_capture_Click(object sender, RoutedEventArgs e)
        {
            bool comapreButtonState = button_compare.IsEnabled;

            button_capture.IsEnabled = false;
            if (comapreButtonState) button_compare.IsEnabled = false;

            IProgress<string> textBoxUpdate = new Progress<string>(message =>
            {
                textbox_output.AppendText(message + Environment.NewLine);
            });
            string saveDirectory = textbox_saveDirectory.Text;

            RegistryKey[] keys = GetCheckedHives();
            await Task.Run(() => Snapshot(keys, textBoxUpdate, saveDirectory));

            button_capture.IsEnabled = true;
            button_compare.IsEnabled = comapreButtonState;
        }

        private void button_saveDirectory_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog ofd = new OpenFolderDialog
            {
                Title = "Select folder to save capture",
                Multiselect = false
            };

            bool? result = ofd.ShowDialog();
            if (result == true) {
                textbox_saveDirectory.Text = ofd.FolderName;
                button_capture.IsEnabled = true;
            }
        }

        private void button_compareFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Select file to compare against",
                Multiselect = false,
                Filter = "SQLite3 Files (*.sqlite3)|*.sqlite3"
            };

            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                textbox_compareFile.Text = ofd.FileName;
                button_compare.IsEnabled = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _dataManager.DetachDatabse();
            _dataManager.TerminateConnection();
            Application.Current.Shutdown();
        }
    }
}
