using Modding;
using Modding.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq;
using spaar.ModLoader.PCUI;
using System.Xml.Serialization;

namespace PCheckerSpace
{
	public class Mod : ModEntryPoint
	{
		public static GameObject PCmod;
		public override void OnLoad()
		{
			PCmod = new GameObject("P1GP Regulation Checker");
			Log("Pチェッカーをロードしました");
			SingleInstance<PCGUI>.Instance.transform.parent = PCmod.transform;
			SingleInstance<BlockController.BlockSelector>.Instance.transform.parent = PCmod.transform;
			UnityEngine.Object.DontDestroyOnLoad(PCmod);
			PCGUI.LoadAudioClip();
		}
		public static void Log(string message)
		{
			Debug.Log("P Checker Log : " + message);
		}
		public static void Warning(string message)
		{
			Debug.LogWarning("P Checker Warning : " + message);
		}
		public static void Error(string message)
		{
			Debug.LogError("P Checker Error : " + message);
		}
	}
	public class PCGUI : SingleInstance<PCGUI>
	{
		private Rect m_windowRect = new Rect(0f, 80f, 300f, 100f);
		private int m_windowId;
		private bool m_hideWindow = false;
		public static BlockBehaviour PickedBlockBehaviour;
		private bool OpenURL = false;
		private Machine m_machine = Machine.Active();
		public override string Name => "P Chacker GUI";
		/// <summary>
		/// ブロックの総数を数える
		/// </summary>
		public int TotalBlock
		{
			get
			{
				int count = 0;
				foreach (BlockBehaviour current in m_machine.BuildingBlocks)
				{
					if (current.BlockID != (int)BlockType.BuildEdge && current.BlockID != (int)BlockType.BuildNode)
					{
						count++;
					}
				}
				return count;
			}
		}
		private bool m_showDuringSimulation = true;
		private bool m_minimizeUI = false;
		private bool m_wasInGlobalPlayMode = false;
		/// <summary>
		/// マシンがレギュレーションに適合している
		/// </summary>
		public bool IsOK
		{
			get; private set;
		} = false;
		private AudioSource audioSource;
		public static ModAudioClip SEPankoro;
		public static ModAudioClip SENG;
		private bool PlaySound = true;
		private GUIStyle StyleOk;
		private GUIStyle StyleNg;
		/// <summary>
		/// 回ごとのレギュ
		/// </summary>
		public XMLDeserializer.P1GPRegulations Regulations;
		/// <summary>
		/// 現在表示しているレギュの回
		/// </summary>
		private int m_currentRegulationCount = 7;
		public bool AllowRocketExceed => Regulations.Find(m_currentRegulationCount).AllowRocketExceed;

		public void Awake()
        {
			// AudioSource取得
			if (audioSource == null)
            {
				audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
			}

			// レギュ取得
			Regulations = XMLDeserializer.Deserialize();
			m_currentRegulationCount = Regulations.P1GPRegulation.Max(r => r.Count);

			// GUIのテキストスタイルの初期化
			StyleOk = new GUIStyle();
			StyleOk.normal.textColor = Color.white;
			StyleNg = new GUIStyle();
			StyleNg.normal.textColor = Color.red;

			// その他初期化
			m_windowId = ModUtility.GetWindowId();
			m_machine = Machine.Active();
        }
		public void Update()
		{
			if (!StatMaster.isMainMenu && Input.GetKeyDown(KeyCode.Tab))
			{
				m_hideWindow = !m_hideWindow;
			}
			if (OpenURL)
			{
				OpenURL = false;

				//公式レギュレーションのスプシを開く
				Application.OpenURL(Regulations.Find(m_currentRegulationCount).WebLink);
			}

			// サイズを常に小さくなるようにする
			m_windowRect.size = new Vector2(120f, 100f);

			// シミュ開始時にルールを満たしたかどうかでSEを鳴らす
			if (StatMaster.InGlobalPlayMode && !m_wasInGlobalPlayMode && PlaySound)
            {
				if (IsOK)
				{
					audioSource.volume = 1f;
					audioSource.PlayOneShot(SEPankoro);
					//Mod.Log("OK");
				}
				else
				{
					audioSource.volume = 0.5f;
					audioSource.PlayOneShot(SENG);
					//Mod.Log("NG");
				}
			}
			m_wasInGlobalPlayMode = StatMaster.InGlobalPlayMode;
		}
		public void OnGUI()
		{
			if (m_machine is null)
            {
				m_machine = Machine.Active();
            }
			GUI.skin = ModGUI.Skin;
			//if (!StatMaster.isClient && !StatMaster.isMainMenu && !hide)
			if (!StatMaster.isMainMenu && !m_hideWindow)
			{
				if (m_machine.isSimulating && !m_showDuringSimulation) { return; }
				m_windowRect = GUILayout.Window(m_windowId, m_windowRect, Mapper, $"Pチェッカー（第{m_currentRegulationCount}回用）");
			}
		}
		public void Mapper(int windowId)
		{
			// 禁止・制限ブロック
			List<XMLDeserializer.Block> MandatoryBlocks = Regulations.MandatoryBlocks(m_currentRegulationCount);
			List<XMLDeserializer.Block> LimitedBlocks = Regulations.LimitedBlocks(m_currentRegulationCount);
			List<XMLDeserializer.Block> ForbiddenBlocks = Regulations.ForbiddenBlocks(m_currentRegulationCount);
			List<bool> FlagsMandatory = new bool[MandatoryBlocks.Count].ToList();
			List<bool> FlagsLimited = new bool[LimitedBlocks.Count].ToList();
			bool flagForbidden = false;

			// スケールされたブロック、違法にコピペされたブロック
			bool flagScale, flagPower, flagSkin;
			int total = TotalBlock;
			int max = Regulations.Find(m_currentRegulationCount).MachineBlockMax;
			bool flagWhole = total <= max;
			if (!m_minimizeUI)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label($"総ブロック数", flagWhole ? StyleOk : StyleNg);
				GUILayout.FlexibleSpace();
				GUILayout.Label($"{total}/{max}", flagWhole ? StyleOk : StyleNg);
				GUILayout.EndHorizontal();
			}

