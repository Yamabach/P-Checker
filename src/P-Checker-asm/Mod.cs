using Modding;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq;
using spaar.ModLoader.PCUI;

namespace PCheckerSpace
{
	public class Mod : ModEntryPoint
	{
		public static GameObject PCmod;
		public override void OnLoad()
		{
			PCmod = new GameObject("4th P1GP regulation checker");
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
		private Rect windowRect = new Rect(0f, 80f, 300f, 100f);
		public int windowId;
		private bool hide = false;
		public static BlockBehaviour picked;
		private bool OpenURL = false;
		private Machine machine = Machine.Active();
		public override string Name
		{
			get
			{
				return "P Chacker GUI";
			}
		}
		public const int MachineBlockMax = 200;
		public const int MachineSkinMax = 2;
		public int TotalBlock
		{
			get
			{
				int count = 0;
				foreach (BlockBehaviour current in machine.BuildingBlocks)
				{
					if (current.BlockID != (int)BlockType.BuildEdge && current.BlockID != (int)BlockType.BuildNode)
					{
						count++;
					}
				}
				return count;
			}
		}
		private bool showDuringSimulation = true;
		private bool minimizeUI = false;
		private bool WasInGlobalPlayMode = false;
		private bool IsOK = false;
		private AudioSource audioSource;
		public static ModAudioClip SEPankoro, SENG;
		private bool PlaySound = true;
		public GUIStyle StyleOk, StyleNg;

		public void Awake()
        {
			if (audioSource == null)
            {
				audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
			}
			StyleOk = new GUIStyle();
			StyleOk.normal.textColor = Color.white;
			StyleNg = new GUIStyle();
			StyleNg.normal.textColor = Color.red;
			windowId = ModUtility.GetWindowId();
			machine = Machine.Active();
        }
		void Start()
        {
			machine = Machine.Active();
		}

		public void Update()
		{
			if (!StatMaster.isMainMenu && Input.GetKeyDown(KeyCode.Tab))
			{
				hide = !hide;
			}
			if (OpenURL)
			{
				OpenURL = false;
				//Application.OpenURL("https://docs.google.com/spreadsheets/d/1oEi-RUSannCdbTHx1yuU1mhQcdUu-1u46QdpQjEo6Hg/edit?usp=sharing"); // 第4回
				Application.OpenURL("https://docs.google.com/spreadsheets/d/1KBizrmb2volLAelfyXzKeEzKSntdt8GG4n-w8pF8J-E/edit?usp=sharing"); // 第5回
				//公式レギュレーションのスプシを開く
			}
			if (minimizeUI)
			{
				windowRect.size = new Vector2(120f, 100f);
			}
			else 
			{
				windowRect.size = new Vector2(120f, 100f);
			}
			if (StatMaster.InGlobalPlayMode && !WasInGlobalPlayMode && PlaySound)
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
			WasInGlobalPlayMode = StatMaster.InGlobalPlayMode;
		}
		public void OnGUI()
		{
			if (machine == null)
            {
				machine = Machine.Active();
            }
			GUI.skin = ModGUI.Skin;
			//if (!StatMaster.isClient && !StatMaster.isMainMenu && !hide)
			if (!StatMaster.isMainMenu && !hide)
			{
				if (machine.isSimulating && !showDuringSimulation) { return; }
				windowRect = GUILayout.Window(windowId, windowRect, Mapper, "Pチェッカー");
			}
		}
		public void Mapper(int windowId)
		{
			bool flagWhole, flagBomb, flagFlyingBlock, flagWaterCannon, flagShrapnelCannon, flagRocket, flagForbidden; //禁止・制限ブロック
			bool flagScale, flagPower, flagSkin; //スケールされたブロック、違法にコピペされたブロック
			if (!minimizeUI)
			{
				GUILayout.BeginHorizontal(); GUILayout.Label("総ブロック数", (TotalBlock <= MachineBlockMax ? StyleOk : StyleNg)); GUILayout.FlexibleSpace(); GUILayout.Label(TotalBlock.ToString() + "/" + MachineBlockMax, (TotalBlock <= MachineBlockMax ? StyleOk : StyleNg)); GUILayout.EndHorizontal();
			}
			flagWhole = TotalBlock <= MachineBlockMax;
			flagBomb = ShowBlockNumber("ボム", (int)BlockType.Bomb, MachineBlockMax, 4);
			flagFlyingBlock = ShowBlockNumber("フライングブロック", (int)BlockType.FlyingBlock, 20);
			flagWaterCannon = ShowBlockNumber("ウォーターキャノン", (int)BlockType.WaterCannon, 20);
			flagShrapnelCannon = ShowBlockNumber("榴散弾キャノン", (int)BlockType.ShrapnelCannon, 20);
			flagRocket = ShowBlockNumber("ロケット", (int)BlockType.Rocket, 10);
			flagForbidden = ShowBlockNumber("禁止ブロック", new int[]
			{
				(int)BlockType.Flamethrower,
				(int)BlockType.Cannon, 
				(int)BlockType.Vacuum, 
				(int)BlockType.Crossbow, 
				(int)BlockType.BuildSurface, 
				52, 
				(int)BlockType.ScalingBlock,
				(int)BlockType.Propeller,
				(int)BlockType.SmallPropeller,
			});

			flagScale = ShowBlockNumber("スケール変更", CheckType.Scale);
			flagPower = ShowBlockNumber("コピペ使用", CheckType.Power);
			flagSkin = ShowSkinNumber();

			GUILayout.BeginHorizontal();
			IsOK = flagWhole &&
				flagBomb &&
				flagFlyingBlock &&
				flagWaterCannon &&
				flagShrapnelCannon &&
				flagRocket &&
				flagForbidden &&
				flagScale &&
				flagPower &&
				flagSkin;
			GUILayout.Label("ブロック数総評", IsOK ? StyleOk : StyleNg); GUILayout.FlexibleSpace(); GUILayout.Label(IsOK ? "OK" : "NG", IsOK ? StyleOk : StyleNg);
			GUILayout.EndHorizontal();

			if (!minimizeUI)
			{
				GUILayout.Label("");
				GUILayout.BeginHorizontal();
				GUILayout.Label("選択中のブロック"); GUILayout.FlexibleSpace(); GUILayout.Label(picked != null ? picked.name : "-");
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("ブロックの角度"); GUILayout.FlexibleSpace(); GUILayout.Label(picked != null ? picked.transform.rotation.eulerAngles.ToString() : "(-, -, -)");
				GUILayout.EndHorizontal();

				GUILayout.Label("");
				GUILayout.Label("詳しいレギュレーションは、\n大会運営スプレッドシートをご覧ください");
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(picked != null ? picked.name : "-"); GUILayout.FlexibleSpace(); GUILayout.Label(picked != null ? picked.transform.rotation.eulerAngles.ToString() : "(-,-,-)");
				GUILayout.EndHorizontal();
			}
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace(); OpenURL = GUILayout.Button("ブラウザでルールを読む");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("シミュ中もUIを表示");
			GUILayout.FlexibleSpace();
			showDuringSimulation = GUILayout.Toggle(showDuringSimulation, "    ");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("UIを最小化");
			GUILayout.FlexibleSpace();
			minimizeUI = GUILayout.Toggle(minimizeUI, "    ");
			GUILayout.EndHorizontal();

			if (!minimizeUI)
            {
				GUILayout.BeginHorizontal();
				GUILayout.Label("サウンド");
				GUILayout.FlexibleSpace();
				PlaySound = GUILayout.Toggle(PlaySound, "    ");
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
			if (!minimizeUI)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(Label, ret ? StyleOk : StyleNg); GUILayout.FlexibleSpace(); GUILayout.Label(BlockCount.ToString() + "/" + (min != 0 ? min.ToString() : max.ToString()), ret ? StyleOk : StyleNg);
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
			foreach(BlockBehaviour current in machine.BuildingBlocks)
			{
				if (BlockIds.Contains(current.BlockID)){
					BlockCount++;
				}
			}
			bool ret = min <= BlockCount && BlockCount <= max;
			if (!minimizeUI)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(Label, ret ? StyleOk : StyleNg); GUILayout.FlexibleSpace(); GUILayout.Label(BlockCount.ToString(), ret ? StyleOk : StyleNg);
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
					foreach (BlockBehaviour block in machine.BuildingBlocks)
					{
						if (block.transform.localScale != Vector3.one)
						{
							num_scale++;
						}
					}
					ret = num_scale == 0;
					if (!minimizeUI)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(Label, ret ? StyleOk : StyleNg); GUILayout.FlexibleSpace(); GUILayout.Label(num_scale.ToString(), ret ? StyleOk : StyleNg);
						GUILayout.EndHorizontal();
					}
					break;
				case CheckType.Power:
					int num_power = 0;
					BlockController.CustomBlockBehaviour CBB;
					foreach (BlockBehaviour block in machine.BuildingBlocks)
					{
						if (block.BlockID == (int)BlockType.StartingBlock)
                        {
							continue;
                        }
						CBB = block.GetComponent<BlockController.CustomBlockBehaviour>();
						if (CBB == null)
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
					if (!minimizeUI)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(Label, ret ? StyleOk : StyleNg); GUILayout.FlexibleSpace(); GUILayout.Label(num_power.ToString(), ret ? StyleOk : StyleNg);
						GUILayout.EndHorizontal();
					}
					break;
				default:
					ret = TotalBlock <= MachineBlockMax;
					if (!minimizeUI)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(Label, ret ? StyleOk : StyleNg); GUILayout.FlexibleSpace(); GUILayout.Label(TotalBlock.ToString(), ret ? StyleOk : StyleNg);
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
			foreach (BlockBehaviour block in machine.BuildingBlocks)
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
			bool ret = skins.Count <= MachineSkinMax;
			if (!minimizeUI)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("スキン数", ret ? StyleOk : StyleNg); GUILayout.FlexibleSpace(); GUILayout.Label((skins.Count).ToString() + "/" + MachineSkinMax.ToString(), ret ? StyleOk : StyleNg);
				GUILayout.EndHorizontal();
			}
			return ret; //デフォルトスキンを含む
		}
		public int NumOfBlock(int BlockId)
		{
			int num = 0;
			foreach(BlockBehaviour current in machine.BuildingBlocks)
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
}
