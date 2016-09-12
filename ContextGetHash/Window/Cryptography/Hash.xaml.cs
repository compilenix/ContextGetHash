using FirstFloor.ModernUI.Windows.Controls;
using HashLib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ContextGetHash.Window.Cryptography {
    public partial class Hash : Page,IDisposable {
        private CoreTools.Hash CoreToolsHash;
        private CoreTools.Hash.CalculateDelegateBytes CalculateBytes;
        private CoreTools.Hash.CalculateDelegateFile CalculateFile;
        private BackgroundWorker BackgroundWorker, BackgroundWorkerReadFile;
        private byte[] ContentResult;
        private string FileName;
        private int Rounds;
        private bool HashFile, _disposed;
        private FileStream FileStream;
        private ReadOnlyCollection<Type> SetOfHashes;

        public Hash() {
            InitializeComponent();

            this.Rounds = 1;
            
            this.CoreToolsHash = new CoreTools.Hash();
            this.CalculateBytes = new CoreTools.Hash.CalculateDelegateBytes(CoreToolsHash.Calculate);
            this.CalculateFile = new CoreTools.Hash.CalculateDelegateFile(CoreToolsHash.Calculate);

            this.BackgroundWorker = new BackgroundWorker();
            this.BackgroundWorker.DoWork += new DoWorkEventHandler(this.BackgroundWorker_DoWork);
            this.BackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(this.BackgroundWorker_ProgressChanged);
            this.BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BackgroundWorker_RunWorkerCompleted);
            this.BackgroundWorker.WorkerReportsProgress = true;
            this.BackgroundWorker.WorkerSupportsCancellation = true;

            this.BackgroundWorkerReadFile = new BackgroundWorker();
            this.BackgroundWorkerReadFile.DoWork += new DoWorkEventHandler(this.BackgroundWorkerReadFile_DoWork);

            this.comboBoxHashSet.SelectedIndex = 0; // All
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this._disposed) {
                if (disposing) {
                    BackgroundWorkerReadFile.Dispose();
                    BackgroundWorker.Dispose();
                }
                this._disposed = true;
            }
        }

        private void SetSetOfHashes(string Set) {
            switch (Set) {
                case "All":
                    this.SetOfHashes = HashLib.Hashes.All;
                    break;
                case "AllUnique":
                    this.SetOfHashes = HashLib.Hashes.AllUnique;
                    break;
                case "Checksums":
                    this.SetOfHashes = HashLib.Hashes.Checksums;
                    break;
                case "CryptoAll":
                    this.SetOfHashes = HashLib.Hashes.CryptoAll;
                    break;
                case "CryptoBuildIn":
                    this.SetOfHashes = HashLib.Hashes.CryptoBuildIn;
                    break;
                case "CryptoNotBuildIn":
                    this.SetOfHashes = HashLib.Hashes.CryptoNotBuildIn;
                    break;
                case "FastComputes":
                    this.SetOfHashes = HashLib.Hashes.FastComputes;
                    break;
                case "Hash32":
                    this.SetOfHashes = HashLib.Hashes.Hash32;
                    break;
                case "Hash64":
                    this.SetOfHashes = HashLib.Hashes.Hash64;
                    break;
                case "HMACCryptoBuildIn":
                    this.SetOfHashes = HashLib.Hashes.HMACCryptoBuildIn;
                    break;
                case "NonBlock":
                    this.SetOfHashes = HashLib.Hashes.NonBlock;
                    break;
                default:
                    this.SetOfHashes = HashLib.Hashes.All;
                    break;
            }

            this.comboBoxHashes.ItemsSource = this.SetOfHashes;
            this.comboBoxHashes.SelectedIndex = 0;
        }

        private void SwitchUiToBusyMode() {
            this.ButtonCancel.IsEnabled = true;
            this.ButtonCancel.Foreground = Brushes.White;

            this.comboBoxHashes.IsEnabled = false;
            this.comboBoxHashSet.IsEnabled = false;
            this.TextBoxNumeric.IsEnabled = false;
            this.ButtonGoFile.IsEnabled = false;
            this.ButtonGoText.IsEnabled = false;
            this.ButtonOpenFile.IsEnabled = false;
            this.TextBoxFileInput.IsEnabled = false;
            this.TextBoxInput.IsEnabled = false;
            _showProgressbarAsync();
        }

        async void _showProgressbarAsync() {
            await Task.Delay(500);
            
            if (this.ProgressBar.Visibility != Visibility.Visible && (this.BackgroundWorker.IsBusy || this.BackgroundWorkerReadFile.IsBusy)) {
                showProgressbar();
            }
        }

        private void showProgressbar() {
            this.ProgressBar.IsIndeterminate = true;
            this.ProgressBar.Visibility = Visibility.Visible;
            this.ProgressBar.Value = 0;

            this.TextBlockProgress.Visibility = Visibility.Visible;
            this.TextBlockProgress.Text = "";

            this.Grid.RowDefinitions.ElementAt(this.Grid.RowDefinitions.Count - 1).Height = new GridLength(43);

            for (int i = 0; i < 43; i++) {
                Application.Current.MainWindow.Height += 1;
            }
            //Application.Current.MainWindow.Height += 43;
        }

        private void hideProgressBar() {
            this.ProgressBar.Visibility = Visibility.Hidden;
            this.ProgressBar.IsIndeterminate = true;
            this.ProgressBar.Value = 0;

            this.TextBlockProgress.Visibility = Visibility.Hidden;
            this.TextBlockProgress.Text = "";

            this.Grid.RowDefinitions.ElementAt(this.Grid.RowDefinitions.Count - 1).Height = new GridLength(0);
            for (int i = 43; i > 0; i--) {
                Application.Current.MainWindow.Height -= 1;
            }
            //Application.Current.MainWindow.Height -= 43;
        }

        private bool checkTextBoxNumeric() {
            try {
                this.Rounds = Int32.Parse(this.TextBoxNumeric.Text);

                if (this.Rounds > 0) {
                    this.TextBoxNumeric.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
                    return true;
                } else {
                    this.TextBoxNumeric.BorderBrush = new SolidColorBrush(Colors.Red);
                    return false;
                }
            } catch (Exception ex) {
                this.TextBoxNumeric.BorderBrush = new SolidColorBrush(Colors.Red);
                ModernDialogDetailed.ShowMessage(ex.GetType().Name, ex.Message, ex.StackTrace);
                return false;
            }
        }

        private void DoText() {
            this.ContentResult = this.CoreToolsHash.Calculate(this.ContentResult, this.CoreToolsHash.HashAlgorithm);
        }

        private void DoFileStream() {
            long FileStreamLength = this.FileStream.Length;
            long Position = 0;
            long x = FileStreamLength / 100;
            
            this.BackgroundWorkerReadFile.RunWorkerAsync();

            while (this.BackgroundWorkerReadFile.IsBusy) {
                Position = this.FileStream.Position;
                if (Position >= x) {
                    x = Position + (FileStreamLength / 100);
                    this.BackgroundWorker.ReportProgress((int) (((float)Position) / ((float)FileStreamLength) * 100));
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        private void BackgroundWorkerReadFile_DoWork(Object sender, DoWorkEventArgs e) {
            this.ContentResult = this.CoreToolsHash.Calculate(this.FileStream, this.CoreToolsHash.HashAlgorithm);
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            if (this.ProgressBar.IsIndeterminate) {
                this.ProgressBar.IsIndeterminate = false;
            }

            if (this.BackgroundWorkerReadFile.IsBusy) {
                this.TextBlockProgress.Text = "Reading file";
            } else {
                this.TextBlockProgress.Text = "Calculate Hash";
            }

            this.ProgressBar.Value = e.ProgressPercentage;
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            if (this.Rounds > 1) {
                
                if (this.HashFile) {
                    DoFileStream();
                }

                int y = this.Rounds / 100;
                for (int i = 0; i < this.Rounds; i++) {
                    if (this.BackgroundWorker.CancellationPending == true) {
                        e.Cancel = true;
                        break;
                    }

                    DoText();

                    if (i >= y) {
                        y += Rounds / 100;
                        this.BackgroundWorker.ReportProgress((i * 100) / this.Rounds);
                    }
                }
            } else if (this.Rounds == 1) {
                if (this.HashFile) {
                    DoFileStream();
                } else {
                    DoText();
                }
            } else {
                this.ContentResult = null;
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            try {
                this.TextBoxTextResult.Text = CoreTools.Converter.ToHex(this.ContentResult);
            } catch {
                this.TextBoxTextResult.Text = "Error!";
            }

            if (this.ProgressBar.Visibility == Visibility.Visible) {
                hideProgressBar();
            }

            if (e.Cancelled == true) {
                this.TextBoxTextResult.Text = "Canceled!";
            }

            if (this.BackgroundWorkerReadFile.IsBusy) {
                this.BackgroundWorkerReadFile.Dispose();
            }

            if (this.FileStream != null) {
                this.FileStream.Dispose();
            }

            this.ContentResult = null;
            
            this.ButtonCancel.IsEnabled = false;
            this.ButtonCancel.Foreground = Brushes.DarkGray;
            this.comboBoxHashes.IsEnabled = true;
            this.comboBoxHashSet.IsEnabled = true;
            this.TextBoxNumeric.IsEnabled = true;
            this.ButtonGoFile.IsEnabled = true;
            this.ButtonGoText.IsEnabled = true;
            this.ButtonOpenFile.IsEnabled = true;
            this.TextBoxFileInput.IsEnabled = true;
            this.TextBoxInput.IsEnabled = true;
        }

        private void TextBoxNumeric_LostFocus(object sender, RoutedEventArgs e) {
            checkTextBoxNumeric();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            this.BackgroundWorker.CancelAsync();
        }

        private void ButtonOpenFile_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog OpenDialog = new Microsoft.Win32.OpenFileDialog();

            Nullable<bool> result = OpenDialog.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true) {
                // Open document 
                this.TextBoxFileInput.Text = this.FileName = OpenDialog.FileName;
            }
        }

        private void TextBoxFileInput_Drop(object sender, DragEventArgs e) {
            this.TextBoxFileInput.Text = this.FileName = ((DataObject)e.Data).GetFileDropList()[0];
        }

        private void comboBoxHashes_SelectionChanged(Object sender, SelectionChangedEventArgs e) {
            if (this.comboBoxHashes.SelectedIndex < 0 || this.comboBoxHashes.SelectedIndex >= this.SetOfHashes.Count) {
                this.CoreToolsHash.HashAlgorithm = (IHash)Activator.CreateInstance(this.SetOfHashes.ElementAt(0));
            } else {
                this.CoreToolsHash.HashAlgorithm = (IHash)Activator.CreateInstance(this.SetOfHashes.ElementAt(this.comboBoxHashes.SelectedIndex));
            }
        }

        private void comboBoxHashSet_SelectionChanged(Object sender, SelectionChangedEventArgs e) {
            SetSetOfHashes((string) (((ComboBoxItem) this.comboBoxHashSet.SelectedItem).Content));
        }

        private void ButtonGoText_Click(object sender, RoutedEventArgs e) {
            if (!BackgroundWorker.IsBusy && checkTextBoxNumeric()) {
                this.HashFile = false;
                this.ContentResult = CoreTools.Hash.DefaultEncoding.GetBytes(this.TextBoxInput.Text);
                this.BackgroundWorker.RunWorkerAsync(this.CalculateBytes);
                SwitchUiToBusyMode();
            }
        }

        private void ButtonGoFile_Click(object sender, RoutedEventArgs e) {
            if (!BackgroundWorker.IsBusy && checkTextBoxNumeric()) {
                this.HashFile = true;

                try {
                    this.FileStream = CoreTools.IOutils.FileCanRead(this.FileName);
                    this.BackgroundWorker.RunWorkerAsync(this.CalculateFile);
                    SwitchUiToBusyMode();
                } catch (Exception ex) {
                    ModernDialog.ShowMessage(ex.Message + "\n\n" + ex.StackTrace, ex.GetType().Name, MessageBoxButton.OK);
                }
            }
        }

        private void Page_Drop(object sender, DragEventArgs e)
        {
            this.TextBoxFileInput_Drop(sender, e);
        }
    }
}
