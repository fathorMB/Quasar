using System.Collections.Generic;
using System.Linq;

namespace Quasar.Ui.Navigation;

public sealed class QuasarUiNavigationOptions
{
    public IList<QuasarUiNavSection> Sections { get; } = new List<QuasarUiNavSection>();

    public QuasarUiNavItem? DefaultItem => Sections.SelectMany(section => section.Items).FirstOrDefault(item => item.IsDefault) ?? Sections.SelectMany(section => section.Items).FirstOrDefault();
}
