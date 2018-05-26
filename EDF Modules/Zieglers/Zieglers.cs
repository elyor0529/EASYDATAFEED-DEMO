using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Scraper.Shared;
using System.Web;
using HtmlAgilityPack;
using Zieglers;
using Databox.Libs.Zieglers;
using Newtonsoft.Json;

namespace WheelsScraper
{
    public sealed class Zieglers : BaseScraper
    {

        public Zieglers()
        {
            Name = "Zieglers";
            Url = "http://www.zieglers.com";
            PageRetriever.Referer = Url;
            WareInfoList = new List<ExtWareInfo>();
            Wares.Clear();
            SpecialSettings = new ExtSettings();
        }

        private ExtSettings extSett
        {
            get
            {
                return (ExtSettings)Settings.SpecialSettings;
            }
        }

        public override Type[] GetTypesForXmlSerialization()
        {
            return new[]
            {
                typeof(ExtSettings)
            };
        }

        public override System.Windows.Forms.Control SettingsTab
        {
            get
            {
                return new ucExtSettings
                {
                    Sett = Settings
                };
            }
        }

        public override WareInfo WareInfoType
        {
            get
            {
                return new ExtWareInfo();
            }
        }

        protected override bool Login()
        {
            return true;
        }

        protected override void RealStartProcess()
        {
            lock (this)
            {
                lstProcessQueue.Add(new ProcessQueueItem
                {
                    URL = Url,
                    ItemType = ProcessLevels.Category
                });
            }

            StartOrPushPropertiesThread();
        }

        private void ProcessCategoryListPage(ProcessQueueItem pqi)
        {
            if (cancel)
                return;

            var html = PageRetriever.ReadFromServer(pqi.URL);
            var doc = CreateDoc(html);

            pqi.Processed = true;

            var categories = doc.DocumentNode.SelectNodes("//ul[@class='category-list']/li/ul/li");
            if (categories == null)
            {
                Log.Error(categories, new NullReferenceException());
                return;
            }

            foreach (var category in categories)
            {
                var a = category.SelectSingleNode(".//a[@href]");
                var name = a.InnerTextOrNull();

                if (string.IsNullOrWhiteSpace(name))
                {
                    Log.Warn(category, new NullReferenceException());
                    continue;
                }

                var wi = new ExtWareInfo
                {
                    Category = name
                };
                var url = a.AttributeOrNull("href");

                lock (this)
                {
                    lstProcessQueue.Add(new ProcessQueueItem
                    {
                        Item = wi,
                        ItemType = ProcessLevels.SubCategory,
                        URL = Url + url,
                        Name = name
                    });
                }
            }

            OnItemLoaded(null);
            pqi.Processed = true;

            MessagePrinter.PrintMessage("Category list processed");
            StartOrPushPropertiesThread();
        }

        private void ProcessSubCategoryListPage(ProcessQueueItem pqi)
        {
            if (cancel)
                return;


            var wi = (ExtWareInfo)pqi.Item;
            var html = PageRetriever.ReadFromServer(pqi.URL);
            var doc = CreateDoc(html);

            pqi.Processed = true;

            var subCategories = doc.DocumentNode.SelectNodes("//div[@class='SubCategoryListGrid']/ul/li");
            if (subCategories == null)
            {
                Log.Error(subCategories, new NullReferenceException());
                return;
            }

            foreach (var subCategory in subCategories)
            {
                var a = subCategory.SelectSingleNode(".//a[2][@href]");
                var name = a.InnerTextOrNull();

                if (string.IsNullOrWhiteSpace(name))
                {
                    Log.Warn(subCategory, new NullReferenceException());
                    continue;
                }

                wi.SubCategory = name;

                var url = a.AttributeOrNull("href");

                lock (this)
                {
                    lstProcessQueue.Add(new ProcessQueueItem
                    {
                        Item = wi,
                        ItemType = ProcessLevels.ProductPager,
                        URL = url,
                        Name = name
                    });
                }

            }

            OnItemLoaded(null);
            pqi.Processed = true;

            MessagePrinter.PrintMessage("Sub-category list processed");
            StartOrPushPropertiesThread();
        }

