using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SiLanguage.Navigation
{
    /// <summary>
    /// Interaction logic for Navigation.xaml
    /// </summary>
    public partial class Navigation : UserControl, IWpfTextViewMargin
    {
        private bool _isDisposed = false;
        private Func<ITextSnapshotLine, ITextView, bool> _navigate;
        private ITextView _textView;
        private string _siName;

        public static Dictionary<string, List<NavItem>> NavItems { get; set; }

        public Navigation(Func<ITextSnapshotLine, ITextView, bool> navigate, ITextView TextView, string siName)
        {
            _navigate = navigate;
            DataContext = this;
            _textView = TextView;
            _siName = siName;
            InitializeComponent();
        }

        public NavItem SelectedNavItem
        {
            get { return null; }
            set
            {
                _navigate(value.Snapshot, _textView);
            }
        }

        public ObservableCollection<NavItem> NavItemsLocal
        {
            get { return NavItems == null || !NavItems.ContainsKey(_siName) ? new ObservableCollection<NavItem>() : new ObservableCollection<NavItem>(NavItems[_siName]); }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Navigation");
            }
        }

        public FrameworkElement VisualElement
        {
            get
            {
                ThrowIfDisposed();
                return this;
            }
        }

        public bool Enabled
        {
            get
            {
                ThrowIfDisposed();
                return ActualWidth > 0;
            }
        }

        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return (marginName == "Navigation") ? (IWpfTextViewMargin)this : null;
        }

        public double MarginSize
        {
            get
            {
                ThrowIfDisposed();
                return ActualWidth;
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        private void combobox_Loaded(object sender, RoutedEventArgs e)
        {
            Popup popup = FindVisualChildByName<Popup>((sender as DependencyObject), "PART_Popup");
            Border border = FindVisualChildByName<Border>(popup.Child, "DropDownBorder");
            border.Background = Brushes.Lavender;
        }

        private T FindVisualChildByName<T>(DependencyObject parent, string name) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                string controlName = child.GetValue(Control.NameProperty) as string;
                if (controlName == name)
                {
                    return child as T;
                }
                else
                {
                    T result = FindVisualChildByName<T>(child, name);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }
    }

    public class NavItem
    {
        public string Name { get; set; }
        public int Line { get; set; }
        public ITextSnapshotLine Snapshot { get; set; }
    }
}