			// 必須ブロック
			for (int i=0; i<MandatoryBlocks.Count; i++)
            {
				var block = MandatoryBlocks[i];
				FlagsMandatory[i] = ShowBlockNumber(block.Name, block.Id, block.Max, block.Min);
            }
			// 制限ブロック
			for (int i=0; i<LimitedBlocks.Count; i++)
            {
				var block = LimitedBlocks[i];
				FlagsLimited[i] = ShowBlockNumber(block.Name, block.Id, block.Max, block.Min);
            }
			// 禁止ブロック
			flagForbidden = ShowBlockNumber($"禁止ブロック", ForbiddenBlocks.Select(a => a.Id).ToArray());


			flagScale = ShowBlockNumber($"スケール変更", CheckType.Scale);
			flagPower = ShowBlockNumber($"コピペ使用", CheckType.Power);
			flagSkin = ShowSkinNumber();

			IsOK = flagWhole &&
			    FlagsMandatory.All(f => f) &&
				FlagsLimited.All(f => f) &&
				flagForbidden &&
				flagScale &&
				flagPower &&
				flagSkin;
			GUILayout.BeginHorizontal();
			GUILayout.Label($"ブロック数総評", IsOK ? StyleOk : StyleNg);
			GUILayout.FlexibleSpace();
			GUILayout.Label(IsOK ? $"OK" : $"NG", IsOK ? StyleOk : StyleNg);
			GUILayout.EndHorizontal();

