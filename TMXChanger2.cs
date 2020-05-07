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
    public class TranslationUnit
    {
        public int ID { get; set; }
        public string SourceLang { get; set; }
        public string TargetLang { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
    }


    public static class TMXWriter
    {
        public static void WriteTMXFile(List<TranslationUnit> TUs, string FileName)
        {
            StreamWriter sw = new StreamWriter(FileName, false, Encoding.Unicode);
            string TMTime = GetTime();

            string Header = "<?xml version=\"1.0\" ?>\r\n<tmx version=\"1.4\">\r\n<header\r\n\tcreationtool=\"TRADOS Translator's Workbench for Windows\"\r\n\tcreationtoolversion=\"Edition 7 Build 756\"\r\n\tsegtype=\"sentence\"\r\n\to-tmf=\"TW4Win 2.0 Format\"\r\n\tadminlang=\"" + TUs.First<TranslationUnit>().SourceLang + "\"\r\n\tsrclang=\"" + TUs.First<TranslationUnit>().SourceLang + "\"\r\n\tdatatype=\"rtf\"\r\n\tcreationdate=\"" + TMTime + "\"\r\n\tcreationid=\"LocalizationTools\"\r\n>\r\n</header>\r\n\r\n<body>\r\n";
            string Footer = "\r\n</body>\r\n</tmx>";
            string T1 = "<tu creationdate=\"" + TMTime + "\" creationid=\"AutoTool\">\r\n<tuv xml:lang=\"" + TUs.First<TranslationUnit>().SourceLang + "\">\r\n<seg>";
            string T2 = "</seg>\r\n</tuv>\r\n<tuv xml:lang=\"" + TUs.First<TranslationUnit>().TargetLang + "\">\r\n<seg>";
            string T3 = "</seg>\r\n</tuv>\r\n</tu>\r\n";
            sw.WriteLine(Header);


            foreach (TranslationUnit tu in TUs)
            {
                sw.Write(T1);
                sw.Write(tu.Source);
                sw.Write(T2);
                sw.Write(tu.Target);
                sw.Write(T3);
            }

            sw.WriteLine(Footer);
            sw.Close();

        }
        private static string GetTime()
        {
            DateTime TimeNow = System.DateTime.Now;
            string TMDate = TimeNow.ToString("yyyyMMdd");
            string TMTime = TimeNow.ToString("hhmmss");
            return TMDate + "T" + TMTime + "Z";
        }
    }


    public class TMXChanger
    {
        //Regex regSeg = new Regex(@"(?<!\\):(?= )", RegexOptions.Compiled);
        Regex regSeg = new Regex(@"(?-i:(?<!\\):(?= [A-Z]))", RegexOptions.Compiled);


        public int SplitTMX(string tmxFile)
        {
            StreamWriter log = new StreamWriter(tmxFile + ".log",false, Encoding.UTF8);

            XDocument doc = XDocument.Load(tmxFile);

            IEnumerable<XElement> tuElements = doc.Descendants("tu");
            List<TranslationUnit> TUs = new List<TranslationUnit>();
            List<TranslationUnit> TUswithTagissue = new List<TranslationUnit>();

            int counter = 0;
            foreach (XElement tuElement in tuElements)
            {
                //lang
                IEnumerable<XElement> tuvElements = tuElement.Descendants("tuv");
                XElement sourceTuvElement = tuvElements.First<XElement>();
                XElement targetTuvElement = tuvElements.Last<XElement>();

                IEnumerable<XElement> segElements = tuElement.Descendants("seg");
                XElement sourceSegElement = segElements.First<XElement>();
                XElement targetSegElement = segElements.Last<XElement>();

                //if (sourceSegElement.Value == sourceSegElement.FirstNode.ToString() && targetSegElement.Value == targetSegElement.FirstNode.ToString()) //TEXT Node
                //{
                    string sourceSegElementStr = sourceSegElement.InnerXml();
                    string targetSegElementStr = targetSegElement.InnerXml();
                    if (regSeg.IsMatch(sourceSegElementStr))
                    {
                        string[] tars = targetSegElementStr.Split(':');
                        if (tars.Length == 2)
                        {
                            

                            Match m = regSeg.Match(sourceSegElementStr);
                            TranslationUnit tu1 = new TranslationUnit();
                            tu1.ID = counter;
                            tu1.SourceLang = sourceTuvElement.FirstAttribute.Value;
                            tu1.TargetLang = targetTuvElement.FirstAttribute.Value;
                            tu1.Source = sourceSegElementStr.Substring(0, m.Index + 1);
                            tu1.Target = tars[0] + ':';

                            
                            TranslationUnit tu2 = new TranslationUnit();
                            tu2.ID = counter;
                            tu2.SourceLang = sourceTuvElement.FirstAttribute.Value;
                            tu2.TargetLang = targetTuvElement.FirstAttribute.Value;
                            tu2.Source = sourceSegElementStr.Substring(m.Index + 1, sourceSegElementStr.Length - (m.Index + 1)).TrimStart();
                            tu2.Target = tars[1].TrimStart();

                        //check bpt ept

                        if (SameAmountBptEpt(tu1.Source) && SameAmountBptEpt(tu1.Target))
                        {
                            counter++;
                            TUs.Add(tu1);
                        }
                        else
                        {
                            TUswithTagissue.Add(tu1);
                        }

                        if (SameAmountBptEpt(tu2.Source) && SameAmountBptEpt(tu2.Target))
                        {
                            counter++;
                            TUs.Add(tu2);
                        }
                        else
                        {
                            TUswithTagissue.Add(tu2);
                        }

                    }
                    else
                    {
                        log.WriteLine();
                        log.Write(tuElement.ToString());
                    }
                }

            }

            if(TUs.Count>0)
                TMXWriter.WriteTMXFile(TUs, tmxFile + ".extracted.tmx");
            if(TUswithTagissue.Count>0)
                TMXWriter.WriteTMXFile(TUswithTagissue, tmxFile + ".tag-error.tmx");

            log.Close();
            return counter;
        }

        public bool SameAmountBptEpt(string xmlStr)
        {
            try
            {
                XDocument doc = XDocument.Parse("<r>" + xmlStr + "</r>");
                IEnumerable<XElement> bpts = doc.Descendants("bpt");
                IEnumerable<XElement> epts = doc.Descendants("ept");

                int f = 0;

                if (bpts.Count<XElement>() > 0)
                {
                    if (epts.Count<XElement>() == bpts.Count<XElement>())
                    {
                        foreach (XElement bpt in bpts)
                        {
                            XAttribute ib = bpt.Attribute("i");
                            foreach (XElement ept in epts)
                            {
                                XAttribute ie = ept.Attribute("i");
                                if (ie.Value == ib.Value)
                                {
                                    f++;
                                }
                            }
                        }

                        if (f == epts.Count<XElement>())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (epts.Count<XElement>() == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception es)
            { return false; }
        }
    }
}
