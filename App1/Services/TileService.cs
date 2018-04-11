using App1.Models;
using System.Xml.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace App1.Services {
    class TileService {
        static public void SetBadgeCountOnTile(int count) {
            // Update the badge on the real tile
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);

            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", count.ToString());

            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
        }

        public static XmlDocument CreateTiles(ListItem item) {
            XDocument xDoc = new XDocument(
                new XElement("tile", new XAttribute("version", 3),
                    new XElement("visual",
                        // Small Tile
                        new XElement("binding", new XAttribute("branding", "name"), new XAttribute("displayName", "MyList"), new XAttribute("template", "TileSmall"),
                            new XElement("image", new XAttribute("placement", "background"), new XAttribute("src", "Assets/sea.jpg")),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", item.date, new XAttribute("hint-style", "caption")),
                                    new XElement("text", item.title, new XAttribute("hint-style", "caption"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                )
                            )
                        ),

                        // Medium Tile
                        new XElement("binding", new XAttribute("branding", "name"), new XAttribute("displayName", "MyList"), new XAttribute("template", "TileMedium"),
                            new XElement("image", new XAttribute("placement", "background"), new XAttribute("src", "Assets/sea.jpg")),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", item.date, new XAttribute("hint-style", "caption")),
                                    new XElement("text", item.title, new XAttribute("hint-style", "caption"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                )
                            )
                        ),

                        // Wide Tile
                        new XElement("binding", new XAttribute("branding", "name"), new XAttribute("displayName", "MyList"), new XAttribute("template", "TileWide"),
                            new XElement("image", new XAttribute("placement", "background"), new XAttribute("src", "Assets/sea.jpg")),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", item.date, new XAttribute("hint-style", "caption")),
                                    new XElement("text", item.title, new XAttribute("hint-style", "caption"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3)),
                                    new XElement("text", item.detail, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                ),
                                new XElement("subgroup", new XAttribute("hint-weight", 25),
                                    new XElement("image", new XAttribute("placement", "inline"), new XAttribute("src", item.image_uri))
                                )
                            )
                        ),

                        //Large Tile
                        new XElement("binding", new XAttribute("branding", "name"), new XAttribute("displayName", "MyList"), new XAttribute("template", "TileLarge"),
                            new XElement("image", new XAttribute("placement", "background"), new XAttribute("src", "Assets/sea.jpg")),
                            new XElement("group",
                                new XElement("subgroup",
                                    new XElement("text", item.date, new XAttribute("hint-style", "caption")),
                                    new XElement("text", item.title, new XAttribute("hint-style", "subtitle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3)),
                                    new XElement("text", item.detail, new XAttribute("hint-style", "captionsubtle"), new XAttribute("hint-wrap", true), new XAttribute("hint-maxLines", 3))
                                ),
                                new XElement("subgroup", new XAttribute("hint-weight", 30),
                                    new XElement("image", new XAttribute("placement", "inline"), new XAttribute("src", item.image_uri))
                                )
                            )
                        )
                    )
                )
            );

 
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xDoc.ToString());
            return xmlDoc;
        }
    }
}
