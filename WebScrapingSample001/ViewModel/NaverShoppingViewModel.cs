using AngleSharp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using WebScrapingSample001.Messenger;
using WebScrapingSample001.Model;

namespace WebScrapingSample001.ViewModel
{
    internal class NaverShoppingViewModel : ObservableObject
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

        public NaverShoppingViewModel()
        {
            _productInfos = new ObservableCollection<ProductInfo>();

            WeakReferenceMessenger.Default.Register<WRMSearch>(this, OnWRMSearch);
            MainSearchCmd = new RelayCommand<string>(ExMainSearchCmd);
            MainSearch = "Naver Shopping";
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
        private void OnWRMSearch(object recipient, WRMSearch message)
        {
            try
            {
                string text = message.Value;
                Task.Run(async () =>
                {
                    await NaverShoppingTaskAsync(text);
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion Weak Reference Messenger

        #region Main Function
        /// <summary>
        /// 1. Naver Shopping site Scraping
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        private async Task NaverShoppingTaskAsync(string search)
        {
            try
            {
                // 1. URL
                string url = $"https://search.shopping.naver.com/search/all?query={search}&cat_id=&frm=NVSHATC";

                // 2. Main Search Button에 맵핑
                MainSearchLink = url;

                // 3. AngleSharp의 Configuration 객체 생성
                var config = Configuration.Default.WithDefaultLoader();

                // 4. BrowsingContext를 설정하여 웹 페이지에 대한 요청 및 파싱을 처리
                var context = BrowsingContext.New(config);
                var document = await context.OpenAsync(url);

                // 5. 네이버 쇼핑 검색시 가장 처음에 보이는 태그에 대한 ID, 링크 정보를 가져온다. (처음은 5개만 출력됨)
                List<NaverParser> firstNP = document.QuerySelectorAll(".thumbnail_thumb__Bxb6Z")
                    .Select(element => new NaverParser
                    {
                        Id = element.GetAttribute("data-i"),        // Product ID로 스크립트에서 정보를 찾을때 필요함
                        AdcrUrl = element.GetAttribute("href")      // 사이트 링크
                    })
                    .ToList();

                // 6. 이 스크립트는 Json형태로 되어있으며, 모든 정보가 다 들어있음
                var scriptElement = document.All
                   .FirstOrDefault(element => element.Id == "__NEXT_DATA__");

                // 6-1 예외처리
                if (scriptElement == null)
                    throw new Exception("script is Null");

                // 7. html script 필요한 값 추출하여 맵핑
                var jsonData = scriptElement.TextContent.Trim();
                var jsonObject = JObject.Parse(jsonData); // Parse JSON data
                var jtoken = jsonObject["props"]?["pageProps"]?["initialState"]?["products"]?["list"]; // 필요한 값을 추출
                List<NaverParser> secondNP = SetJsonInfoParser(jtoken);

                // 8. thirdNP는 secondNP(데이터 10개)에서 firstNP(데이터 5개) id값이 동일한 경우에만 추출한다.
                List<NaverParser> thirdNP = secondNP
                    .Join(firstNP,
                          nItem => nItem.Id,
                          firstItem => firstItem.Id,
                          (nItem, firstItem) => { nItem.AdcrUrl = firstItem.AdcrUrl; return nItem; })
                    .ToList();

                // 9. 마지막 데이터 맵핑 
                List<ProductInfo> productInfos = new List<ProductInfo>();
                int nCount = 0;
                foreach (var item in thirdNP)
                {
                    if (nCount == 5)
                        break;

                    productInfos.Add(new ProductInfo()
                    {
                        ProducName = item.productName,
                        ImgUri = item.ImageUrl,
                        Description = item.CharacterValue,
                        InnerItems = new ObservableCollection<InnerItem>()
                            {
                                new InnerItem()
                                {
                                    Link = item.AdcrUrl,
                                    LinkCmd = new RelayCommand<string>(ExLinkCmd),
                                    Price = item.Price,
                                    Mall_icon = item.MallLogos_basic
                                }
                            }
                    });

                    nCount++;
                }

                // 10. Naver Shopping 정보 보내기
                WeakReferenceMessenger.Default.Send(new WRMNaverShoppingPI(productInfos));

                // 11. ObservableCollection 루프에서 사용시 이터레이터를 다 돌지 않음
                ProductInfos = new ObservableCollection<ProductInfo>(productInfos);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion Main Function

        #region Html Parse Function


        /// <summary>
        /// 1. FirstNP 데이터 맵핑
        /// 2. Html Script에는 많은 데이터가 있어 10개만 가져온다.
        /// 3. Naver Shopping 처음 출력하였을때 기본 5개만 가져온다.
        /// </summary>
        /// <param name="jToken"></param>
        /// <returns></returns>
        private List<NaverParser> SetJsonInfoParser(JToken jToken)
        {
            return jToken?.Take(10).Select(item => new NaverParser
            {
                Brand = item?["item"]?["brand"]?.ToString(),
                ImageUrl = item?["item"]?["imageUrl"]?.ToString(),
                Id = item?["item"]?["id"]?.ToString(),
                Rank = item?["item"]?["rank"]?.ToString(),
                AdcrUrl = item?["item"]?["adcrUrl"]?.ToString(),
                Price = GetFormatPrice(item?["item"]?["price"]?.ToString()), // "," 및 "원" 추가
                productName = item?["item"]?["productName"]?.ToString(),
                CharacterValue = item?["item"]?["characterValue"]?.ToString(),
                MallLogos_basic = item?["item"]?["mallInfoCache"]?["mallLogos"]?["BASIC"]?.ToString() // 로고
            }).ToList() ?? new List<NaverParser>();
        }

        /// <summary>
        /// 1. , 및 원 글자 추가
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetFormatPrice(string value)
        {
            if (int.TryParse(value, out int numericValue))
                return $"{numericValue.ToString("N0")}원";

            return null;
        }

        #endregion Html Parse Function
    }
}
