using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml;
using FastBoard.ViewModels;
using FastBoard.Core.Models;
using System.Collections.Generic;

namespace FastBoard.Controls
{
    public sealed partial class CourtView
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CourtViewAutomationPeer(this);
        }
    }

    internal sealed class CourtViewAutomationPeer : FrameworkElementAutomationPeer
    {
        public CourtViewAutomationPeer(FrameworkElement owner) : base(owner) { }

        protected override IList<AutomationPeer> GetChildrenCore()
        {
            var view = (CourtView)Owner;
            var list = new List<AutomationPeer>();
            if (view.DataContext is BoardViewModel vm)
            {
                foreach (var s in vm.Shapes)
                {
                    if (s is Token t)
                    {
                        list.Add(new TokenAutomationPeer(t, view));
                    }
                }
            }
            return list;
        }

        protected override string GetClassNameCore() => "CourtView";
        protected override string GetNameCore() => "Basketball Court";
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Pane;
    }

    internal sealed class TokenAutomationPeer : AutomationPeer
    {
        private readonly Token _token;
        private readonly CourtView _view;
        public TokenAutomationPeer(Token token, CourtView view) { _token = token; _view = view; }
        protected override string GetNameCore()
        {
            var n = _token.Number != 0 ? $" {_token.Number}" : string.Empty;
            var img = string.IsNullOrEmpty(_token.ImagePath) ? "Circle" : System.IO.Path.GetFileNameWithoutExtension(_token.ImagePath);
            return $"Token{n} {img}";
        }
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Image;
        protected override IList<AutomationPeer> GetChildrenCore() => new List<AutomationPeer>();
        protected override string GetClassNameCore() => "Token";
    }
}
