using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Windows;
using WebScrapingSample001.Messenger;

namespace WebScrapingSample001.ViewModel
{
    internal class NavigationViewModel
    {
        #region Properties
        public RelayCommand<string>? KeyDownCmd { get; set; }
        #endregion Properties

        public NavigationViewModel()
        {
            KeyDownCmd = new RelayCommand<string>(ExKeyDownCmd);
        }

        /// <summary>
        /// 1. KeyDown 이벤트로 'Enter' 키 입력시 실행 됨
        /// </summary>
        /// <param name="text"></param>
        private void ExKeyDownCmd(string? text)
        {
            try
            {
                if (text == string.Empty || text == null)
                {
                    MessageBox.Show("Please type the word you want to search.");
                    return;
                }


                if (text?.Length >= 20)
                {
                    MessageBox.Show("Please enter no more than 20 characters.");
                    return;
                }


                // Danawa 검색 Text 전달
                WeakReferenceMessenger.Default.Send(new WRMSearch(text));

                // Naver Shopping 검색 Text 전달

                // Enuri 검색 Text 전달
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
