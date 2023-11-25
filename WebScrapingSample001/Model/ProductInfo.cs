using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace WebScrapingSample001.Model
{
    public class ProductInfo
    {
        public string? ProducName { get; set; }
        public string? Logo { get; set; }
        public string? MainSearch { get; set; }
        public string? Benefit { get; set; }
        public string? Price { get; set; }
        public string? ImgUri { get; set; }
        public string? Description { get; set; }
        public ObservableCollection<InnerItem>? InnerItems { get; set; }
    }

    public class InnerItem
    {
        public string? Memory { get; set; }
        public string? Price { get; set; }
        public string? Link { get; set; }
        public string? Mall_icon { get; set; }
        public RelayCommand<string>? LinkCmd { get; set; }
    }
}
