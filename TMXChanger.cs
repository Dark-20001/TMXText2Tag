using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace TMXText2Tag
{
    public class TMXChanger
    {
        Regex regTag = new Regex(@"(\\\\\\\\|\$)?\{[^{}]+\}", RegexOptions.Compiled);
        Regex regTag2 = new Regex(@"(\\[nrt])+", RegexOptions.Compiled);

        public void ChangeTMX(string tmxFile)
        {
            StreamWriter log = new StreamWriter(tmxFile + ".log",false);

            XDocument doc = XDocument.Load(tmxFile);

            IEnumerable<XElement> tuElements = doc.Descendants("tu");
            foreach (XElement tuElement in tuElements)
            {
                IEnumerable<XElement> segElements = tuElement.Descendants("seg");
                XElement sourceSegElement = segElements.First<XElement>();
                XElement targetSegElement = segElements.Last<XElement>();

                if (sourceSegElement.Descendants("ph").Count<XElement>() > 0)
                {
                    log.WriteLine(tuElement.ToString());
                }
                else
                {
                 
                    ////////////////REG1
                    if (regTag.IsMatch(sourceSegElement.InnerXml()))
                    {
                        int counter = 0;
                        counter++;

                        string sourceResult = sourceSegElement.InnerXml();
                        string targetResult = targetSegElement.InnerXml();

                        MatchCollection mc= regTag.Matches(sourceResult);
                        int loop = mc.Count;

                        for (int i = 0; i < loop; i++)
                        {
                            sourceResult = regTag.Replace(sourceResult, "<ph x=\"" + counter + "\" />", 1);
                            string text = mc[i].Groups[0].Value;
                            text = text.Replace("$", "\\$");
                            text = text.Replace("{", "\\{");
                            text = text.Replace("}", "\\}");

                            Regex innoReg = new Regex(text, RegexOptions.Compiled);

                            targetResult = innoReg.Replace(targetResult, "<ph x=\"" + counter + "\" />", 1);
                            counter++;
                        }

                        sourceSegElement.Value = string.Empty;
                        targetSegElement.Value = string.Empty;
                        sourceSegElement.Add(new XCData(sourceResult));
                        targetSegElement.Add(new XCData(targetResult));
                    }
                    ///////////REG2
                    //1
                    if (regTag2.IsMatch(sourceSegElement.InnerXml()))
                    {
                        int counter = 0;
                        counter++;

                        string sourceResult = sourceSegElement.InnerXml();
                        string targetResult = targetSegElement.InnerXml();

                        //2
                        MatchCollection mc = regTag2.Matches(sourceResult);
                        int loop = mc.Count;

                        for (int i = 0; i < loop; i++)
                        {
                            //3
                            sourceResult = regTag2.Replace(sourceResult, "<ph x=\"" + counter + "\" />", 1);
                            string text = mc[i].Groups[0].Value;
                            text = text.Replace("\\", "\\\\");

                            Regex innoReg = new Regex(text, RegexOptions.Compiled);

                            targetResult = innoReg.Replace(targetResult, "<ph x=\"" + counter + "\" />", 1);
                            counter++;
                        }

                        sourceSegElement.Value = string.Empty;
                        targetSegElement.Value = string.Empty;
                        sourceSegElement.Add(new XCData(sourceResult));
                        targetSegElement.Add(new XCData(targetResult));
                    }

                    /////////FIX Source
                    string sourceStrResult = sourceSegElement.InnerXml();
                    //string targetStrResult = targetSegElement.InnerXml();

                    sourceStrResult = sourceStrResult.Replace("'s ", "\\'s ");
                    sourceStrResult = sourceStrResult.Replace("'ve ", "\\'ve ");
                    sourceStrResult = sourceStrResult.Replace("'t ", "\\'t ");
                    sourceStrResult = sourceStrResult.Replace("'re ", "\\'re ");


                    sourceSegElement.Value = string.Empty;
                    //targetSegElement.Value = string.Empty;
                    sourceSegElement.Add(new XCData(sourceStrResult));
                    //targetSegElement.Add(new XCData(targetStrResult));
                }
                
            }
            doc.Save(tmxFile + ".new");
            log.Close();
        }
    }
}
