using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using WebScrapingSample001.Messenger;
using WebScrapingSample001.Model;

namespace WebScrapingSample001.ViewModel
{
    internal class AllViewModel : ObservableObject
    {
        #region Properties

        private string? _quasarZonBrowserUrl;
        public string? QuasarZonBrowserUrl
        {
            get => _quasarZonBrowserUrl;
            set => SetProperty(ref _quasarZonBrowserUrl, value);
        }

        private ObservableCollection<ProductInfo>? _danawaPI;
        public ObservableCollection<ProductInfo>? DanawaPI
        {
            get => _danawaPI;
            set => SetProperty(ref _danawaPI, value);
        }

        private ObservableCollection<ProductInfo>? _naverShoppingPI;
        public ObservableCollection<ProductInfo>? NaverShoppingPI
        {
            get => _naverShoppingPI;
            set => SetProperty(ref _naverShoppingPI, value);
        }

        #endregion Properties



        public AllViewModel()
        {
            WeakReferenceMessenger.Default.Register<WRMSearch>(this, OnWRMSearch);
            WeakReferenceMessenger.Default.Register<WRMDanawaPI>(this, OnWRMDanawaPI);
            WeakReferenceMessenger.Default.Register<WRMNaverShoppingPI>(this, OnWRMNaverShoppingPI);
        }




        #region Weak Reference Messanger

        private void OnWRMSearch(object recipient, WRMSearch message)
        {
            try
            {
                QuasarZonBrowserUrl = $"https://quasarzone.com/groupSearches?keyword={message.Value}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 1. Danawa Product Information 정보 받기
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="message"></param>
        private void OnWRMDanawaPI(object recipient, WRMDanawaPI message)
        {
            try
            {
                DanawaPI = new ObservableCollection<ProductInfo>(message.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 1. Naver Shopping Product Information 정보 받기
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="message"></param>
        private void OnWRMNaverShoppingPI(object recipient, WRMNaverShoppingPI message)
        {
            try
            {
                NaverShoppingPI = new ObservableCollection<ProductInfo>(message.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        #endregion Weak Reference Messanger
    }
}
