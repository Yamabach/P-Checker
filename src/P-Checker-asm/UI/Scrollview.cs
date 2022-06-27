using UnityEngine;
using Modding;

namespace spaar.ModLoader.PCUI
{
  // This class was taken from Vapid's ModLoader with permissions.
  // All credit goes to VapidLinus.
  public class Scrollview
  {
    public GUIStyle Horizontal { get; set; }
    public GUIStyle Vertical { get; set; }
    public GUIStyle ThumbVertical { get; set; }
    public GUIStyle ThumbHorizontal { get; set; }

    internal Scrollview()
    {
      Horizontal = new GUIStyle
      {
        normal = { background = ModResource.GetTexture("ui_scroll-horizontal.png") },
        fixedHeight = 13,
        border = new RectOffset(6, 6, 3, 3)
      };

      Vertical = new GUIStyle
      {
        normal = { background = ModResource.GetTexture("ui_scroll-vertical.png") },
        fixedWidth = 13,
        border = new RectOffset(3, 3, 6, 6),
      };

      ThumbHorizontal = new GUIStyle
      {
        normal = { background = ModResource.GetTexture("ui_thumb-horizontal.png") },
        fixedHeight = 13,
        border = new RectOffset(6, 6, 3, 3)
      };

      ThumbVertical = new GUIStyle
      {
        normal = { background = ModResource.GetTexture("ui_thumb-vertical.png") },
        fixedWidth = 13,
        border = new RectOffset(3, 3, 6, 6)
      };
    }
  }
}
