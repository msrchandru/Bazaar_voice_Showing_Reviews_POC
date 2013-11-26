using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC4_Foundation3_Lucene_Search.Models;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Lucene.Net.Index;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using MVC4_Foundation3_Lucene_Search.Models;
using System.Reflection;
using System.Collections.Generic;
//using BvSeoSdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MVC4_Foundation3_Lucene_Search.Controllers
{
    public class HomeController : Controller
    {

        string strcon = ConfigurationManager.ConnectionStrings["dbconn"].ConnectionString;

        /// <summary>
        /// Creating an object to store the searched data
        /// </summary>
        public class SearchResults
        {
            public string PageName { get; set; }
            public string Tag { get; set; }
            public string ContentText { get; set; }
            public int Priority { get; set; }
            public string Language { get; set; }

        }
        /// <summary>
        /// Set value for minimum value for prefix match
        /// </summary>
        public enum MinValue
        {
            MinPrefexvalue = 5
        }

        #region Indexing methods
        // The query fetch all person details
        public DataSet GetPersons()
        {
            String sqlQuery = @"SELECT [PageName],[Tag],[ContentText],[Priority],[Language] FROM [dbo].[tblCrawlerData]";

            return GetDataSet(sqlQuery);
        }

        // Returns the dataset
        public DataSet GetDataSet(string sqlQuery)
        {
            DataSet ds = new DataSet();
            SqlConnection sqlCon = new SqlConnection(strcon);
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.Connection = sqlCon;
            sqlCmd.CommandType = CommandType.Text;
            sqlCmd.CommandText = sqlQuery;
            SqlDataAdapter sqlAdap = new SqlDataAdapter(sqlCmd);
            sqlAdap.Fill(ds);
            return ds;
        }

        // Creates the lucene.net index with person details
        public void CreatePersonsIndex(DataSet ds)
        {
            //Specify the index file location where the indexes are to be stored
            string indexFileLocation = @"C:\Persons";
            Lucene.Net.Store.Directory dir = Lucene.Net.Store.FSDirectory.GetDirectory(indexFileLocation, true);
            IndexWriter indexWriter = new IndexWriter(dir, new StandardAnalyzer(), true);
            indexWriter.SetRAMBufferSizeMB(10.0);
            indexWriter.SetUseCompoundFile(false);
            indexWriter.SetMaxMergeDocs(10000);
            indexWriter.SetMergeFactor(100);

            if (ds.Tables[0] != null)
            {
                DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        //Create the Document object
                        Document doc = new Document();
                        foreach (DataColumn dc in dt.Columns)
                        {
                            //Populate the document with the column name and value from our query
                            doc.Add(new Field(dc.ColumnName, dr[dc.ColumnName].ToString(), Field.Store.YES, Field.Index.TOKENIZED));
                        }
                        // Write the Document to the catalog
                        indexWriter.AddDocument(doc);
                    }
                }
            }
            // Close the writer
            indexWriter.Close();
        }
        #endregion

        #region Searching Methods
        /// <summary>
        /// for simple searching
        /// </summary>
        /// <param name="searchString"></param>
        public DataTable SearchPersons(string searchString, string strlanguage)
        {
            // Results are collected as a List
            List<SearchResults> Searchresults = new List<SearchResults>();

            // Specify the location where the index files are stored
            string indexFileLocation = @"C:\Persons";
            Lucene.Net.Store.Directory dir = Lucene.Net.Store.FSDirectory.GetDirectory(indexFileLocation,true);
            // specify the search fields, lucene search in multiple fields
            string[] searchfields = new string[] { "ContentText" };
            IndexSearcher indexSearcher=null;
            try
            {
                 indexSearcher = new IndexSearcher(dir);
            }
            catch (Exception  z) {
              
                CreatePersonsIndex(GetPersons());
                indexSearcher = new IndexSearcher(dir);
            
            }
            // Making a boolean query for searching and get the searched hits
            BooleanQuery objbool = QueryMaker(searchString, searchfields);

            var hits = indexSearcher.Search(objbool); // ~ symbol is used for fuzzy search. * for wildcard search

            List<SearchResults> searchlist = new List<SearchResults>();
            SearchResults result = null;
            //add to list
            for (int i = 0; i < hits.Length(); i++)
            {
                result = new SearchResults();
                result.PageName = hits.Doc(i).GetField("PageName").StringValue();
                result.Tag = hits.Doc(i).GetField("Tag").StringValue();
                result.ContentText = hits.Doc(i).GetField("ContentText").StringValue();
                result.Priority = Convert.ToInt32(hits.Doc(i).GetField("Priority").StringValue());
                result.Language = hits.Doc(i).GetField("Language").StringValue();

                searchlist.Add(result);
            }
            //sort by priority
            searchlist = searchlist.OrderBy(x => x.Priority).ToList();

            if (strlanguage == String.Empty)
            {
                  searchlist = searchlist.Where(x => x.Language.StartsWith("en") || x.Language.StartsWith("EN") || x.Language.StartsWith("En") || x.Language.StartsWith("eN")).ToList();
                //searchlist.Any(x => string.Equals(x.Language.ToLower(), "en", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
             searchlist = searchlist.Where(x => x.Language.StartsWith(strlanguage) || x.Language.StartsWith(strlanguage.ToLower()) || x.Language.StartsWith(strlanguage.ToUpper())).ToList();
                //searchlist.Any(x => string.Equals(x.Language.ToLower(), strlanguage, StringComparison.OrdinalIgnoreCase));
            }

            indexSearcher.Close();
            //GridView1.DataSource = searchlist;
            //GridView1.DataBind();
            ListtoDataTableConverter converter = new ListtoDataTableConverter();
            DataTable dt = converter.ToDataTable(searchlist);


            return dt;
        }

        /// <summary>
        /// Making the query for simple search
        /// </summary>
        /// <param name="searchString">text for search</param>
        /// <param name="searchfields">passing fields for search</param>
        /// <returns></returns>
        public BooleanQuery QueryMaker(string searchString, string[] searchfields)
        {
            var parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_29, searchfields, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));

            var finalQuery = new BooleanQuery();

            string searchText;
            searchText = searchString.Replace("+", "");
            searchText = searchText.Replace("\"", "");
            searchText = searchText.Replace("\'", "");
            searchText = searchText.Replace("~", "");

            //Split the search string into separate search terms by word
            string[] terms = searchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string term in terms)
            {
                finalQuery.Add(parser.Parse(term.Replace("*", "") + "*"), BooleanClause.Occur.SHOULD);
                Query query = new FuzzyQuery(new Term("ContentText", term), 0.5f, Convert.ToInt32(MinValue.MinPrefexvalue));
                finalQuery.Add(query, BooleanClause.Occur.SHOULD);
            }

            return finalQuery;
        }

        #region Advance search Methods

        /// <summary>
        /// For Advance searching- with group search
        /// </summary>
        public DataTable SearchPersons_Multiple(MVC4_Foundation3_Lucene_Search.Models.IndexingModel indexingModel, string selectedlang)
        {

            // Results are collected as a List
            List<SearchResults> Searchresults = new List<SearchResults>();

            // Specify the location where the index files are stored
            string indexFileLocation = @"C:\Persons";
            Lucene.Net.Store.Directory dir = Lucene.Net.Store.FSDirectory.GetDirectory(indexFileLocation);
            // specify the search fields, lucene search in multiple fields
            string[] searchfields = new string[] { "ContentText" };
            IndexSearcher indexSearcher = new IndexSearcher(dir);

            // Making a boolean query for searching and get the searched hits
            BooleanQuery objbool = QueryMaker_Multiple(searchfields, indexingModel);

            var hits = indexSearcher.Search(objbool);

            List<SearchResults> searchlist = new List<SearchResults>();
            SearchResults result = null;

            for (int i = 0; i < hits.Length(); i++)
            {
                result = new SearchResults();
                result.PageName = hits.Doc(i).GetField("PageName").StringValue();
                result.Tag = hits.Doc(i).GetField("Tag").StringValue();
                result.ContentText = hits.Doc(i).GetField("ContentText").StringValue();
                result.Priority = Convert.ToInt32(hits.Doc(i).GetField("Priority").StringValue());
                result.Language = hits.Doc(i).GetField("Language").StringValue();

                searchlist.Add(result);

            }
            //sort by priority
            searchlist = searchlist.OrderBy(x => x.Priority).ToList();

            if (selectedlang.Trim() == String.Empty)
            {
                searchlist = searchlist.Where(x => x.Language.Contains("english")).ToList();
            }
            else
            {
                searchlist = searchlist.Where(x => x.Language.Contains(selectedlang)).ToList();
            }
            indexSearcher.Close();
            ListtoDataTableConverter converter = new ListtoDataTableConverter();
            DataTable dt = converter.ToDataTable(searchlist);
            return dt;
        }

        /// <summary>
        /// Making the query for multiple search
        /// </summary>
        /// <param name="searchfields"></param>
        /// <returns></returns>
        public BooleanQuery QueryMaker_Multiple(string[] searchfields, MVC4_Foundation3_Lucene_Search.Models.IndexingModel indexingModel)
        {
            //var parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_29, searchfields, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
            var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, searchfields[0], new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));

            var finalQuery = new BooleanQuery();

            //for Text with all words
            if (indexingModel.wiithallwords != null)
            {
                string searchText = indexingModel.wiithallwords;
                searchText = searchText.Replace("+", "");
                searchText = searchText.Replace("\"", "");
                searchText = searchText.Replace("\'", "");
                searchText = searchText.Replace("~", "");

                //Split the search string into separate search terms by word
                string[] terms = searchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var finalQuery_sub = new BooleanQuery();

                foreach (string term in terms)
                {
                    finalQuery_sub.Add(parser.Parse(term.Replace("*", "") + "*"), BooleanClause.Occur.SHOULD);

                    Query query = new FuzzyQuery(new Term("ContentText", term), 0.5f, Convert.ToInt32(MinValue.MinPrefexvalue));
                    finalQuery_sub.Add(query, BooleanClause.Occur.SHOULD);
                }

                finalQuery.Add(finalQuery_sub, BooleanClause.Occur.SHOULD);

            }
            // with exact phrase
            if (indexingModel.exactphrase != null)
            {
                string searchText = indexingModel.exactphrase;
                searchText = searchText.Replace("+", "");
                searchText = searchText.Replace("\"", "");
                searchText = searchText.Replace("\'", "");
                searchText = searchText.Replace("~", "");

                //Split the search string into separate search terms by word
                string[] terms = searchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var finalQuery_sub = new BooleanQuery();

                foreach (string term in terms)
                {
                    finalQuery_sub.Add(parser.Parse(term.Replace("*", "") + ""), BooleanClause.Occur.MUST);
                    //Query query = new FuzzyQuery(new Term("ContentText", term), 0.5f, 4);
                    //finalQuery.Add(query, BooleanClause.Occur.MUST);
                }
                finalQuery.Add(finalQuery_sub, BooleanClause.Occur.SHOULD);
            }
            // for atleast one word
            if (indexingModel.leastWords != null)
            {
                string searchText = indexingModel.leastWords;
                searchText = searchText.Replace("+", "");
                searchText = searchText.Replace("\"", "");
                searchText = searchText.Replace("\'", "");
                searchText = searchText.Replace("~", "");

                //Split the search string into separate search terms by word
                string[] terms = searchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var finalQuery_sub = new BooleanQuery();

                foreach (string term in terms)
                {
                    //  finalQuery.Add(parser.Parse(term.Replace("*", "") + "*"), BooleanClause.Occur.SHOULD);
                    Query query = new FuzzyQuery(new Term("ContentText", term), 0.5f, Convert.ToInt32(MinValue.MinPrefexvalue));
                    finalQuery_sub.Add(query, BooleanClause.Occur.SHOULD);
                }
                finalQuery.Add(finalQuery_sub, BooleanClause.Occur.SHOULD);
            }
            // for "without word"
            if (indexingModel.withoutWords != null)
            {
                string searchText = indexingModel.withoutWords;
                searchText = searchText.Replace("+", "");
                searchText = searchText.Replace("\"", "");
                searchText = searchText.Replace("\'", "");
                searchText = searchText.Replace("~", "");

                //Split the search string into separate search terms by word
                string[] terms = searchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string term in terms)
                {

                    finalQuery.Add(parser.Parse(term.Replace("*", "") + ""), BooleanClause.Occur.MUST_NOT);
                    //previous code- to get all data then remove word from that list.
                    //finalQuery.Add(new BooleanClause(new MatchAllDocsQuery(), BooleanClause.Occur.SHOULD));
                    //finalQuery.Add(new BooleanClause(new TermQuery(new Term("ContentText", term)), BooleanClause.Occur.MUST_NOT));
                }
            }

            return finalQuery;
        }
        #endregion

        #endregion


        public ActionResult Index(MVC4_Foundation3_Lucene_Search.Models.SearchModels searchValue)
        {
            DataTable dt = SearchPersons("", "");
            ViewBag.AuthorList = dt;

            //code started for dropdownlist

            // string[] countries = new string[] { "India", "US", "UK", "Canada", "China", "Srilanka", "Singapore", "Japan", "Australia" };
            IEnumerable<string> LanguageList = GetLanguages();
            List<SelectListItem> drpItems = (from Language in LanguageList select new SelectListItem { Value = Language, Text = Language }).ToList();
            IndexingModel MyModel = new IndexingModel();
            MyModel.AdvanceLanguageList = new SelectList(drpItems, "Value", "Text");
            MyModel.SimpleLanguageList = new SelectList(drpItems, "Value", "Text");

            ViewData.Model = MyModel;
            //code ended for dropdownlist

            //Dharani-code started for bazzervoice
            string api_key = "kuy3zj9pr3n7i0wxajrzj04xo";
            string num_items = "3";
            MyModel.api_filter = "apiversion=4.9&filter=isRatingsOnly:false&filter=DisplayLocale:en_US&include=products,authors&passkey=" + api_key + "&limit=" + num_items;
            MyModel.api_URL = "http://reviews.apitestcustomer.bazaarvoice.com/bvstaging/data/reviews.json?callback=?";
            //Dharani-code Ended for bazzervoice

            return View();

        }

        /*
                public String Index()
                {
                    return new Bv(
                        deploymentZoneID: "12325",
                        //product_id: "mdw2h5jg9pwkxyg8zvfqx4fg", 
                        product_id: "kuy3zj9pr3n7i0wxajrzj04xo",
                        //The page_url is optional
                        //page_url: "http://www.example.com/store/products/data-gen-696yl2lg1kurmqxn88fqif5y2/",

                        page_url: "http://reviews.apitestcustomer.bazaarvoice.com/bvstaging/data",
                        cloudKey: "agileville-78B2EF7DE83644CAB5F8C72F2D8C8491", //agileville
                        bv_product: BvProduct.REVIEWS,
                        //bot_detection: false, //by default bot_detection is set to true
                        user_agent: "msnbot") //Setting user_agent for testing. Leave this blank in production.
                        .getSeoWithSdk(System.Web.HttpContext.Current.Request);
                }
          */
        /// <summary>
        /// For Creating a Index
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateIndex()
        {
            CreatePersonsIndex(GetPersons());


            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Search(MVC4_Foundation3_Lucene_Search.Models.IndexingModel indexingModel)
        {

            //for dropdown
            IEnumerable<string> LanguageList = GetLanguages();
            string selectedvaluelang = Request.Form["AdvanceLanguageList"];
            string selectedvaluelang2 = Request.Form["SimpleLanguageList"];


            List<SelectListItem> drpItems = (from Language in LanguageList select new SelectListItem { Value = Language, Text = Language }).ToList();
            IndexingModel MyModel = new IndexingModel();
            MyModel.AdvanceLanguageList = new SelectList(drpItems, "Value", "Text", selectedvaluelang);
            MyModel.SimpleLanguageList = new SelectList(drpItems, "Value", "Text", selectedvaluelang2);


            DataTable dt = SearchPersons(indexingModel.SearchValue, selectedvaluelang2);
            ViewBag.AuthorList = dt;

            //Dharani-code started for bazzervoice
            string api_key = "kuy3zj9pr3n7i0wxajrzj04xo";
            string num_items = "3";
            MyModel.api_filter = "apiversion=4.9&filter=isRatingsOnly:false&filter=DisplayLocale:en_US&include=products,authors&passkey=" + api_key + "&limit=" + num_items;
            MyModel.api_URL = "http://reviews.apitestcustomer.bazaarvoice.com/bvstaging/data/reviews.json?callback=?";
            //Dharani-code Ended for bazzervoice


            ViewData.Model = MyModel;

            return View("Index");
        }

        public class ListtoDataTableConverter
        {
            public DataTable ToDataTable<T>(List<T> items)
            {
                DataTable dataTable = new DataTable(typeof(T).Name);
                //Get all the properties
                PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo prop in Props)
                {
                    //Setting column names as Property names
                    dataTable.Columns.Add(prop.Name);
                }
                foreach (T item in items)
                {
                    var values = new object[Props.Length];
                    for (int i = 0; i < Props.Length; i++)
                    {
                        //inserting property values to datatable rows
                        values[i] = Props[i].GetValue(item, null);
                    }
                    dataTable.Rows.Add(values);
                }
                //put a breakpoint here and check datatable
                return dataTable;
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult advanceSearch(MVC4_Foundation3_Lucene_Search.Models.IndexingModel indexingModel)
        {
            DataTable dt = new DataTable();

            //for dropdown
            IEnumerable<string> LanguageList = GetLanguages();
            string selectedvaluelang = Request.Form["AdvanceLanguageList"];
            string selectedvaluelang2 = Request.Form["SimpleLanguageList"];

            List<SelectListItem> drpItems = (from Language in LanguageList select new SelectListItem { Value = Language, Text = Language }).ToList();
            IndexingModel MyModel = new IndexingModel();
            MyModel.AdvanceLanguageList = new SelectList(drpItems, "Value", "Text", selectedvaluelang);
            MyModel.SimpleLanguageList = new SelectList(drpItems, "Value", "Text", selectedvaluelang2);

            ViewData.Model = MyModel;

            if (indexingModel.wiithallwords == string.Empty && indexingModel.exactphrase == string.Empty && indexingModel.leastWords == string.Empty && indexingModel.withoutWords == string.Empty)
            {
                dt = SearchPersons("", "");
            }
            else
            {//alteast one word in entered in the group
                dt = SearchPersons_Multiple(indexingModel, selectedvaluelang);
            }

            ViewBag.AuthorList = dt;
            return View("Index");
        }


        // Returns the dataset for dropdownlist
        public IEnumerable<string> GetLanguages()
        {
            // Results are collected as a List
            List<SearchResults> Searchresults = new List<SearchResults>();

            // Specify the location where the index files are stored
            string indexFileLocation = @"C:\Persons";
            Lucene.Net.Store.Directory dir = Lucene.Net.Store.FSDirectory.GetDirectory(indexFileLocation);

            // specify the search fields, lucene search in multiple fields
            string[] searchfields = new string[] { "Language" };
            IndexSearcher indexSearcher = new IndexSearcher(dir);

            var parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_29, searchfields, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));

            // Making a boolean query for searching and get the searched hits
            BooleanQuery objbool = new BooleanQuery();
            objbool.Add(new BooleanClause(new MatchAllDocsQuery(), BooleanClause.Occur.SHOULD));

            var hits = indexSearcher.Search(objbool);

            List<string> Languages = new List<string>();

            //add to list
            for (int i = 0; i < hits.Length(); i++)
            {
                Languages.Add(hits.Doc(i).GetField("Language").StringValue());
            }
            //sort by priority
            IEnumerable<string> LanguagesList = Languages.Distinct();

            indexSearcher.Close();
            return LanguagesList;
        }


        #region reviews
        /*
        public class Includes
        {
        }

        public class TagDimensions
        {
        }

        public class ContextDataValues
        {
        }
        public class Top25Contributor
        {
            public string BadgeType { get; set; }
            public string Id { get; set; }
            public string ContentType { get; set; }
        }

        public class Badges
        {
            public Top25Contributor top25Contributor { get; set; }
        }

        public class AdditionalFields
        {
        }

        public class SecondaryRatings
        {
        }

        public class Result
        {

            public TagDimensions TagDimensions { get; set; }
            public List<object> TagDimensionsOrder { get; set; }
            public List<object> AdditionalFieldsOrder { get; set; }
            public object Cons { get; set; }
            public object IsRecommended { get; set; }
            public bool IsRatingsOnly { get; set; }
            public string UserNickname { get; set; }
            public object Pros { get; set; }
            public List<object> Photos { get; set; }
            public ContextDataValues ContextDataValues { get; set; }
            public List<object> Videos { get; set; }
            public List<object> ContextDataValuesOrder { get; set; }
            public string LastModificationTime { get; set; }
            public string SubmissionId { get; set; }
            public int TotalFeedbackCount { get; set; }
            public int TotalPositiveFeedbackCount { get; set; }
            public List<object> BadgesOrder { get; set; }
            public object UserLocation { get; set; }
            public Badges Badges { get; set; }
            public string AuthorId { get; set; }
            public List<object> SecondaryRatingsOrder { get; set; }
            public bool IsFeatured { get; set; }
            public bool IsSyndicated { get; set; }
            public List<object> ProductRecommendationIds { get; set; }
            public string Title { get; set; }
            public string ProductId { get; set; }
            public AdditionalFields AdditionalFields { get; set; }
            public object CampaignId { get; set; }
            public object Helpfulness { get; set; }
            public int TotalNegativeFeedbackCount { get; set; }
            public string SubmissionTime { get; set; }
            public int Rating { get; set; }
            public string ContentLocale { get; set; }
            public int RatingRange { get; set; }
            public int TotalCommentCount { get; set; }
            public string ReviewText { get; set; }
            public string ModerationStatus { get; set; }
            public List<object> ClientResponses { get; set; }
            public string Id { get; set; }
            public SecondaryRatings SecondaryRatings { get; set; }
            public List<object> CommentIds { get; set; }
            public string LastModeratedTime { get; set; }

        }

        public class RootObject
        {
            public Includes Includes { get; set; }
            public bool HasErrors { get; set; }
            public int Offset { get; set; }
            public int TotalResults { get; set; }
            public string Locale { get; set; }
            public List<object> Errors { get; set; }
            public List<Result> Results { get; set; }
            public int Limit { get; set; }
        }
      
        //  public static Dictionary<string, object> GetSubTitlePFAQ()
        public static string GetSubTitlePFAQ()
        {
            //Dictionary<string, Dictionary<string, string>> getSubTitlePFAQ = new Dictionary<string, Dictionary<string, string>>();
            //getSubTitlePFAQ.Add("1", new Dictionary<string, string>() { { "Description", " Lorem ipsum dolor sit amet, tellus id adipiscing aliquet, tortor elit faucibus lectus, vel blandit nisi velit vitae lectus." } });

            string strResults = Bv.httpGet("http://reviews.apitestcustomer.bazaarvoice.com/bvstaging/data/reviews.json?apiversion=5.4&passkey=s8kts27vafznww7rnzzyac22");

            //Result res=new Result(

            //  Console.WriteLine(Results);

            //Dictionary<string, object> obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(Convert.ToString(strResults));
            object JsonDe = JsonConvert.DeserializeObject(strResults);
            IndexingModel MyModel = new IndexingModel();
            MyModel.api_key1 = JsonDe;
            //return "<script language='javascript' type='text/javascript'>alert('Save Successfully');</script>";
            return JsonDe.ToString();
        }
        public string GetJsonValue()
        {
            //string strResults = Bv.httpGet("http://reviews.apitestcustomer.bazaarvoice.com/bvstaging/data/reviews.json?callback=?,apiversion=5.4&passkey=s8kts27vafznww7rnzzyac22");

            //object JsonDe = JsonConvert.DeserializeObject(strResults);
            string api_key = "s8kts27vafznww7rnzzyac22";
            string num_items = "5";
            string strfilter = "apiversion=4.9&include=products,authors&filter=isRatingsOnly:false&filter=DisplayLocale:en_US&passkey=" + api_key + "&limit=" + num_items;
           
            IndexingModel MyModel = new IndexingModel();
            MyModel.api_key1 = strfilter;

            string url = "http://reviews.apitestcustomer.bazaarvoice.com/bvstaging/data/reviews.json?callback=?";
            return url;
        }
      */

        #endregion
    }
}