			// ブロックのTransform表示
			if (!m_minimizeUI)
			{
				GUILayout.Label("");
				GUILayout.BeginHorizontal();
				GUILayout.Label($"選択中のブロック"); GUILayout.FlexibleSpace(); GUILayout.Label(PickedBlockBehaviour != null ? PickedBlockBehaviour.name : "-");
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label($"ブロックの角度"); GUILayout.FlexibleSpace(); GUILayout.Label(PickedBlockBehaviour != null ? PickedBlockBehaviour.transform.rotation.eulerAngles.ToString() : "(-, -, -)");
				GUILayout.EndHorizontal();

				GUILayout.Label("");
				GUILayout.Label($"詳しいレギュレーションは、\n大会運営スプレッドシートをご覧ください");
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(PickedBlockBehaviour != null ? PickedBlockBehaviour.name : "-"); GUILayout.FlexibleSpace(); GUILayout.Label(PickedBlockBehaviour != null ? PickedBlockBehaviour.transform.rotation.eulerAngles.ToString() : "(-,-,-)");
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			OpenURL = GUILayout.Button("ブラウザでルールを読む");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("シミュ中もUIを表示");
			GUILayout.FlexibleSpace();
			m_showDuringSimulation = GUILayout.Toggle(m_showDuringSimulation, "    ");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("UIを最小化");
			GUILayout.FlexibleSpace();
			m_minimizeUI = GUILayout.Toggle(m_minimizeUI, "    ");
			GUILayout.EndHorizontal();

			if (!m_minimizeUI)
            {
				GUILayout.BeginHorizontal();
				GUILayout.Label("サウンド");
				GUILayout.FlexibleSpace();
				PlaySound = GUILayout.Toggle(PlaySound, "    ");
				GUILayout.EndHorizontal();
			}

			if (!m_minimizeUI) {
				// 第n回のルールに切り替える
				GUILayout.BeginHorizontal();
				for (int i = 0; i < Regulations.P1GPRegulation.Count; i++)
				{
					if (GUILayout.Button($"第{Regulations.P1GPRegulation[i].Count}回"))
					{
						m_currentRegulationCount = Regulations.P1GPRegulation[i].Count;
						//Mod.Log($"第{currentRegulationCount}回");
					}

					// 3個ごとに改行
					if (i % 3 == 2)
                    {
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
                    }
				}
				GUILayout.EndHorizontal();
			}

			GUI.DragWindow();
		}
		/// <summary>
		/// ブロックの数と判定をGUIに表示する
		/// OKならTrue
		/// </summary>
		/// <param name="Label"></param>
		/// <param name="BlockId"></param>
		/// <param name="max"></param>
		/// <param name="min"></param>
		/// <returns></returns>
		public bool ShowBlockNumber(string Label, int BlockId, int max=0, int min=0)
		{
			int BlockCount = NumOfBlock(BlockId);
			bool ret = min <= BlockCount && BlockCount <= max;
			if (!m_minimizeUI)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(Label, ret ? StyleOk : StyleNg); 
				GUILayout.FlexibleSpace(); 
				//GUILayout.Label(BlockCount.ToString() + "/" + (min != 0 ? min.ToString() : max.ToString()), ret ? StyleOk : StyleNg);
				var deliminator = min != 0 ? min : max;
				GUILayout.Label($"{BlockCount}/{deliminator}", ret ? StyleOk : StyleNg);
				GUILayout.EndHorizontal();
			}
			return ret;
		}
		/// <summary>
		/// ブロックの数をGUIに表示する
		/// </summary>
		/// <param name="Label"></param>
		/// <param name="BlockIds"></param>
		/// <param name="max"></param>
		/// <param name="min"></param>
		/// <returns></returns>
		public bool ShowBlockNumber(string Label, int[] BlockIds, int max=0, int min = 0)
		{
			int BlockCount = 0;
			foreach(BlockBehaviour current in m_machine.BuildingBlocks)
			{
				if (BlockIds.Contains(current.BlockID)){
					BlockCount++;
				}
			}
			bool ret = min <= BlockCount && BlockCount <= max;
			if (!m_minimizeUI)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(Label, ret ? StyleOk : StyleNg); 
				GUILayout.FlexibleSpace(); 
				GUILayout.Label($"{BlockCount}", ret ? StyleOk : StyleNg);
				GUILayout.EndHorizontal();
			}
			return ret;
		}
		public enum CheckType
        {
			Scale, Power,
        }
		/// <summary>
		/// マシンが規定を満たすことをGUIに表示する
		/// </summary>
		/// <param name="Label"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool ShowBlockNumber(string Label, CheckType type)
		{
			bool ret;
			switch (type)
			{
				case CheckType.Scale:
					int num_scale = 0;
					foreach (BlockBehaviour block in m_machine.BuildingBlocks)
					{
						if (block.transform.localScale != UnityEngine.Vector3.one)
						{
							num_scale++;
						}
					}
					ret = num_scale == 0;
					if (!m_minimizeUI)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(Label, ret ? StyleOk : StyleNg);
						GUILayout.FlexibleSpace();
						GUILayout.Label(num_scale.ToString(), ret ? StyleOk : StyleNg);
						GUILayout.EndHorizontal();
					}
					break;
				case CheckType.Power:
					int num_power = 0;
					BlockController.CustomBlockBehaviour CBB;
					foreach (BlockBehaviour block in m_machine.BuildingBlocks)
					{
						if (block.BlockID == (int)BlockType.StartingBlock)
                        {
							continue;
                        }
						CBB = block.GetComponent<BlockController.CustomBlockBehaviour>();
						if (CBB is null)
						{
							Mod.Error(block.name + "でCustomBlockBehaviourがnullです");
							continue;
						}
						if (CBB.powerFlag)
						{
							num_power++;
						}
					}
					ret = num_power == 0;
					if (!m_minimizeUI)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(Label, ret ? StyleOk : StyleNg);
						GUILayout.FlexibleSpace();
						GUILayout.Label($"{num_power}", ret ? StyleOk : StyleNg);
						GUILayout.EndHorizontal();
					}
					break;
				default:
					ret = TotalBlock <= Regulations.Find(m_currentRegulationCount).MachineBlockMax;
					if (!m_minimizeUI)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(Label, ret ? StyleOk : StyleNg);
						GUILayout.FlexibleSpace();
						GUILayout.Label(TotalBlock.ToString(), ret ? StyleOk : StyleNg);
						GUILayout.EndHorizontal();
					}
					break;
			}
			return ret;
		}
		/// <summary>
		/// マシンのスキンの数をGUIに表示する
		/// </summary>
		/// <returns></returns>
		public bool ShowSkinNumber()
		{
			List<BlockSkinLoader.SkinPack> skins = new List<BlockSkinLoader.SkinPack> { }; //ブロックごとに違う扱いっぽい
			foreach (BlockBehaviour block in m_machine.BuildingBlocks)
			{
				if (block.BlockID == (int)BlockType.BuildNode || block.BlockID == (int)BlockType.BuildEdge)
                {
					continue; // サーフェスの辺と頂点なら飛ばす
                }
				if (block.VisualController.selectedSkin.pack.isDefault)
				{
					continue; //デフォルトなら飛ばす
				}
				if (!skins.Contains(block.VisualController.selectedSkin.pack)){
					skins.Add(block.VisualController.selectedSkin.pack);
				}
			}
			var maxSkins = Regulations.Find(m_currentRegulationCount).Skins;
			var isLegal = skins.Count <= maxSkins;
			if (!m_minimizeUI)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("スキン数", isLegal ? StyleOk : StyleNg);
				GUILayout.FlexibleSpace();
				GUILayout.Label($"{skins.Count}/{maxSkins}", isLegal ? StyleOk : StyleNg);
				GUILayout.EndHorizontal();
			}
			return isLegal; //デフォルトスキンを含む
		}
		public int NumOfBlock(int BlockId)
		{
			int num = 0;
			foreach(BlockBehaviour current in m_machine.BuildingBlocks)
			{
				if (current.BlockID == BlockId)
				{
					num++;
				}
			}
			return num;
		}

		// サウンド
		public static void LoadAudioClip()
        {
			SEPankoro = ModAudioClip.GetAudioClip("pankoro");
			SENG = ModAudioClip.GetAudioClip("NG");
		}
	}
	/// <summary>
	/// Regulation.xmlを読み込むクラス
	/// https://json2csharp.com/code-converters/xml-to-csharp により内部クラスを作成
	/// </summary>
	public class XMLDeserializer
    {
		[XmlRoot("Block")]
		public class Block
		{

			[XmlAttribute(AttributeName = "name")]
			public string Name { get; set; }

			[XmlAttribute(AttributeName = "id")]
			public int Id { get; set; }

			[XmlAttribute(AttributeName = "min")]
			public int Min { get; set; }

			[XmlAttribute(AttributeName = "max")]
			public int Max { get; set; }
		}

		[XmlRoot("P1GPRegulation")]
		public class P1GPRegulation
		{
			/// <summary>
			/// 個数が制限されるブロック
			/// </summary>
			[XmlElement("Block")]
			public List<Block> Block { get; set; }
			/// <summary>
			/// ルール表へのリンク
			/// </summary>
			[XmlElement("link")]
			public string WebLink { get; set; }
			/// <summary>
			/// 第n回のn
			/// </summary>
			[XmlAttribute("count")]
			public int Count { get; set; }
			/// <summary>
			/// マシン全体のブロック上限
			/// </summary>
			[XmlAttribute("machine_block_max")]
			public int MachineBlockMax { get; set; }
			/// <summary>
			/// ロケットの限凸を許容するかどうか
			/// </summary>
			[XmlAttribute("allow_rocket_exceed")]
			public bool AllowRocketExceed { get; set; }
			[XmlAttribute("skins")]
			public int Skins { get; set; }
		}

		[XmlRoot("P1GPRegulations")]
		public class P1GPRegulations : Element
		{

			[XmlElement("P1GPRegulation")]
			public List<P1GPRegulation> P1GPRegulation { get; set; }

			public P1GPRegulation Find(int count) => P1GPRegulation.Find(r => r.Count == count);
			/// <summary>
			/// 必須ブロック
			/// </summary>
			/// <param name="count"></param>
			/// <returns></returns>
			public List<Block> MandatoryBlocks(int count) => Find(count).Block.FindAll(b => 0 < b.Min);
			/// <summary>
			/// 制限ブロック
			/// </summary>
			/// <param name="count"></param>
			/// <returns></returns>
			public List<Block> LimitedBlocks(int count) => Find(count).Block.FindAll(b => b.Min <= 0 && 0 < b.Max);
			/// <summary>
			/// 禁止ブロック
			/// </summary>
			/// <param name="count"></param>
			/// <returns></returns>
			public List<Block> ForbiddenBlocks(int count) => Find(count).Block.FindAll(b => b.Max <= 0);
		}
		/// <summary>
		/// XMLからレギュレーションを読み込む
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static P1GPRegulations Deserialize(string filePath = "Resources/Regulation.xml")
        {
			Mod.Log($"Loaded Regulations from modding folder");
			var result = Modding.ModIO.DeserializeXml<P1GPRegulations>(filePath);

			// 古い順にソート
			result.P1GPRegulation = result.P1GPRegulation.OrderBy(regulation => regulation.Count).ToList();

			return result;
        }
    }
}
