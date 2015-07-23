using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace WindowsFormsApplication1
{
    class SecurityConfigReader : IConfigurationSectionHandler
    {
        public const string SECTION_NAME = "SecurityConfig";
        public object Create(object parent, object configContext, XmlNode section)
        {
            string szConfig = section.SelectSingleNode("//SecurityConfig").OuterXml;
            SecurityConfig retConf = null;

            if (szConfig != string.Empty || szConfig != null)
            {
                XmlSerializer xsw = new XmlSerializer(typeof(SecurityConfig));
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szConfig));
                ms.Position = 0;
                retConf = (SecurityConfig)xsw.Deserialize(ms);
            }
            return retConf;
        }
    }
}
