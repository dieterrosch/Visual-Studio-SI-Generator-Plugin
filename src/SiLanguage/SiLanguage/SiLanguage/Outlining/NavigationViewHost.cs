using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace SiLanguage.Navigation
{
    [Export(typeof(IWpfTextViewMarginProvider))]
    [Name("Navigation")]
    [Order(After = PredefinedMarginNames.Top)]
    [MarginContainer(PredefinedMarginNames.Top)]
    [ContentType("si")]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class NavigationViewHost : IWpfTextViewMarginProvider
    {
        public IWpfTextViewMargin CreateMargin(IWpfTextViewHost textViewHost, IWpfTextViewMargin containerMargin)
        {
            var nameLine = textViewHost.TextView.TextSnapshot.Lines.FirstOrDefault(x => x.GetText().Contains("Name:"));
            var name = "";
            if (nameLine != null)
                name = nameLine.GetText().Split(':')[1].Split('.')[0];
            return textViewHost.TextView.Properties.GetOrCreateSingletonProperty<Navigation>("Navigation", delegate { return new Navigation(this.NavigateTo, textViewHost.TextView, name); });

        }

        public bool NavigateTo(ITextSnapshotLine snapshotSpan, ITextView TextView)
        {
            IWpfTextView wpfTextView = TextView as IWpfTextView;
            if (wpfTextView == null)
                return false;
            wpfTextView.VisualElement.Dispatcher.BeginInvoke(new Action(delegate()
            {
                var navSnapShot = TextView.TextSnapshot.Lines.FirstOrDefault(x => x.GetText() == snapshotSpan.GetText());

                if (navSnapShot != null)
                {
                    TextView.Caret.MoveTo(navSnapShot.Extent.Start);
                    TextView.Selection.Select(navSnapShot.Extent, false);
                    TextView.ViewScroller.EnsureSpanVisible(navSnapShot.Extent, EnsureSpanVisibleOptions.ShowStart);
                    Keyboard.Focus((IInputElement)wpfTextView.VisualElement);
                }
            }), (object[])null);

            return true;
        }
    }
}
