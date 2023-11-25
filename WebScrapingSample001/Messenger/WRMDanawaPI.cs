using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.Generic;
using WebScrapingSample001.Model;

namespace WebScrapingSample001.Messenger
{
    internal class WRMDanawaPI : ValueChangedMessage<List<ProductInfo>>
    {
        public WRMDanawaPI(List<ProductInfo> value) : base(value)
        {
        }
    }
}
