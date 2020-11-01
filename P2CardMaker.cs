using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using UnityEditor.AnimatedValues;
using UnityEngine.Events; 
using System;


public class P2CardMaker : EditorWindow {
	public enum Timing {
		選択してください,
		召喚時効果,
		手札効果,
		戦場効果,
		攻撃成功時,
		防御成功時,
	}
	public Timing timing = Timing.選択してください;

	public enum When {
		何かの召喚時,
		天候変更時,
		ターン開始時,
		ターン終了時,
		カードを引いた時,

		コストかパワー増減時,

		LP減少時,
		SP減少時,
		LP増加時,
		SP増加時,

		状態異常付与時,
		破壊時,
		移動時,
	}
	string[] whenScripts = new string[]{
		"SUMMON",
		"WEATHER_CHANGE",
		"TURN_START",
		"TURN_END",
		"DRAW",

		"PARAM_CHANGE",

		"DAMAGE",
		"NOIZ",
		"HEAL",
		"CHARGE",
		"EFFECT",
		"BREAK",
		"MOVE"
	};

	public When when;
	int whenCount = 30;

	public enum TargetUser {
		自分の,
		相手の,
//		両者の,
		システムの,
		このカード,
	}
	public TargetUser targetUser;

	public enum User {
		自分,
		相手,
//		両者,
	}
	string[] userScripts = new string[]{
		"my",
		"enemy",
//		"",
		"",
		"self",
	};

	public enum TargetField {
		クリーチャーの,
		手札の,
		山札の,
		墓地の,
		LP,
		SP
	}
	string[] fieldScripts = new string[]{
		"Cre",
		"Hand",
		"Deck",
		"Grave",
		"LP",
		"SP"
	};

	public enum Field
	{
		戦場,
		手札,
		山札,
		墓地,
	}

	public TargetField targetField;

	public enum SelectType
	{
		選択1枚,
		ランダム1枚,
		ランダム2枚,
		ランダム3枚,
		ランダム4枚,
		ランダムZ枚,
		全て,
	}
	string[] SelectScripts = new string[]{
		"select1",
		"random(1)",
		"random(2)",
		"random(3)",
		"random(4)",
		"random(z)",
		""
	};

	public enum Systems
	{
		天候が,
		新天候が,
		ターン数が,
		対象のプレイヤーが,
		対象のカードが,
		対象のフィールドが,
		対象の状態異常が,
//		対象の状態異常の値が,
		増減値が,
	}


	string[] SystemScripts = new string[]{
		"weather",
		"[toWeather]",
		"turn",
		"[targetUser]",
		"[targetCard]",
		"[targetField]",
		"[targetEffect]",
		"[point]"
	};
	public enum DoSystems {
		天候をCにする,
		自分はカードをC枚引く,
		相手はカードをC枚引く,
		自分の山札をシャッフルする,
		相手の山札をシャッフルする,
	}
	string[] DoSystemsScripts = new string[]{
		"setWeather(C)",
		"my.draw(C)",
		"enemy.draw(C)",
		"my.deckShf",
		"enemy.deckShf"
	};

	public enum CardRefines
	{
		全てのカードの枚数が,
		カード,
		アタッカーの,
		チャージャーの,
		ディフェンダーの,
		アタッカー以外の,
		チャージャー以外の,
		ディフェンダー以外の,
		凍結を持つ,
		拘束を持つ,
		火傷を持つ,
		貫通を持つ,
		種族がWである,
		カードIDがWである,
		コストが,
		パワーが,
		戦場に存在する,
		アクティブなカード,
	}
	string[] CardRefindesScripts = new string[]{
		"count",
		"",
		"a",
		"c",
		"d",
		"-a",
		"-c",
		"-d",
		"hasEff(1)",
		"hasEff(2)",
		"hasEff(3)",
		"hasEff(4)",
		"group(W)",
		"id(W)",
		"cost",
		"power",
		"exist",
		"cardActive",
	};

	public enum CardEffect {
		凍結 = 1,
		拘束,
		火傷,
		滞空,
		貫通,
	}

	public enum IntDoRefine {
		Aである,
		Aではない,
		A以上,
		A以下,
		Aの倍数,
	}
	string[] IntRefineScripts = new string[]{
		"isInt(A)",
		"isNotInt(A)",
		"orOver(A)",
		"orUnder(A)",
		"per(A).isInt(0)"
	};

