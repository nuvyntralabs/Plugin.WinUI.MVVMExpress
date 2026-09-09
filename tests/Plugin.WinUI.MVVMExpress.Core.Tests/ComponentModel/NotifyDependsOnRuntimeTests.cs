using Plugin.WinUI.MVVMExpress.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.ComponentModel;

public sealed class NotifyDependsOnRuntimeTests
{
    [Fact]
    public void NotifyDependsOn_RaisesComputedProperty()
    {
        var model = new NameModel();
        var seen = new List<string>();
        model.PropertyChanged += (_, e) => seen.Add(e.PropertyName ?? "");
        model.First = "Ada";
        Assert.Contains(nameof(NameModel.First), seen);
        Assert.Contains(nameof(NameModel.FullName), seen);
        seen.Clear();
        model.Last = "Lovelace";
        Assert.Contains(nameof(NameModel.Last), seen);
        Assert.Contains(nameof(NameModel.FullName), seen);
        Assert.Equal("Ada Lovelace", model.FullName);
    }

    private sealed class NameModel : ObservableModel
    {
        private string _first = "";
        private string _last = "";

        public string First
        {
            get => _first;
            set
            {
                if (SetProperty(ref _first, value))
                {
                    NotifyDependsOn(nameof(First), nameof(FullName));
                }
            }
        }

        public string Last
        {
            get => _last;
            set
            {
                if (SetProperty(ref _last, value))
                {
                    NotifyDependsOn(nameof(Last), nameof(FullName));
                }
            }
        }

        public string FullName => $"{First} {Last}".Trim();
    }
}
