using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Text;
using System.xml;

Namespace FeedCollector
{
    Class Program
   {
       static void Main(string[] args)
       {
           List<string> maplist = new List<string>();
           List<string> mapdesc = new List<string>();
           List<string> mapping = new List<string>();
           List<string> comp_neg_check = new List<string>();


           List<string> complist = File.ReadAllLines("SourceFilePath_with_allNodes_listed").ToList();


           foreach (string comp in complist)

           {

               /* Configuring the WebClient */

               string url = "http://www.genome.jp/dbget-bin/www_bget?cpd:" + comp;
               WebClient webmap = new WebClient();
               String html = webmap.DownloadString(url);

               /* Pattern matching for webpage */

               string pattern_map = @"\bmap\w*\b";
               string pattern_desc = @"<td align=""left""><div>(.*?)</div></td>";
               string pattern_neg_check = @"\bNo such data\w*\b";

               /* Parsing to make sure about the availablility of the components */

               foreach (Match neg in Regex.Matches(html, pattern_neg_check, RegexOptions.Singleline))
                   comp_neg_check.Add(neg.Value);

               /* Parsing the map nodes for the components */

               foreach (Match mat in Regex.Matches(html, pattern_map, RegexOptions.Singleline))
                   maplist.Add(mat.Value);

               /* Removing the duplicates from the earlier parsed list of map nodes*/

               List<string> map_dist = maplist.Distinct().ToList();

               /* Parsing the map description for the components */

               foreach (Match mat1 in Regex.Matches(html, pattern_desc, RegexOptions.Singleline))
                   mapdesc.Add(mat1.Value);

               /* Concatenating the mapnode with map_description with tab delimited */

               for (int j = 0; j < map_dist.Count; j++)
               {

                   string mapvalue = map_dist[j] + mapdesc[j];
                   string mapval_rep = mapvalue.Replace("<td align=\"left\"><div>", "\t").Replace("</div></td>", "");
                   mapping.Add(mapval_rep);

               }

               string line = string.Empty;


               if (comp_neg_check.Count > 0)
                   /* Marking the unavailable component with -1 */
                   line = comp + "\t" + "-1" + Environment.NewLine;
               else
                   /* For available components map_nodes with description are concatenated*/
                   line = comp + "\t" + string.Join(";", mapping.ToArray()) + Environment.NewLine;

               /* Write the concatenated list appended to the target truth file */

               File.AppendAllText("TargetFilePath_for_groundtruth_file", line);

               /* Clear all the list */

               maplist.Clear();
               map_dist.Clear();
               mapdesc.Clear();
               mapping.Clear();
               comp_neg_check.Clear();

           }
           /* end of loop for component */
       }
   }
}
