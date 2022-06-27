using UnityEngine;
using Modding;

namespace spaar.ModLoader.PCUI
{
  // This class was taken from Vapid's ModLoader with permission.
  // All credit goes to VapidLinus.
  // It was later modified for this project.
  public class Buttons
  {
    public GUIStyle Default { get; set; }
    public GUIStyle Disabled { get; set; }
    public GUIStyle Red { get; set; }
    public GUIStyle ComponentField { get; set; }
    public GUIStyle LogEntryLabel { get; set; }
    public GUIStyle ThinNoTopBotMargin { get; set; }
    public GUIStyle ArrowExpanded { get; set; }
    public GUIStyle ArrowCollapsed { get; set; }
    public GUIStyle ArrowDarkExpanded { get; set; }
    public GUIStyle ArrowDarkCollapsed { get; set; }

    internal Buttons()
    {
      var textColor = Elements.Colors.DefaultText;

      Default = new GUIStyle
      {
        normal = { background = ModResource.GetTexture("ui_blue-normal.png"), textColor = textColor },
        hover = { background = ModResource.GetTexture("ui_blue-light.png"), textColor = textColor },
        active = { background = ModResource.GetTexture("ui_blue-dark.png"), textColor = textColor },
        border = new RectOffset(4, 4, 4, 4),
        padding = Elements.Settings.DefaultPadding,
        margin = Elements.Settings.DefaultMargin,
        alignment = TextAnchor.MiddleCenter,
        fontSize = 14,
        fontStyle = FontStyle.Bold
      };

      Red = new GUIStyle(Default)
      {
        normal = { background = ModResource.GetTexture("ui_button-light-grey.png"), },
        hover = { background = ModResource.GetTexture("ui_button-red.png"), },
      };

      Disabled = new GUIStyle(Default)
      {
        normal = { background = ModResource.GetTexture("ui_blue-very-dark.png") }
      };

      var margin = Elements.Settings.LowMargin;
      margin.left = 0;
      ComponentField = new GUIStyle(Default)
      {
        margin = margin,
        padding = Elements.Settings.LowPadding,
        fontSize = 13
      };

      LogEntryLabel = new GUIStyle(Elements.Labels.LogEntry)
      {
        hover = { background = ModResource.GetTexture("ui_blue-light.png"), textColor = textColor },
        active = { background = ModResource.GetTexture("ui_blue-dark.png"), textColor = Elements.Colors.LowlightText }
      };

      ThinNoTopBotMargin = new GUIStyle(Default)
      {
        margin = new RectOffset(Default.margin.left, Default.margin.right, 0, 0)
      };

      ArrowCollapsed = new GUIStyle
      {
        normal = { background = ModResource.GetTexture("ui_arrow-normal-right.png") },
        hover = { background = ModResource.GetTexture("ui_arrow-hover-right.png") },
        active = { background = ModResource.GetTexture("ui_arrow-disabled-right.png") },
        alignment = TextAnchor.MiddleCenter,
        margin = new RectOffset(0, 6, 2, 2)
      };

      ArrowExpanded = new GUIStyle(ArrowCollapsed)
      {
        normal = { background = ModResource.GetTexture("ui_arrow-normal-down.png") },
        hover = { background = ModResource.GetTexture("ui_arrow-hover-down.png") },
        active = { background = ModResource.GetTexture("ui_arrow-disabled-down.png") }
      };

      ArrowDarkCollapsed = new GUIStyle(ArrowCollapsed)
      {
        normal = { background = ModResource.GetTexture("ui_arrow-disabled-right.png") },
        hover = { background = ModResource.GetTexture("ui_arrow-disabled-right.png") }
      };

      ArrowDarkExpanded = new GUIStyle(ArrowExpanded)
      {
        normal = { background = ModResource.GetTexture("ui_arrow-disabled-down.png") },
        hover = { background = ModResource.GetTexture("ui_arrow-disabled-down.png") }
      };
    }
  }
}