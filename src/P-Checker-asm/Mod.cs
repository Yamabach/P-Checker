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
			Log("P�`�F�b�J�[�����[�h���܂���");
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
		/// �u���b�N�̑����𐔂���
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
		/// �}�V�������M�����[�V�����ɓK�����Ă���
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
		/// �񂲂Ƃ̃��M��
		/// </summary>
		public XMLDeserializer.P1GPRegulations Regulations;
		/// <summary>
		/// ���ݕ\�����Ă��郌�M���̉�
		/// </summary>
		private int m_currentRegulationCount = 7;
		public bool AllowRocketExceed => Regulations.Find(m_currentRegulationCount).AllowRocketExceed;

		public void Awake()
        {
			// AudioSource�擾
			if (audioSource == null)
            {
				audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
			}

			// ���M���擾
			Regulations = XMLDeserializer.Deserialize();
			m_currentRegulationCount = Regulations.P1GPRegulation.Max(r => r.Count);

			// GUI�̃e�L�X�g�X�^�C���̏�����
			StyleOk = new GUIStyle();
			StyleOk.normal.textColor = Color.white;
			StyleNg = new GUIStyle();
			StyleNg.normal.textColor = Color.red;

			// ���̑�������
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

				//�������M�����[�V�����̃X�v�V���J��
				Application.OpenURL(Regulations.Find(m_currentRegulationCount).WebLink);
			}

			// �T�C�Y����ɏ������Ȃ�悤�ɂ���
			m_windowRect.size = new Vector2(120f, 100f);

			// �V�~���J�n���Ƀ��[���𖞂��������ǂ�����SE��炷
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
				m_windowRect = GUILayout.Window(m_windowId, m_windowRect, Mapper, $"P�`�F�b�J�[�i��{m_currentRegulationCount}��p�j");
			}
		}
		public void Mapper(int windowId)
		{
			// �֎~�E�����u���b�N
			List<XMLDeserializer.Block> MandatoryBlocks = Regulations.MandatoryBlocks(m_currentRegulationCount);
			List<XMLDeserializer.Block> LimitedBlocks = Regulations.LimitedBlocks(m_currentRegulationCount);
			List<XMLDeserializer.Block> ForbiddenBlocks = Regulations.ForbiddenBlocks(m_currentRegulationCount);
			List<bool> FlagsMandatory = new bool[MandatoryBlocks.Count].ToList();
			List<bool> FlagsLimited = new bool[LimitedBlocks.Count].ToList();
			bool flagForbidden = false;

			// �X�P�[�����ꂽ�u���b�N�A��@�ɃR�s�y���ꂽ�u���b�N
			bool flagScale, flagPower, flagSkin;
			int total = TotalBlock;
			int max = Regulations.Find(m_currentRegulationCount).MachineBlockMax;
			bool flagWhole = total <= max;
			if (!m_minimizeUI)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label($"���u���b�N��", flagWhole ? StyleOk : StyleNg);
				GUILayout.FlexibleSpace();
				GUILayout.Label($"{total}/{max}", flagWhole ? StyleOk : StyleNg);
				GUILayout.EndHorizontal();
			}

			// �K�{�u���b�N
			for (int i=0; i<MandatoryBlocks.Count; i++)
            {
				var block = MandatoryBlocks[i];
				FlagsMandatory[i] = ShowBlockNumber(block.Name, block.Id, block.Max, block.Min);
            }
			// �����u���b�N
			for (int i=0; i<LimitedBlocks.Count; i++)
            {
				var block = LimitedBlocks[i];
				FlagsLimited[i] = ShowBlockNumber(block.Name, block.Id, block.Max, block.Min);
            }
			// �֎~�u���b�N
			flagForbidden = ShowBlockNumber($"�֎~�u���b�N", ForbiddenBlocks.Select(a => a.Id).ToArray());


			flagScale = ShowBlockNumber($"�X�P�[���ύX", CheckType.Scale);
			flagPower = ShowBlockNumber($"�R�s�y�g�p", CheckType.Power);
			flagSkin = ShowSkinNumber();

			IsOK = flagWhole &&
			    FlagsMandatory.All(f => f) &&
				FlagsLimited.All(f => f) &&
				flagForbidden &&
				flagScale &&
				flagPower &&
				flagSkin;
			GUILayout.BeginHorizontal();
			GUILayout.Label($"�u���b�N�����]", IsOK ? StyleOk : StyleNg);
			GUILayout.FlexibleSpace();
			GUILayout.Label(IsOK ? $"OK" : $"NG", IsOK ? StyleOk : StyleNg);
			GUILayout.EndHorizontal();

			// �u���b�N��Transform�\��
			if (!m_minimizeUI)
			{
				GUILayout.Label("");
				GUILayout.BeginHorizontal();
				GUILayout.Label($"�I�𒆂̃u���b�N"); GUILayout.FlexibleSpace(); GUILayout.Label(PickedBlockBehaviour != null ? PickedBlockBehaviour.name : "-");
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label($"�u���b�N�̊p�x"); GUILayout.FlexibleSpace(); GUILayout.Label(PickedBlockBehaviour != null ? PickedBlockBehaviour.transform.rotation.eulerAngles.ToString() : "(-, -, -)");
				GUILayout.EndHorizontal();

				GUILayout.Label("");
				GUILayout.Label($"�ڂ������M�����[�V�����́A\n���^�c�X�v���b�h�V�[�g��������������");
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(PickedBlockBehaviour != null ? PickedBlockBehaviour.name : "-"); GUILayout.FlexibleSpace(); GUILayout.Label(PickedBlockBehaviour != null ? PickedBlockBehaviour.transform.rotation.eulerAngles.ToString() : "(-,-,-)");
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			OpenURL = GUILayout.Button("�u���E�U�Ń��[����ǂ�");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("�V�~������UI��\��");
			GUILayout.FlexibleSpace();
			m_showDuringSimulation = GUILayout.Toggle(m_showDuringSimulation, "    ");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("UI���ŏ���");
			GUILayout.FlexibleSpace();
			m_minimizeUI = GUILayout.Toggle(m_minimizeUI, "    ");
			GUILayout.EndHorizontal();

			if (!m_minimizeUI)
            {
				GUILayout.BeginHorizontal();
				GUILayout.Label("�T�E���h");
				GUILayout.FlexibleSpace();
				PlaySound = GUILayout.Toggle(PlaySound, "    ");
				GUILayout.EndHorizontal();
			}

			if (!m_minimizeUI) {
				// ��n��̃��[���ɐ؂�ւ���
				GUILayout.BeginHorizontal();
				for (int i = 0; i < Regulations.P1GPRegulation.Count; i++)
				{
					if (GUILayout.Button($"��{Regulations.P1GPRegulation[i].Count}��"))
					{
						m_currentRegulationCount = Regulations.P1GPRegulation[i].Count;
						//Mod.Log($"��{currentRegulationCount}��");
					}

					// 3���Ƃɉ��s
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
		/// �u���b�N�̐��Ɣ����GUI�ɕ\������
		/// OK�Ȃ�True
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
		/// �u���b�N�̐���GUI�ɕ\������
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
		/// �}�V�����K��𖞂������Ƃ�GUI�ɕ\������
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
							Mod.Error(block.name + "��CustomBlockBehaviour��null�ł�");
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
		/// �}�V���̃X�L���̐���GUI�ɕ\������
		/// </summary>
		/// <returns></returns>
		public bool ShowSkinNumber()
		{
			List<BlockSkinLoader.SkinPack> skins = new List<BlockSkinLoader.SkinPack> { }; //�u���b�N���ƂɈႤ�������ۂ�
			foreach (BlockBehaviour block in m_machine.BuildingBlocks)
			{
				if (block.BlockID == (int)BlockType.BuildNode || block.BlockID == (int)BlockType.BuildEdge)
                {
					continue; // �T�[�t�F�X�̕ӂƒ��_�Ȃ��΂�
                }
				if (block.VisualController.selectedSkin.pack.isDefault)
				{
					continue; //�f�t�H���g�Ȃ��΂�
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
				GUILayout.Label("�X�L����", isLegal ? StyleOk : StyleNg);
				GUILayout.FlexibleSpace();
				GUILayout.Label($"{skins.Count}/{maxSkins}", isLegal ? StyleOk : StyleNg);
				GUILayout.EndHorizontal();
			}
			return isLegal; //�f�t�H���g�X�L�����܂�
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

		// �T�E���h
		public static void LoadAudioClip()
        {
			SEPankoro = ModAudioClip.GetAudioClip("pankoro");
			SENG = ModAudioClip.GetAudioClip("NG");
		}
	}
	/// <summary>
	/// Regulation.xml��ǂݍ��ރN���X
	/// https://json2csharp.com/code-converters/xml-to-csharp �ɂ������N���X���쐬
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
			/// �������������u���b�N
			/// </summary>
			[XmlElement("Block")]
			public List<Block> Block { get; set; }
			/// <summary>
			/// ���[���\�ւ̃����N
			/// </summary>
			[XmlElement("link")]
			public string WebLink { get; set; }
			/// <summary>
			/// ��n���n
			/// </summary>
			[XmlAttribute("count")]
			public int Count { get; set; }
			/// <summary>
			/// �}�V���S�̂̃u���b�N���
			/// </summary>
			[XmlAttribute("machine_block_max")]
			public int MachineBlockMax { get; set; }
			/// <summary>
			/// ���P�b�g�̌��ʂ����e���邩�ǂ���
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
			/// �K�{�u���b�N
			/// </summary>
			/// <param name="count"></param>
			/// <returns></returns>
			public List<Block> MandatoryBlocks(int count) => Find(count).Block.FindAll(b => 0 < b.Min);
			/// <summary>
			/// �����u���b�N
			/// </summary>
			/// <param name="count"></param>
			/// <returns></returns>
			public List<Block> LimitedBlocks(int count) => Find(count).Block.FindAll(b => b.Min <= 0 && 0 < b.Max);
			/// <summary>
			/// �֎~�u���b�N
			/// </summary>
			/// <param name="count"></param>
			/// <returns></returns>
			public List<Block> ForbiddenBlocks(int count) => Find(count).Block.FindAll(b => b.Max <= 0);
		}
		/// <summary>
		/// XML���烌�M�����[�V������ǂݍ���
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static P1GPRegulations Deserialize(string filePath = "Resources/Regulation.xml")
        {
			Mod.Log($"Loaded Regulations from modding folder");
			var result = Modding.ModIO.DeserializeXml<P1GPRegulations>(filePath);

			// �Â����Ƀ\�[�g
			result.P1GPRegulation = result.P1GPRegulation.OrderBy(regulation => regulation.Count).ToList();

			return result;
        }
    }
}