	public enum CostPowerRefine {
		Y以上の枚数が,
		Y以下の枚数が,
	}
	string[] CostPowerRefineScripts = new string[]{
		"OrOver(Y)",
		"OrUnder(Y)",
	};

	public enum IntRefine {
		Aである,
		Aではない,
		A以上の,
		A以下の,
		Aの倍数の,
	}


	public enum Do {
		後は何もしない,
		コストを,
		パワーを,
		破壊する,
		凍結する,
		拘束する,
		火傷にする,
		貫通を与える,
		持ち主の手札に加える,
		山札の一番上に置く,
		山札の一番下に置く,
		戦場に出す,
		アタッカーにする,
		チャージャーにする,
		ディフェンダーにする,
		特殊能力を削除する,
		ステータスを元に戻す,
		コピーして自分の手札に,
		IDがEのカードに変える,
	}
	string[] DoScripts = new string[]{
		"",
		"Cost",
		"Power",
		"break",
		"addEffect(1)",
		"addEffect(2)",
		"addEffect(3)",
		"addEffect(5)",
		"toHand",
		"toDeckTop",
		"toDeckBottom",
		"toCre",
		"setRole(0)",
		"setRole(1)",
		"setRole(2)",
		"removeSkill",
		"reset",
		"copyTo(my)",
		"changeCard(E)"

	};

	public enum Calculate {
		B増やす,
//		B減らす,
		B倍する,
		B分の1にする,
		Bにする
	}
	string[] CaluculateScript = new string[]{
		"add<>(B)",
		"mul<>(B)",
		"divi<>(B)",
		"set<>(B)"
	};


	public struct SequensStr
	{
		public List<object> Sequences;
	}
	public List<SequensStr> conditions;
	public List<SequensStr> Actions;

	static P2CardMaker p2CardMaker;

	[MenuItem("PROELIUM2/カードメイカー",false,-9)]
	static void Open ()
	{
		if (p2CardMaker == null) {
			p2CardMaker = GetWindow<P2CardMaker> ();
			p2CardMaker.conditions = new List<SequensStr> ();

			p2CardMaker.Actions = new List<SequensStr> ();
			SequensStr cs = new SequensStr ();
			cs.Sequences = new List<object> (){ TargetUser.システムの };
			p2CardMaker.Actions.Add (cs);
		} else {
			p2CardMaker = GetWindow<P2CardMaker> ();
		}

	}

