using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WebScrapingSample001.Messenger
{
    internal class WRMSearch : ValueChangedMessage<string>
    {
        public WRMSearch(string value) : base(value)
        {

        }
    }
}
