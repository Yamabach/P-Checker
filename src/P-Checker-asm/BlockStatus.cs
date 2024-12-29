using Modding.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace PCheckerSpace.BlockStatus
{
    public class XMLDeserializer
    {
        [XmlRoot("BlockStatus")]
        public class BlockStatusLimit : Element
        {
            [XmlElement("block")]
            public List<Block> Blocks { get; set; }

            /// <summary>
            /// 同じkeyを持つ全てのSliderを取得する
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public List<Slider> GetSliders(string key)
            {
                List<Slider> sliders = new List<Slider>();
                foreach (var block in Blocks)
                {
                    foreach (var slider in block.Sliders)
                    {
                        if (slider.Key == key)
                        {
                            sliders.Add(slider);
                        }
                    }
                }
                return sliders;
            }
        }
        [XmlRoot("Block")]
        public class Block
        {
            [XmlAttribute("id")] public int Id { get; set; }
            [XmlAttribute("name")] public string Name { get; set; }

            [XmlElement("slider")] public List<Slider> Sliders { get; set; }
        }
        [XmlRoot("Slider")]
        public class Slider
        {
            [XmlAttribute("display_name")] public string DisplayName { get; set; }
            [XmlAttribute("key")] public string Key { get; set; }
            [XmlAttribute("min")] public float Min { get; set; }
            [XmlAttribute("max")] public float Max { get; set; }
            [XmlAttribute("unclamped")] public bool Unclamped { get; set; }
        }

        public static BlockStatusLimit Deserialize(string xml="Resources/BlockStatus.xml")
        {
            var result = Modding.ModIO.DeserializeXml<BlockStatusLimit>(xml);
            return result;
        }
        public static void Serialize(BlockStatusLimit blockStatus, string xml = "Resources/BlockStatus.xml")
        {
            Modding.ModIO.SerializeXml(blockStatus, xml);
        }
    }
}