	void OnGUI () {
		GUILayout.Space (5f);
		//発動タイミング
		timing = (Timing)EditorGUILayout.EnumPopup ("発動タイミング",timing);

		if (timing == Timing.選択してください)
			return;
		
		#region 時
		if (timing == Timing.戦場効果
			|| timing == Timing.手札効果) {
			EditorGUILayout.BeginVertical( GUI.skin.box );
			when = (When)EditorGUILayout.EnumMaskField ("時", when);
			for (int i = 0; i < whenCount; i++) {
				if (((int)when & 1 << i) != 0) {
					SmallBox (((When)i).ToString ());
				}
			}
			EditorGUILayout.EndVertical();
		}
		#endregion 

		#region 条件
		EditorGUILayout.BeginVertical( GUI.skin.box );
		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label ("条件");
		GUILayout.FlexibleSpace ();
		if (GUILayout.Button ("追加")) {
			SequensStr cs = new SequensStr ();
			cs.Sequences = new List<object> (){ TargetUser.システムの };
			conditions.Add (cs);

		}
		if (GUILayout.Button ("削除")) {
			if(conditions.Count >= 1)
			conditions.RemoveAt (conditions.Count - 1);
		}
		EditorGUILayout.EndHorizontal ();
		for (int i = 0; i < conditions.Count; i++ ){
			EditorGUILayout.BeginVertical( GUI.skin.box );
			GUILayout.Label ("条件"+(i+1));

			EditorGUILayout.BeginHorizontal ();
			var con = conditions [i];
			var seq = con.Sequences;
			for (int i2 = 0; i2 < seq.Count; i2++ ){
				object data = seq [i2];
				//整数入力時
				int x = 0;
				if (int.TryParse (data.ToString(), out x)) {
					data = EditorGUILayout.IntField (x);
				} else {
					//Enum時
//					if(i2 != 0 && i2 % 4 == 0)//4つごとに改行
//					{
//						EditorGUILayout.EndHorizontal ();
//						EditorGUILayout.BeginHorizontal ();
//					}
					object olddata = (Enum)data;
					data = EditorGUILayout.EnumPopup ((Enum)data);
					if (data.ToString () != olddata.ToString ()) {
						seq.RemoveRange (i2 + 1, seq.Count - (i2 + 1));
					}
				}
				if (i2 == seq.Count - 1) {
					//①
					if (typeof(TargetUser) == data.GetType ()) {
					
						if (((TargetUser)data) == TargetUser.システムの) {
							seq.Add (Systems.天候が);//②
						} else if (((TargetUser)data) == TargetUser.このカード) {
							seq.Add (CardRefines.パワーが);//②
						}else{
							seq.Add (TargetField.手札の);//③
						}
					}
					//②
					if (typeof(Systems) == data.GetType ()) {
						if (((Systems)data) == Systems.対象のカードが) {
							seq.Add (CardRefines.種族がWである);
						}
						if (((Systems)data) == Systems.対象のプレイヤーが) {
							seq.Add (User.自分);
						} else if (((Systems)data) == Systems.対象のフィールドが) {
							seq.Add (Field.手札);
						} else if (((Systems)data) == Systems.対象の状態異常が) {
							seq.Add (CardEffect.凍結);
						} else {
							seq.Add (IntRefine.Aである);//⑩
						}
					}
					//③
					if (typeof(TargetField) == data.GetType ()) {
						if (((TargetField)data) == TargetField.LP || ((TargetField)data) == TargetField.SP) {
							seq.Add (IntRefine.A以上の);//⑩
						} else
							seq.Add (CardRefines.全てのカードの枚数が);//④
					}

					//⑤
					if (typeof(CardRefines) == data.GetType ()) {
						if (((CardRefines)data) == CardRefines.戦場に存在する) {
							//終了
						}else if (((CardRefines)data) == CardRefines.全てのカードの枚数が) {
							seq.Add (IntRefine.Aである);
						}else if (((CardRefines)data) == CardRefines.コストが
						          || ((CardRefines)data) == CardRefines.パワーが) {
							seq.Add (CostPowerRefine.Y以上の枚数が);//⑥
						} else {
							seq.Add (CardRefines.全てのカードの枚数が);
						}
					}
					//⑥
					if (typeof(CostPowerRefine) == data.GetType ()) {
						seq.Add (IntRefine.Aである);//⑩
						//整数入力
					}

					//⑩
					if (typeof(IntRefine) == data.GetType ()) {
						//整数入力
					}
				}
				seq [i2] = data;
			}
			con.Sequences = seq;
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.EndVertical();
			conditions [i] = con;
		}
		EditorGUILayout.EndVertical();
		#endregion 

		GUILayout.Box("",GUILayout.Height(3f),GUILayout.ExpandWidth(true));

		#region 実行内容
		EditorGUILayout.BeginVertical( GUI.skin.box );

		EditorGUILayout.BeginHorizontal ();
		GUILayout.Label ("実行内容");
		GUILayout.FlexibleSpace ();
		/*
		if (GUILayout.Button ("追加")) {
			SequensStr cs = new SequensStr ();
			cs.Sequences = new List<object> (){ TargetUser.システムの };
			Actions.Add (cs);
		}
		if (GUILayout.Button ("削除")) {
			if(Actions.Count >= 1)
				Actions.RemoveAt (Actions.Count - 1);
		}
		*/
		EditorGUILayout.EndHorizontal ();
		for (int i = 0; i < Actions.Count; i++ ){
			EditorGUILayout.BeginVertical( GUI.skin.box );
			GUILayout.Label ("実行内容"+(i+1));

			EditorGUILayout.BeginHorizontal ();
			var con = Actions [i];
			var seq = con.Sequences;
			for (int i2 = 0; i2 < seq.Count; i2++ ){
				object data = seq [i2];
				//整数入力時
				int x = 0;
				if (int.TryParse (data.ToString(), out x)) {
					data = EditorGUILayout.IntField (x);
				} else {
					//Enum時
					object olddata = (Enum)data;
					data = EditorGUILayout.EnumPopup ((Enum)data);
					if (data.ToString () != olddata.ToString ()) {
						seq.RemoveRange (i2 + 1, seq.Count - (i2 + 1));
					}
				}
				if (i2 == seq.Count - 1) {
					//①
					if (typeof(TargetUser) == data.GetType ()) {

						if (((TargetUser)data) == TargetUser.システムの) {
							seq.Add (DoSystems.天候をCにする);//②
						} else if (((TargetUser)data) == TargetUser.このカード) {
							seq.Add (Do.パワーを);//②
						} else {
							seq.Add (TargetField.手札の);//③
						}
					}
					//②
					if (typeof(Systems) == data.GetType ()) {
						if (((Systems)data) == Systems.対象のカードが) {
							seq.Add (CardRefines.種族がWである);
						}
						if (((Systems)data) == Systems.対象のプレイヤーが) {
							seq.Add (User.自分);
						} else if (((Systems)data) == Systems.対象のフィールドが) {
							seq.Add (Field.手札);
						} else if (((Systems)data) == Systems.対象の状態異常が) {
							seq.Add (CardEffect.凍結);
						} else {
							seq.Add (IntRefine.Aである);//⑩
						}
					}
					//③
					if (typeof(TargetField) == data.GetType ()) {
						if (((TargetField)data) == TargetField.LP || ((TargetField)data) == TargetField.SP) {
							seq.Add (Calculate.B増やす);//⑩
						} else
							seq.Add (CardRefines.カード);//④
					}

					//⑤
					if (typeof(CardRefines) == data.GetType ()) {
						if (((CardRefines)data) == CardRefines.戦場に存在する) {
							//終了
						}else if (((CardRefines)data) == CardRefines.カード) {
							seq.Add(SelectType.全て);
						}else if (((CardRefines)data) == CardRefines.カードIDがWである
							|| ((CardRefines)data) == CardRefines.種族がWである
							|| ((CardRefines)data) == CardRefines.コストが
							|| ((CardRefines)data) == CardRefines.パワーが) {
							seq.Add (IntDoRefine.Aである);//⑥
						} else {
							seq.Add (CardRefines.カード);
						}
					}
					//
					if (typeof(SelectType) == data.GetType ()) {
						seq.Add (Do.後は何もしない);
					}

//					if (typeof(IntCardRefine) == data.GetType ()) {
//						seq.Add (IntRefine.Xである);//⑩
//						//整数入力
//					}

					//⑥
					if (typeof(IntDoRefine) == data.GetType ()) {
						seq.Add (CardRefines.カード);
						//整数入力
					}
					//⑦
					if (typeof(Do) == data.GetType ()) {
						if (((Do)data) == Do.後は何もしない) {
							//終わり
						} else if (((Do)data) == Do.コストを 
							|| ((Do)data) == Do.パワーを ) {
							seq.Add (Calculate.B増やす);
						} else {
							seq.Add (Do.後は何もしない);
						}
					}
					//⑧
					if (typeof(Calculate) == data.GetType ()) {
						seq.Add (Do.後は何もしない);
					}
				}
				seq [i2] = data;
			}
			con.Sequences = seq;
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.EndVertical();
			Actions [i] = con;
		}
		EditorGUILayout.EndVertical();
		#endregion 

		if (Actions.Count >= 1) {
			if (timing == Timing.戦場効果 || timing == Timing.手札効果) {
				if ((int)when == 0) {
					SmallBox ("時を指定してください");
					return;
				}
			}
			//結果表示
			string skill = "";
			string script = "";

			//
			switch (timing) {
			case Timing.召喚時効果:
				{
					skill += "[s]";
				}
				break;
			case Timing.戦場効果:
				{
					skill += "[f]";
				}
				break;
			case Timing.手札効果:
				{
					skill += "[h]";
				}
				break;
			case Timing.攻撃成功時:
				{
					skill += "[a]";
				}
				break;
			case Timing.防御成功時:
				{
					skill += "[d]";
				}
				break;

			}
			//When
			for (int i = 0; i < whenCount; i++) {
				if (((int)when & 1 << i) != 0) {
					skill += (((When)i).ToString ()) + "、";
					script+="when("+whenScripts[i]+")";
				}
			}

			//If
			for (int i = 0; i < conditions.Count; i++) {
				var con = conditions [i];
				var seq = con.Sequences;

				script += "if(";

				for (int i2 = 0; i2 < seq.Count; i2++ ){
					var data = seq [i2];

					var load = Load (data);



					if (load == "Hand" 
						|| load == "Deck"
						|| load == "Grave"
						|| load == "Cre"
						||load == "OrOver(Y)"
						|| load == "OrUnder(Y)") {//.を減らす処理
						script = script.Remove(script.Length - 1,1);
					}
					if (load != "") {
						script += load;
						script += ".";
					}

					var addSkillText = ((Enum)seq [i2]).ToString ();
					if (addSkillText == "システムの") {
						
					} else {
						skill += addSkillText;
					}


				}

				skill+="場合、";
				//最後の.を弾く
				script = script.Remove (script.Length - 1);
				script+= ")";
			}


			//do
			for (int i = 0; i < Actions.Count; i++) {
				var con = Actions [i];
				var seq = con.Sequences;
				for (int i2 = 0; i2 < seq.Count; i2++ ){
					var data = seq [i2];

					var load = Load (data);

					//.を減らす処理
					if (load == "Hand" 
						|| load == "Deck"
						|| load == "Grave"
						|| load == "Cre"
						||load == "OrOver(Y)"
						|| load == "OrUnder(Y)") {
						script = script.Remove(script.Length - 1,1);
					}

					//addCost()にする処理
					if (typeof(Calculate) == data.GetType ()) {
						var spr = script.Split ('.');
						int lastMozicount = spr [spr.Length - 2].Length;
						string lastmozi = script.Substring (script.Length - 1 - lastMozicount, lastMozicount);
						script = script.Remove (script.Length - 1 - lastMozicount);

						script += load;

						script = script.Replace ("<>", lastmozi);
					} else {
						if (load != "") {
							script += load;
							script += ".";
						}

					}



					var addSkillText = ((Enum)seq [i2]).ToString ();
					if (addSkillText == "システムの"
						||addSkillText == "後は何もしない") {

					} else {
						skill += addSkillText;
					}
				}
				//最後の.を弾く
				script = script.Remove (script.Length - 1);
				skill+="。";
				script+= ";";
			}

			EditorGUILayout.BeginVertical( GUI.skin.box );
			using (new BackgroundColorScope (Color.yellow)) {
				// 緑色のボタン
				if (GUILayout.Button (skill)) {
					EditorGUIUtility.systemCopyBuffer = skill;
				}
				if (GUILayout.Button (script)) {
					EditorGUIUtility.systemCopyBuffer = script;
				}
			}
			EditorGUILayout.EndVertical();


		}
	}

