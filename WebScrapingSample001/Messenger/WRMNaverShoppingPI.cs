using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.Generic;
using WebScrapingSample001.Model;

namespace WebScrapingSample001.Messenger
{
    internal class WRMNaverShoppingPI : ValueChangedMessage<List<ProductInfo>>
    {
        public WRMNaverShoppingPI(List<ProductInfo> value) : base(value)
        {

        }
    }
}
