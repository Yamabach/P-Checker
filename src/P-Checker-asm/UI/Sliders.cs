using UnityEngine;
using Modding;

namespace spaar.ModLoader.PCUI
{
  public class Sliders
  {
    public GUIStyle Horizontal { get; set; }
    public GUIStyle Vertical { get; set; }

    public GUIStyle ThumbHorizontal { get; set; }
    public GUIStyle ThumbVertical { get; set; }

    internal Sliders()
    {
      Horizontal = new GUIStyle(GUI.skin.horizontalSlider)
      {
        normal = { background = ModResource.GetTexture("ui_blue-normal.png") },
      };
      Vertical = new GUIStyle(GUI.skin.verticalSlider)
      {
        normal = { background = ModResource.GetTexture("ui_blue-normal.png") },
      };
      ThumbHorizontal = new GUIStyle(GUI.skin.horizontalSliderThumb)
      {
        normal = { background = ModResource.GetTexture("ui_slider-thumb.png") },
        hover = { background = ModResource.GetTexture("ui_slider-thumb-hover.png") },
        active = { background = ModResource.GetTexture("ui_slider-thumb-active.png") }
      };
      ThumbVertical = new GUIStyle(GUI.skin.verticalSliderThumb)
      {
        normal = { background = ModResource.GetTexture("ui_slider-thumb.png") },
        hover = { background = ModResource.GetTexture("ui_slider-thumb-hover.png") },
        active = { background = ModResource.GetTexture("ui_slider-thumb-active.png") }
      };
    }
  }
}
