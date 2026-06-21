using CommunityToolkit.Mvvm.ComponentModel;

namespace BeanTracker.MAUI.Features.Admin;

public sealed partial class AdminViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = "Admin Dashboard";

    public AdminViewModel()
    {
    }
}
