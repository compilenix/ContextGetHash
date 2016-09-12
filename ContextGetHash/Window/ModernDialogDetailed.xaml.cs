using FirstFloor.ModernUI.Windows.Controls;
using System.Windows;
using System.Linq;
using System.Windows.Media;

namespace ContextGetHash.Window {
    public partial class ModernDialogDetailed : ModernDialog {
        public ModernDialogDetailed() {
            InitializeComponent();

            this.OkButton.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            this.CancelButton.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
            this.CloseButton.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 0, 122, 204));
        }

        private void CheckBox_Checked(System.Object sender, RoutedEventArgs e) {
            this.textBoxExtendedMessage.Visibility = Visibility.Visible;
            this.buttonCopy.Visibility = Visibility.Visible;

            AdjustGrid();
        }

        private void CheckBox_Unchecked(System.Object sender, RoutedEventArgs e) {
            this.textBoxExtendedMessage.Visibility = Visibility.Hidden;
            this.buttonCopy.Visibility = Visibility.Hidden;

            AdjustGrid();
        }

        private void AdjustGrid() {
            this.Grid.RowDefinitions.ElementAt(2).Height = new GridLength(this.textBoxExtendedMessage.ActualHeight + this.textBoxExtendedMessage.Margin.Top + this.textBoxExtendedMessage.Margin.Bottom);
            this.Grid.RowDefinitions.ElementAt(3).Height = new GridLength(this.buttonCopy.ActualHeight + this.buttonCopy.Margin.Top + this.buttonCopy.Margin.Bottom);
        }

        public static void ShowMessage(string Title, string Message, string DetailedMessage) {
            ModernDialogDetailed Dialog = new ModernDialogDetailed();
            Dialog.InitializeComponent();
            Dialog.Title = Title;
            Dialog.textBlockMessage.Text = Message;
            Dialog.textBoxExtendedMessage.Text = DetailedMessage;
            Dialog.Owner = Application.Current.Windows[0];
            Dialog.ShowDialog();
        }

        private void buttonCopy_Click(System.Object sender, RoutedEventArgs e) {
            Clipboard.SetText(this.textBoxExtendedMessage.Text);
        }
    }
}