	string Load (object data) {
		string script = ""; 
		int x = (int)data;
		if (typeof(User) == data.GetType ()
			|| typeof(TargetUser) == data.GetType ()) {
			script+= userScripts[x];
		}
		if (typeof(Field) == data.GetType () 
			|| typeof(TargetField) == data.GetType () ) {
			script+= fieldScripts[x];
		}
		if (typeof(SelectType) == data.GetType ()) {
			script+= SelectScripts[x];
		}
		if (typeof(Systems) == data.GetType () ) {
			script+= SystemScripts[x];
		}
		if (typeof(CardRefines) == data.GetType () ) {
			script+= CardRefindesScripts[x];
		}
		if (typeof(IntRefine) == data.GetType () 
			|| typeof(IntDoRefine) == data.GetType ()) {
			script+= IntRefineScripts[x];
		}
		if (typeof(CostPowerRefine) == data.GetType ()) {
			script+= CostPowerRefineScripts[x];
		}
		if (typeof(DoSystems) == data.GetType ()) {
			script+= DoSystemsScripts[x];
		}
		if (typeof(Do) == data.GetType ()) {
			script+= DoScripts[x];
		}
		if (typeof(Calculate) == data.GetType ()) {
			script+= CaluculateScript[x];
		}

		return script;
	}

//	bool SelectTarget () {
//		EditorGUILayout.BeginHorizontal ();
//
//		EditorGUILayout.EndHorizontal ();
//	}

	void SmallBox (string str) {
		EditorGUILayout.BeginVertical( GUI.skin.box );
		GUILayout.Label (str);
		EditorGUILayout.EndVertical();
	}


}
