﻿using System.Xml;

namespace Andromeda_Actions_Core.Infrastructure
{
    public interface IXmlServices
    {
        XmlDocument GetXmlFileData(string path);
        XmlWriter CreateXmlWriter(string filePath);
        void CreateUnattributedElement(ref XmlWriter xWriter, string elementName, string stringData);
        string GetNodeData(XmlDocument xDoc, string xpath);
        void CloseXmlWriter(XmlWriter xwriter);
    }
}