        private void ProcessProductPagerPage(ProcessQueueItem pqi)
        {
            if (cancel)
                return;


            var wi = (ExtWareInfo)pqi.Item;
            var html = PageRetriever.ReadFromServer(pqi.URL);
            var doc = CreateDoc(html);

            pqi.Processed = true;

            var pages = doc.DocumentNode.SelectNodes("//ul[@class='PagingList'][1]/li/a[@href]");

            if (pages == null)
            {
                wi.Page = 1;

                lock (this)
                {
                    lstProcessQueue.Add(new ProcessQueueItem
                    {
                        Item = wi,
                        ItemType = ProcessLevels.ProductList,
                        URL = pqi.URL
                    });
                }
            }
            else
            {
                wi.Page = 1;

                lock (this)
                {
                    lstProcessQueue.Add(new ProcessQueueItem
                    {
                        Item = wi,
                        ItemType = ProcessLevels.ProductList,
                        URL = pqi.URL
                    });
                }

                foreach (var page in pages)
                {
                    var name = page.InnerTextOrNull();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Log.Warn(page, new NullReferenceException());
                        continue;
                    }

                    wi.Page = ParseInt(name);

                    lock (this)
                    {
                        lstProcessQueue.Add(new ProcessQueueItem
                        {
                            Item = wi,
                            ItemType = ProcessLevels.ProductList,
                            URL = page.AttributeOrNull("href")
                        });
                    }
                }
            }

            OnItemLoaded(null);
            pqi.Processed = true;

            MessagePrinter.PrintMessage("Product pager processed");
            StartOrPushPropertiesThread();
        }

        private void ProcessProductListPage(ProcessQueueItem pqi)
        {
            if (cancel)
                return; 

            var wi = (ExtWareInfo)pqi.Item;
            var html = PageRetriever.ReadFromServer(pqi.URL);
            var doc = CreateDoc(html);

            pqi.Processed = true;

            var products = doc.DocumentNode.SelectNodes("//ul[contains(@class,'ProductList')]/li");
            if (products == null)
            {
                Log.Error(products, new NullReferenceException());

                return;
            }

            foreach (var product in products)
            {

                var a = product.SelectSingleNode(".//div[@class='ProductDetails']/strong/a[@href]");
                var name = a.InnerTextOrNull();

                if (string.IsNullOrWhiteSpace(name))
                {
                    Log.Warn(product, new NullReferenceException());

                    continue;
                }

                var url = a.AttributeOrNull("href");

                wi.Url = url;

                lock (this)
                {
                    lstProcessQueue.Add(new ProcessQueueItem
                    {
                        Item = wi,
                        Name = name,
                        ItemType = ProcessLevels.ProductDetails,
                        URL = url
                    });
                }

            }

            OnItemLoaded(null);
            pqi.Processed = true;

            MessagePrinter.PrintMessage("Product list processed");
            StartOrPushPropertiesThread();
        }

        private void ProcessDetailsPage(ProcessQueueItem pqi)
        {
            if (cancel) 
                return; 

            var wi = (ExtWareInfo)pqi.Item;
            var html = PageRetriever.ReadFromServer(pqi.URL);
            var doc = CreateDoc(html);

            pqi.Processed = true;

            var details = doc.DocumentNode.SelectSingleNode("//div[@class='BlockContent']");
            if (details == null)
            {
                Log.Error(details, new NullReferenceException());
                return;
            }

            var images = details.SelectNodes("//div[@class='ProductTinyImageList']/ul/li/div[@class='TinyOuterDiv']/div/a[@rel]")
                .Select(s => JsonConvert.DeserializeObject<ImageObject>(s.AttributeOrNull("rel")));
            var item = new ExtWareInfo
            {
                Category = wi.Category,
                SubCategory = wi.SubCategory,
                Page = wi.Page,
                Url = wi.Url,
                Title = details.SelectSingleNode("//h1").InnerTextOrNull(),
                Description = details.SelectSingleNode("//div[@class='ProductDescriptionContainer']").InnerTextOrNull(),
                Price = ParsePrice(details.SelectSingleNode("//em[@class='ProductPrice VariationProductPrice']").InnerTextOrNull()),
                Weight = ParseDouble(details.SelectSingleNode("//span[@class='VariationProductWeight']").InnerTextOrNull()),
                Images = String.Join(";", images.Select(s => s.largeimage))
            };

            AddWareInfo(item);

            OnItemLoaded(null);
            MessagePrinter.PrintMessage("Product '" + item.Title + "' details processed");
            StartOrPushPropertiesThread();

        }

        protected override Action<ProcessQueueItem> GetItemProcessor(ProcessQueueItem item)
        {
            Action<ProcessQueueItem> act;

            switch (item.ItemType)
            {

                case ProcessLevels.Category:
                    act = ProcessCategoryListPage;
                    break;

                case ProcessLevels.SubCategory:
                    act = ProcessSubCategoryListPage;
                    break;

                case ProcessLevels.ProductPager:
                    act = ProcessProductPagerPage;
                    break;

                case ProcessLevels.ProductList:
                    act = ProcessProductListPage;
                    break;

                case ProcessLevels.ProductDetails:
                    act = ProcessDetailsPage;
                    break;

                default:
                    act = null;
                    break;
            }

            return act;
        }
    }
}
