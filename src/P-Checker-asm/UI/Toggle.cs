using UnityEngine;
using Modding;

namespace spaar.ModLoader.PCUI
{
  public class Toggle
  {
    public GUIStyle Default { get; set; }

    internal Toggle()
    {
      Default = new GUIStyle()
      {
        normal = {
          background = ModResource.GetTexture("ui_toggle-normal.png"),
        },
        onNormal = {
          background = ModResource.GetTexture("ui_toggle-on-normal.png"),
        },
        hover = {
          background = ModResource.GetTexture("ui_toggle-hover.png"),
        },
        onHover = {
          background = ModResource.GetTexture("ui_toggle-on-hover.png"),
        },
        active = {
          background = ModResource.GetTexture("ui_toggle-active.png"),
        },
        onActive = {
          background = ModResource.GetTexture("ui_toggle-on-active.png"),
        },
        margin = { right = 10 }
      };
    }
  }
}
