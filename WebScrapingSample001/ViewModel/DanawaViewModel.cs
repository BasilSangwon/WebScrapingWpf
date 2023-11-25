using AngleSharp;
using AngleSharp.Dom;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WebScrapingSample001.Messenger;
using WebScrapingSample001.Model;

namespace WebScrapingSample001.ViewModel
{
    internal class DanawaViewModel : ObservableObject
    {
        #region Properties
        private ObservableCollection<ProductInfo>? _productInfos;
        public ObservableCollection<ProductInfo>? ProductInfos
        {
            get => _productInfos;
            set => SetProperty(ref _productInfos, value);
        }

        public IRelayCommand<string> MainSearchCmd { get; set; }
        public string? MainSearch { get; set; }

        private string? _mainSearchLink;
        public string? MainSearchLink
        {
            get => _mainSearchLink;
            set => SetProperty(ref _mainSearchLink, value);
        }

        #endregion Properties


        public DanawaViewModel()
        {
            _productInfos = new ObservableCollection<ProductInfo>();

            WeakReferenceMessenger.Default.Register<WRMSearch>(this, OnWRMSearch);
            MainSearchCmd = new RelayCommand<string>(ExMainSearchCmd);
            MainSearch = "Danawa";
        }

        #region Command Function
        /// <summary>
        /// 1. Main Search Link 연결 함수
        /// </summary>
        /// <param name="url"></param>
        private void ExMainSearchCmd(string? url)
        {
            if (url != null)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = @"C:\Program Files\Google\Chrome\Application\chrome.exe", // 웹 브라우저 실행 파일 경로를 여기에 넣으세요
                        Arguments = url,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 1. 상품별 Link 연결 함수
        /// 2. 가격 정보 클릭시 해당 사이트로 이동
        /// 3. DanawaTaskAsync에서 상품만큼 생성함
        /// </summary>
        /// <param name="url"></param>
        private void ExLinkCmd(string? url)
        {
            if (url != null)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = @"C:\Program Files\Google\Chrome\Application\chrome.exe", // 웹 브라우저 실행 파일 경로를 여기에 넣으세요
                        Arguments = url,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        #endregion Command Function

        #region Weak Reference Messenger
        /// <summary>
        /// 1. 키 입력 후 Enter 이벤트 발생시 실행됨
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="message"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnWRMSearch(object recipient, WRMSearch message)
        {
            try
            {
                string text = message.Value;
                Task.Run(async () =>
                {
                    await DanawaTaskAsync(text);
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion Weak Reference Messenger

        /// <summary>
        /// 1. Danawa Site Scraping
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        private async Task DanawaTaskAsync(string search)
        {
            try
            {
                // 1. URL
                string url = $"https://search.danawa.com/dsearch.php?k1={search}&module=goods&act=dispMain";

                // 2. Main Search Button에 맵핑
                MainSearchLink = url;

                // 3. AngleSharp의 Configuration 객체 생성
                var config = Configuration.Default.WithDefaultLoader();

                // 4. BrowsingContext를 설정하여 웹 페이지에 대한 요청 및 파싱을 처리
                var context = BrowsingContext.New(config);
                var document = await context.OpenAsync(url);

                // 5. Query 선택, 자식 가져오기
                var container = document.QuerySelector(".product_list");
                var containerChildren = container.Children;

                // 6. 비동기로 처리할 작업 목록 생성
                var tasks = containerChildren
                    .Take(5)
                    .Select(async item =>
                    {
                        /*** Image ***/
                        // 제품 이미지 가져오기
                        var qThumbImage = item.QuerySelectorAll(".thumb_image");
                        string? imgUri = qThumbImage.Length > 0 ? await GetProductImage(qThumbImage.FirstOrDefault()?.OuterHtml) : null;

                        /*** Information ***/
                        // 1. 제품 이름 가져오기
                        string productName = item.QuerySelector("p.prod_name").TextContent.Trim();

                        // 2. 제품 설명 가져오기
                        string description = await GetDescription(item);

                        /*** Price ***/
                        // Price
                        var qPriceList = item.QuerySelectorAll(".prod_pricelist");
                        List<InnerItem> innerItems = await GetPrice(qPriceList.FirstOrDefault()?.OuterHtml);

                        // UI 맵핑
                        return new ProductInfo()
                        {
                            ProducName = productName,
                            InnerItems = new ObservableCollection<InnerItem>(innerItems),
                            ImgUri = imgUri,
                            Description = description,
                        };
                    });

                // 7. 비동기 작업 병렬 실행
                var productInfos = await Task.WhenAll(tasks);

                // 8. Danawa 정보 보내기
                WeakReferenceMessenger.Default.Send(new WRMDanawaPI(productInfos.ToList()));

                // 9. ObservableCollection 루프에서 사용시 이터레이터를 다 돌지 않음
                ProductInfos = new ObservableCollection<ProductInfo>(productInfos);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }




        #region Html Parse Function
        /// <summary>
        /// 1. Html Parser 하여 제품 이미지 가져오기
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private async Task<string?> GetProductImage(string htmlString)
        {
            try
            {
                var context = BrowsingContext.New(Configuration.Default);
                var document = await context.OpenAsync(req => req.Content(htmlString));

                // img 태그
                var imgElement = document.QuerySelector("img");
                // JavaScript 이벤트를 직접적으로 처리할 수 없어 data-src를 사용하고, 기본적으로 src를 백업으로 사용
                string srcValue = imgElement?.GetAttribute("data-src") ?? imgElement?.GetAttribute("src");

                // https 문자열 삽입
                return srcValue != null ? $"https:{srcValue}" : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                // 오류가 발생했을 때 기본값으로 null을 반환
                return null;
            }
        }

        /// <summary>
        /// 1. 제품의 설명을 추가한다.
        /// 2. 제품 설명이 없으면 빈공란으로 납둔다.
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private async Task<string?> GetDescription(IElement item)
        {
            try
            {
                var qdescription = item.QuerySelectorAll(".spec_list");

                StringBuilder sb = new StringBuilder();
                foreach (var items in qdescription)
                {
                    string originalString = items.TextContent;
                    string stringWithoutNewLineAndTab = new string(originalString.Where(c => c != '\n' && c != '\t').ToArray());
                    sb.Append(stringWithoutNewLineAndTab);
                    sb.Append(" / ");
                }

                return sb.ToString() != null ? sb.ToString() : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 1. 제품의 가격을 Html에서 Parser한다.
        /// 2. 아이콘 유무를 판단하여 반영 한다.
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        private async Task<List<InnerItem>?> GetPrice(string htmlString)
        {
            try
            {
                var context = BrowsingContext.New(Configuration.Default);
                var document = await context.OpenAsync(req => req.Content(htmlString));

                var TagElements = document.QuerySelectorAll(".prod_pricelist li");

                string price = string.Empty;
                string link = string.Empty;
                string iconLink = string.Empty;
                string memory = string.Empty;
                List<InnerItem> innerItems = new List<InnerItem>();

                foreach (var item in TagElements)
                {
                    // Mall Icon
                    var iconElement = item?.QuerySelector("p.mall_icon img");


                    // 아이콘 없는 것
                    if (iconElement == null)
                    {
                        // Price
                        var priceElement = item?.QuerySelector(".price_sect a");
                        price = priceElement.TextContent.Trim();

                        // Link
                        link = priceElement?.GetAttribute("href");

                        // Memory
                        var memoryElemnet = item?.QuerySelector(".memory_sect");
                        string orginal = memoryElemnet?.TextContent.Trim();
                        memory = new string(orginal.Where(c => c != '\n' && c != '\t').ToArray());

                    }
                    // 아이콘 있는 것
                    else
                    {
                        // Price
                        var priceElement = item?.QuerySelector(".price_sect");
                        price = priceElement.TextContent.Trim();

                        // Link
                        var linkElement = item?.QuerySelector("a");
                        link = linkElement?.GetAttribute("href");

                        // Icon
                        iconLink = $"https:{iconElement?.GetAttribute("src")}";
                    }

                    // List<InnterItem>
                    innerItems.Add(new InnerItem()
                    {
                        Memory = memory,
                        Link = link,
                        Price = price,
                        Mall_icon = iconLink,
                        LinkCmd = new RelayCommand<string>(ExLinkCmd)
                    });
                }

                return innerItems != null ? innerItems : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                // 오류가 발생했을 때 기본값으로 null을 반환
                return null;
            }
        }

        #endregion Html Parse Function
    }
}
