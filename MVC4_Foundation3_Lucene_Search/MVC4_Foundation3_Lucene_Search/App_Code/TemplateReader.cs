using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace MVC4_Foundation3_Lucene_Search
{
    public class TemplateReader
    {
        public static string GetTemplate1(string templateName, string Path)
        {
            string templateContent = string.Empty;

            templateContent = File.ReadAllText(string.Format(@"D:\Projects\DMI Projects\MVC4_Foundation3_Lucene_Search\MVC4_Foundation3_Lucene_Search\Templates\{0}.tmpl", templateName, Path));
            //templateContent = "<li>{{Title}}</li>";
            return templateContent;
        }


        public static string GetTemplatePath
        {

            get
            {
                return HostingEnvironment.MapPath ("~/Templates").ToString ();
            }
        }

        public static string GetTemplate(string TemplateName, string FilePath)
        {
            //   string fileContent =System.Web.Caching.Cache[TemplateName] as string;
            string fileContent = HttpContext.Current.Cache[TemplateName] as string;

            if (string.IsNullOrEmpty(fileContent))
            {
                using (StreamReader sr = File.OpenText(string.Format("{1}\\{0}.tmpl", TemplateName, FilePath)))
                {
                    fileContent = sr.ReadToEnd();
                    CacheDependency dep = new CacheDependency(string.Format("{1}\\{0}.tmpl", TemplateName, FilePath), DateTime.Now);
                    HttpContext.Current.Cache.Insert(TemplateName, fileContent, dep);
                }

            }
            else
            {
            }

            return fileContent;
        }
        /// <summary>
        /// For Custom Template.
        /// </summary>
        /// <param name="TemplateName"></param>
        /// <param name="FilePath"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static string GetTemplateCustom(string TemplateName, string FilePath, string[] tags)
        {
            //   string fileContent =System.Web.Caching.Cache[TemplateName] as string;
            string fileContent = HttpContext.Current.Cache[TemplateName] as string;

           // if (string.IsNullOrEmpty(fileContent))
           // {
                using (StreamReader sr = File.OpenText(string.Format("{1}\\{0}.tmpl", TemplateName, FilePath)))
                {
                    fileContent = sr.ReadToEnd();                                          
                    fileContent = string.Format(fileContent, tags).ToString();               
                    CacheDependency dep = new CacheDependency(string.Format("{1}\\{0}.tmpl", TemplateName, FilePath), DateTime.Now);
                    HttpContext.Current.Cache.Insert(TemplateName, fileContent, dep);
                }

          //  }
           // else
           // {
           // }

            return fileContent;
        }

        public static string GetTemplateCustomChild(string TemplateName, string FilePath)
        {
            //   string fileContent =System.Web.Caching.Cache[TemplateName] as string;
            string fileContent = HttpContext.Current.Cache[TemplateName] as string;

            // if (string.IsNullOrEmpty(fileContent))
            // {
            using (StreamReader sr = File.OpenText(string.Format("{1}\\{0}.tmpl", TemplateName, FilePath)))
            {
                fileContent = sr.ReadToEnd();
              //  fileContent = string.Format(fileContent, tags).ToString();
                CacheDependency dep = new CacheDependency(string.Format("{1}\\{0}.tmpl", TemplateName, FilePath), DateTime.Now);
                HttpContext.Current.Cache.Insert(TemplateName, fileContent, dep);
            }

            //  }
            // else
            // {
            // }

            return fileContent;
        }

        //public static string GetTemplateCustomChild(string TemplateName, string FilePath, string[] tags,string childTemp)
        //{
        //    //   string fileContent =System.Web.Caching.Cache[TemplateName] as string;
        //    string fileContent = HttpContext.Current.Cache[TemplateName] as string;
        //    string fileContentChild = HttpContext.Current.Cache[childTemp] as string;
        //    List<string> listTemp = new List<string>();
        //    if (string.IsNullOrEmpty(fileContent))
        //    {
        //        using (StreamReader sr = File.OpenText(string.Format("{1}\\{0}.tmpl", TemplateName, FilePath)))
        //        {
        //            using (StreamReader sr1 = File.OpenText(string.Format("{1}\\{0}.tmpl", childTemp, FilePath)))
        //            {
        //                fileContent = sr.ReadToEnd();
        //                fileContent = string.Format(fileContent, tags).ToString();
        //                listTemp.Add(fileContent);
        //                fileContentChild = sr1.ReadToEnd();
        //                listTemp.Add(fileContentChild);
        //                CacheDependency dep = new CacheDependency(string.Format("{1}\\{0}.tmpl", TemplateName, FilePath), DateTime.Now);
        //                HttpContext.Current.Cache.Insert(TemplateName, listTemp, dep);


        //                //fileContentChild = sr.ReadToEnd();
        //                //fileContentChild = string.Format(fileContent, tags).ToString();
        //                //CacheDependency dep = new CacheDependency(string.Format("{1}\\{0}.tmpl", childTemp, FilePath), DateTime.Now);
        //                //HttpContext.Current.Cache.Insert(childTemp, fileContentChild, dep);
        //            }
                   
        //        }
                
        //    }
        //    else
        //    {
        //    }

        //    return fileContent;
        //}
    }
}
