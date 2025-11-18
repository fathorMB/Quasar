namespace Quasar.Ui.Navigation;

public sealed class QuasarUiNavSection
{
    public QuasarUiNavSection(string title)
    {
        Title = title;
    }

    public string Title { get; }

    public IList<QuasarUiNavItem> Items { get; } = new List<QuasarUiNavItem>();
}
