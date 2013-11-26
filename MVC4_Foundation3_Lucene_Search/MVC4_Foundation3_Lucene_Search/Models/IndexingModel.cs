using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC4_Foundation3_Lucene_Search.Models
{
    public class IndexingModel
    {
        public string SearchValue { get; set; }
        public string wiithallwords { get; set; }
        public string exactphrase { get; set; }
        public string leastWords { get; set; }
        public string withoutWords { get; set; }

        public string hndvalue { get; set; }

        //for dropdown list
        public IEnumerable<SelectListItem> AdvanceLanguageList { get; set; }

        public IEnumerable<SelectListItem> SimpleLanguageList { get; set; }


        //for Bazzervoice
        public string api_URL { set; get; }
        public object api_filter { set; get; }

        
    }
   
   
   
}