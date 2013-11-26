
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace MVC4_Foundation3_Lucene_Search
{
    public class Helper
    {

        public static MvcHtmlString RenderUI(Dictionary<string, string> dictionary, string format)
        {
            foreach (var item in dictionary)
            {
                format = format.Replace("{{" + item.Key + "}}", item.Value);
            }

            return new MvcHtmlString(format);
        }

        public static MvcHtmlString RenderUI(Dictionary<string, Dictionary<string, string>> dictionary, string format)
        {
            var returnString = string.Empty;

            foreach (var item in dictionary)
            {
                returnString += RenderUI(item.Value, format);
            }

            return new MvcHtmlString(returnString);
        }

        #region Temporary... need to customize
        /// <summary>
        /// Temporary for "GetTemplateCustom" method in TemplateReader.cs
        /// </summary>
        /// <param name="navigationItems"></param>
        /// <param name="format"></param>
        /// <param name="container"></param>
        /// <param name="ulClassName"></param>
        /// <param name="liClassName"></param>
        /// <returns></returns>
        public static MvcHtmlString RenderUICustom(Dictionary<string, string> dictionary, string format)
        {
            foreach (var item in dictionary)
            {
                format = format.Replace("{" + item.Key + "}", item.Value);
            }

            return new MvcHtmlString(format);
        }

        public static MvcHtmlString RenderUICustom(Dictionary<string, Dictionary<string, string>> dictionary, string format)
        {
            var returnString = string.Empty;

            foreach (var item in dictionary)
            {
                returnString += RenderUICustom(item.Value, format);
            }

            return new MvcHtmlString(returnString);
        }

        /// <summary>
        /// RenderUICustomChild
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="format"></param>
        /// <returns></returns>
//        for(var item in dictionary ) {

//   string1 += renderui(item, childtemplate);
   
//   item["ChildContent"]= string1;
//   finalHtml = renderUi(item , parentTemplate);

//}
        public static MvcHtmlString RenderUICustomChild(Dictionary<string, string> dictionary, string format)
        {
            foreach (var item in dictionary)
            {
                format = format.Replace("{" + item.Key + "}", item.Value);
            }

            return new MvcHtmlString(format);
        }
        public static MvcHtmlString RenderUICustomChild1(Dictionary<string, string> dictionary1, string format1)
        {
            foreach (var item1 in dictionary1)
            {
                format1 = format1.Replace("{{" + item1.Key + "}}", item1.Value);
            }

            return new MvcHtmlString(format1);
        }
        public static MvcHtmlString RenderUICustomChild(Dictionary<string, Dictionary<string, string>> dictionary, Dictionary<string, Dictionary<string, string>> dictionary1, string format, string format1)
        {
            var returnString = string.Empty;
            //var returnString1 = string.Empty;
            foreach (var item in dictionary)
            {             
                    returnString += RenderUICustomChild(item.Value, format);
                           
            }
            foreach (var item1 in dictionary1)
            {
               returnString += RenderUICustomChild1(item1.Value, format1);
              
                
            }
            return new MvcHtmlString(returnString);
        }
            

        #endregion
        /*
        public static MvcHtmlString RenderMenuItems(List<TopNavigation> navigationItems, string format,
            string container, string ulClassName, string liClassName)
        {
            if (navigationItems == null || navigationItems.Count == 0)
                return new MvcHtmlString(string.Empty);

            var stringBuilder = new StringBuilder(string.Format(container, ulClassName));

            foreach (var item in navigationItems)
            {
                var str = RenderUI(item.GetCollection, format).ToString();

                var children = (from child in navigationItems
                                where child.ParentTopMenuID == item.TopMenuID
                                select child).ToList();

                liClassName = children.Any() ? "has-dropdown not-click" : string.Empty;

                str = string.Format(str, liClassName,
                    RenderMenuItems(children, format, container,
                        children.Any() ? "dropdown" : "left", liClassName));

                stringBuilder.Append(str);
            }

            stringBuilder.Append(container.Replace("<", "</"));  


            return new MvcHtmlString(stringBuilder.ToString());
        }


    
      
                public static MvcHtmlString RenderUI(List<KeyValue> collection, string format)
                {
                    foreach (var item in collection)
                    {
                        format = format.Replace("{{" + item.Key + "}}", item.Value);
                    }

                    return new MvcHtmlString(format);
                }

                public static MvcHtmlString RenderUI(List<List<KeyValue>> collection, string format)
                {
                    string returnString = string.Empty;

                    foreach (var item in collection)
                    {
                        returnString += RenderUI(item, format);
                    }

                    return new MvcHtmlString(returnString);
                }
         */
    }

}
