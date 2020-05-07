using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMXText2Tag
{
    public static class XElementExt
    {
        public static string InnerXml(this XElement node)
        {
            using (var reader = node.CreateReader())
            {
                reader.MoveToContent();
                return reader.ReadInnerXml();

            }

        }
        
    }
}
