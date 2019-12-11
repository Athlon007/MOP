using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Xml.Linq;
using System.Xml;

namespace MOP
{
    class Occlusion : MonoBehaviour
    {
        public Occlusion()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Properties.Resources.occlusiontable);
            XmlNodeList root = doc.SelectNodes("World/Object");
            MSCLoader.ModConsole.Print("Initializing..");
            ReadChildNode(root, "");

            Camera.main.gameObject.AddComponent<OcclusionCamera>();

            MSCLoader.ModConsole.Print("Occlusion Done!");
        }

        void ReadChildNode(XmlNodeList nodeList, string path)
        {
            try
            {
                foreach (XmlNode node in nodeList)
                {
                    string name = node.Attributes["name"].Value;
                    string pathToSelf = path != "" ? path + "/" + name : name;
                    MSCLoader.ModConsole.Print(pathToSelf);

                    if (node.Attributes["exception"] == null)
                    {
                        GameObject gm = GameObject.Find(pathToSelf);

                        if (gm == null)
                        {
                            MSCLoader.ModConsole.Error("[MOP] Object not found: " + pathToSelf);
                            continue;
                        }                

                        if (gm.GetComponent<OcclusionObject>() == null)
                            gm.AddComponent<OcclusionObject>();
                    }
                    else
                    {
                        MSCLoader.ModConsole.Print("Skipped...");
                    }

                    if (node.ChildNodes.Count > 0)
                    {
                        ReadChildNode(node.SelectNodes("Object"), pathToSelf);
                    }
                }
            }
            catch (Exception ex)
            {
                MSCLoader.ModConsole.Error(ex.ToString());
            }
        }
    }
}
