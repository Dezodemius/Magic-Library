using System.Collections.Generic;
using Library.Client.Model;

namespace Library.Client.ViewModel
{
  public class HighlightsViewModel : ViewModel
  {
    public override string Name { get; } = "Я хз как правильно перевести Highlights, но это они, короче";

    public List<HighlightWithPages> SelectedBook { get; set; }
  }
}