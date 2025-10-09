using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// プレイヤデータ
/// </summary>
[System.Serializable]
public class def_player
{
    // プロフィール用
    [SerializeField] private string[] name = new string[2]; // プレイヤ名と工房名
    [SerializeField] private byte titleId; // 肩書のID
    [SerializeField] private Color favorite_color; // お気にの色
    [SerializeField] private string favorite_color_name; // お気にの色の名前
    [SerializeField] private byte favorite_pattern; //お気にの柄

    [SerializeField] private byte[] playerRank = new byte[4]; // 職人ランクと工程レベル・「def_komon.creationPart」に対応した要素番号
    [SerializeField] private float[] playerRank_point = new float[4]; // 職人ランクと工程レベルの経験値・「def_komon.creationPart」に対応した要素番号

    [SerializeField] private int money; // 所持金

    // もちもの系
    [SerializeField] private short[] albumSize = new short[4]; //　各種アルバムの最大数
    [SerializeField] private byte[] havingItem = new byte[100]; // 型紙・小紋・色レシピ以外のアイテム所持数
    [SerializeField] private ushort[] havingPattern = new ushort[50]; // 各型紙の耐久値
    /// <summary>
    /// 所持している小紋データのリスト
    /// </summary>
    public List<def_komon> havingKomon = new List<def_komon>();
    /// <summary>
    /// 所持している色レシピデータのリスト
    /// </summary>
    public List<def_colorRecipe> havingColor = new List<def_colorRecipe>();
    [SerializeField] private bool[] havingTitle = new bool[30]; // 各称号について、獲得しているか

    // バトル系
    [SerializeField] private bool[] battle_areaClear = new bool[100]; // そのステージをクリアしているか

    // ていさつ系
    /// <summary>
    /// ていさつ中か
    /// </summary>
    public bool doingReconnaissance;
    /// <summary>
    /// ていさつ開始日時・偵察していなければdefault値
    /// </summary>
    public string time_reconnaissanceStart;
    /// <summary>
    /// ていさつ先のエリア番号
    /// </summary>
    public byte id_reconnaissanceArea;
    /// <summary>
    /// ていさつの所要時間
    /// </summary>
    public byte time_reconnaissanceRequired;

    /// <summary>
    /// Tutorial3パートの終了フラグ
    /// </summary>
    public bool[] isEndTutorial = new bool[3];




    /// <summary>
    /// 設定し得る名前の種類
    /// </summary>
    public enum playerNameKind
    {
        player = 0,
        workspace = 1,
        komon = 2,
        colorRecipe = 3,
        other = 4 // 自由記述（パスワードなど例外的な処理用）
    }

    public enum albumKind
    {
        pattern = 0,
        komon = 1,
        color = 2,
        storage = 3
    }



    /// <summary>
    /// プレイヤデータの新規作成・ゲームを新しく始めるときに
    /// </summary>
    /// <param name="name_player">プレイヤ名</param>
    /// <param name="name_workspace">工房名</param>
    public def_player(string name_player = "", string name_workspace = "")
    {
        if(name_player.Length > 0)
        {
            set_name(playerNameKind.player, name_player);
        }
        if(name_workspace.Length > 0)
        {
            set_name(playerNameKind.workspace, name_workspace);
        }

        Manager_CommonGroup commonM = Manager_CommonGroup.instance;
        DatabaseManager dataM = commonM.dataM;

        playerRank = new byte[4] { 1, 1, 1, 1 }; // 職人ランク・工程レベル
        albumSize = new short[4] { (short)dataM.get_constFloat(6), (short)dataM.get_constFloat(7), (short)dataM.get_constFloat(8), (short)dataM.get_constFloat(9) }; // アイテム保存種類数
#if false
        // 実行タイミングの問題か、ぬるぽなので、初期アイテムプレゼントは「SaveDataManager.Initialize()」にて行っている
        SaveData saveData = commonM.saveM.saveData;
        // 初期アイテムプレゼント
        // 型紙
        saveData.playerInfo.set_havingPattern_new(5, dataM.list_pattern.list[5].durability);
        // 生地
        saveData.playerInfo.set_havingItem(31, 1);
        // 染料
        saveData.playerInfo.set_havingItem(43, 1);
        saveData.playerInfo.set_havingItem(44, 1);
        saveData.playerInfo.set_havingItem(45, 1);
        
#endif
        // 肩書
        set_havingTitle(1, true);
        set_titleId(1);
        // 所持金
        money = (int)dataM.get_constFloat(5);
    }


    /// <summary>
    /// プレイヤもしくは工房の名前を設定する
    /// </summary>
    /// <param name="kind">設定する名前の種類</param>
    /// <param name="str">設定する値</param>
    /// <returns>設定できたらtrue</returns>
    public bool set_name(playerNameKind kind, string str)
    {
        int max = (int)Manager_CommonGroup.instance.dataM.get_constFloat(0);
        if ((str.Length <= max) && (str.Length > 0))
        {
            
        }
        else
        {
            devlog.logError($"名前の最大文字数は {max} です。　入力文字数：{str.Length}");
            return false;
        }

        if(str.Contains('<') || str.Contains('>'))
        {
            // リッチテキストに引っ掛かりそう
            //devlog.logError($"名前の最大文字数は {max} です。　入力文字数：{str.Length}");
            return false;
        }
        else
        {
            name[(int)kind] = str;
            return true;
        }
    }
    /// <summary>
    /// 指定した種類において設定されている名前を取得する
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public string get_name(playerNameKind kind) {return name[(int)kind];}

    /// <summary>
    /// 称号を設定する
    /// </summary>
    /// <param name="num">設定する称号のID</param>
    /// <returns>正常に設定されたらtrue</returns>
    public bool set_titleId(byte num)
    {
        if(num < Manager_CommonGroup.instance.dataM.list_playerTitle.list.Count)
        {
            titleId = num;
            return true;
        }
        else
        {
            devlog.logError("称号データの範囲外を指定しています！：{num}");
            return false;
        }
    }
    /// <summary>
    /// 設定されている称号のIDを取得する
    /// </summary>
    /// <returns></returns>
    public byte get_titleId() {return titleId;}

    /// <summary>
    /// 職人ランクないし工程レベルを加算する
    /// </summary>
    /// <param name="rankPart">加算したいランクの種類</param>
    /// <param name="plusNum">加算する値</param>
    private void set_playerRank(def_komon.creationPart rankPart, byte plusNum)
    {
        byte max = (byte)Manager_CommonGroup.instance.dataM.get_constFloat(2);
        if ((max - plusNum) < playerRank[(int)rankPart])
        {
            // 上限を超えて加算しようとする場合
            playerRank[(int)rankPart] = max;
        }
        else
        {
            playerRank[(int)rankPart] += plusNum;
        }
    }
    /// <summary>
    /// 職人ランクないし工程レベルを取得する
    /// </summary>
    /// <param name="rankPart">取得したいランクの種類</param>
    /// <returns>指定したランクの値</returns>
    public byte get_playerRank(def_komon.creationPart rankPart) {return playerRank[(int)rankPart];}
    /// <summary>
    /// 職人ランク/工程レベルの経験値を加算する
    /// </summary>
    /// <param name="rankPart">加算対象のランク/レベル</param>
    /// <param name="plusNum">加算値</param>
    public void set_playerRank_point(def_komon.creationPart rankPart, float plusNum)
    {
        if ((playerRank_point[(int)rankPart] + plusNum) < 0f)
        {
            playerRank_point[(int)rankPart] = 0f;
        }
        else
        {
            playerRank_point[(int)rankPart] += plusNum;
        }
    }
    /// <summary>
    /// 職人ランク/工程レベルの現経験値を取得する
    /// </summary>
    /// <param name="part">取得対象のランク/レベル</param>
    /// <returns>指定したランク/レベルの現経験値</returns>
    public float get_playerRank_point(def_komon.creationPart rankPart) { return playerRank_point[(int)rankPart]; }
    /// <summary>
    /// レベルアップするか判定する
    /// </summary>
    /// <param name="rankPart">判定対象のランク/レベル</param>
    /// <returns>レベルアップする場合はtrue</returns>
    public bool checkLevelUp(def_komon.creationPart rankPart)
    {
        DatabaseManager dataM = Manager_CommonGroup.instance.dataM;
        List<PlayerLevelEntity> list = dataM.get_LevelList(rankPart);

        byte plusNum = 0; // どれくらいレベルアップするか
        // 一気にレベルアップするかもなので、複数レベル先までの必要経験値を見ておく
        for (int i = (get_playerRank(rankPart) - 1); i < list.Count; i++)
        {
            // 次Lvまでの必要経験値が0より大きいか
            if (dataM.get_pointToNextLevel(rankPart, (i + 1)) > 0f)
            {
                // これ以上レベルアップしない
                break;
            }
            else
            {
                // そのレベルまでの必要経験値が0以下＝レベルアップ
                plusNum++;
            }
        }

        if (plusNum > 0)
        {
            // レベルアップ
            set_playerRank(rankPart, plusNum);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 所持金の加減算
    /// </summary>
    /// <param name="plusNum">加算する値</param>
    public void set_money(int plusNum)
    {
        DatabaseManager dataM = Manager_CommonGroup.instance.dataM;
        if ((plusNum < 0) && (-plusNum > money))
        {
            // 所持金よりも大きく減らそうとする場合
            money = 0;
        }
        else if((dataM.get_constFloat(1) - plusNum) < money)
        {
            // 上限を超えて加算しようとする場合
            money = (int)dataM.get_constFloat(1);
        }
        else
        {
            money += plusNum;
        }
    }
    public int get_money() {return money;}

    /// <summary>
    /// アルバムの最大保存数の加減算
    /// </summary>
    /// <param name="kind">処理するアルバムの種類</param>
    /// <param name="plusNum">加算する値</param>
    public void set_albumSize(albumKind kind, short plusNum)
    {
        DatabaseManager dataM = Manager_CommonGroup.instance.dataM;
        if ((plusNum < 0) && (-plusNum > albumSize[(int)kind]))
        {
            // 現サイズよりも大きく減らそうとする場合
            albumSize[(int)kind] = 0;
        }
        else if((dataM.get_constFloat(4) - plusNum) < albumSize[(int)kind])
        {
            // 上限を超えて加算しようとする場合
            albumSize[(int)kind] = (short)dataM.get_constFloat(4);
        }
        else
        {
            albumSize[(int)kind] += plusNum;
        }
    }
    public short get_albumSize(albumKind kind) {return albumSize[(int)kind];}

    /// <summary>
    /// アイテムの追加/減少処理
    /// </summary>
    /// <param name="id">追加購入するアイテムのID</param>
    /// <param name="plusNum">加算する値</param>
    public void set_havingItem(ushort id, sbyte plusNum)
    {
        DatabaseManager dataM = Manager_CommonGroup.instance.dataM;

        // アルバムサイズ(＝所持可能アイテム種数)を超えるときは、そのアイテム追加を拒否する必要がある
        // その確認が必要なのは、所持していないアイテムを入手した時。
        if((plusNum > 0) && (get_havingItem(id) == 0))
        {
            shopKind itemKind = dataM.list_items.list[id].shopKind; // 入手しようとしているアイテム種

            // 入手しようとしているアイテムと同じ種類のアイテムを、何種類所持しているか確認
            int num_having = 0;
            for(ushort i = 0; i < dataM.list_items.list.Count; i++)
            {
                // 参照しているアイテムの種類は、入手しようとしているアイテムの種類と同じか
                if(dataM.list_items.list[i].shopKind == itemKind)
                {
                    // そのアイテムを1つ以上持っているか
                    if(get_havingItem(i) > 0)
                    {
                        num_having++; // 所持アイテム種数をカウント
                    }
                }
            }
            // 所持アイテム種数がアルバムサイズ以上だったら、新たにアイテムを入手することはできない
            if(num_having >= get_albumSize(albumKind.storage))
            {
                devlog.logWarning("所持アイテム種数がアルバムサイズ以上なので、新たにアイテムを入手することはできません！");
                return;
            }
        }

        if ((plusNum < 0) && (-plusNum > havingItem[id]))
        {
            havingItem[id] = 0;
        }
        else if((dataM.get_constFloat(3) - plusNum) < havingItem[id])
        {
            havingItem[id] = (byte)dataM.get_constFloat(3);
        }
        else
        {
            havingItem[id] += (byte)plusNum;
        }
    }
    /// <summary>
    /// id番目のアイテムの所持数を取得する
    /// </summary>
    /// <param name="id">所持数を確認したいアイテムのID</param>
    /// <returns>そのアイテムの所持数</returns>
    public byte get_havingItem(ushort id) {return havingItem[id];}
    //public ushort get_havingItem_len() {return (ushort)havingItem.Length;}

    /// <summary>
    /// 型紙の追加（購入）処理
    /// </summary>
    /// <param name="patternId">追加購入する型紙のID</param>
    /// <param name="plusNum">追加する枚数</param>
    public void set_havingPattern_new(ushort patternId, sbyte plusNum)
    {
        Manager_CommonGroup commonM = Manager_CommonGroup.instance;

        ushort haveNum = get_havingPattern_haveNum(patternId); // 所持枚数
        ushort addDura = (ushort)(commonM.dataM.list_pattern.list[patternId].durability * plusNum); // 追加する耐久値

        // 所持数上限を超えないかチェック
        if ((haveNum + plusNum) <= commonM.dataM.get_constFloat(3))
        {
            havingPattern[patternId] += addDura;
        }
        else
        {
            havingPattern[patternId] = (ushort)(commonM.dataM.list_pattern.list[patternId].durability * commonM.dataM.get_constFloat(3));
            devlog.logError("最大所持数を超えて　追加購入しようとしていませんか？");
        }
    }
    /// <summary>
    /// 型紙使用を記録する（耐久-1）・0になれば、それは破棄したのと同様となる
    /// </summary>
    /// <param name="patternId">使用した型紙のID</param>
    public void set_havingPattern_use(ushort patternId)
    {
        if(havingPattern[patternId] > 0)
        {
            havingPattern[patternId]--;
        }
        else
        {
            devlog.logError("耐久が0の型紙を　使おうとしています！");
        }
    }
    /// <summary>
    /// 型紙の破棄処理・古いものから捨てる（耐久値を初期耐久値で割った値を切り捨てる）
    /// </summary>
    /// <param name="patternId">破棄する型紙のID</param>
    public void set_havingPattern_delete(ushort patternId)
    {
        if(havingPattern[patternId] > 0)
        {
            havingPattern[patternId] = (ushort)Mathf.Ceil(havingPattern[patternId] / Manager_CommonGroup.instance.dataM.list_pattern.list[patternId].durability);
        }
        else
        {
            devlog.logError("無い型紙を捨てることは　できません！");
        }
    }
    /// <summary>
    /// 型紙の耐久値から所持数を算出する（耐久値を初期耐久値で割った値を切り上げる）
    /// </summary>
    /// <param name="patternList">型紙のデータベース</param>
    /// <param name="patternId">所持数を確認する型紙のID</param>
    /// <returns>指定した型紙の所持数</returns>
    public ushort get_havingPattern_haveNum(ushort patternId)
    {
        ushort itemDura = Manager_CommonGroup.instance.dataM.list_pattern.list[patternId].durability;
        if (itemDura == 0)
        {
            // 万一のゼロ除算に備える
            return 0;
        }
        else
        {
            return (ushort)Mathf.Ceil((float)havingPattern[patternId] / (float)itemDura);
        }
    }
    /// <summary>
    /// 型紙の現耐久値を取得する
    /// </summary>
    /// <param name="patternList"></param>
    /// <param name="patternId">参照する型紙の番号</param>
    /// <returns>型紙の耐久値</returns>
    public int get_havingPattern_duraNum(byte patternId) {return Manager_CommonGroup.instance.dataM.list_pattern.list[patternId].durability;}

    /// <summary>
    /// 小紋データの削除
    /// </summary>
    /// <param name="id">削除したい小紋のID</param>
    /// <returns>正常に削除できたらtrue</returns>
    public bool set_havingKomon_delete(uint id)
    {
        if(id < havingKomon.Count)
        {
            havingKomon.RemoveAt((int)id);
            return true;
        }
        else
        {
            devlog.logError("小紋データの範囲外を指定しています");
            return false;
        }
    }
    /// <summary>
    /// 小紋の所持数を取得する
    /// </summary>
    /// <returns>小紋の所持数</returns>
    public int get_havingKomon_len() {return havingKomon.Count;}
    
    /// <summary>
    /// 任意の色レシピを取得する
    /// </summary>
    /// <param name="id">取得するレシピの番号</param>
    /// <returns>指定した番号の色レシピ</returns>
    public def_colorRecipe get_havingColor(byte id)
    {
        if(id < havingColor.Count)
        {
            return havingColor[id];
        }
        else
        {
            devlog.logError("リスト範囲外のレシピを取得しようとしています！");
            return null;
        }
    }
    /// <summary>
    /// 色レシピの所持数を取得する
    /// </summary>
    /// <returns>レシピの所持数</returns>
    public int get_havingColor_len() {return havingColor.Count;}
    /// <summary>
    /// 色レシピデータの削除
    /// </summary>
    /// <param name="id">削除したいレシピのID</param>
    /// <returns>正常に削除できたらtrue</returns>
    public bool set_havingColor_delete(uint id)
    {
        if(id < havingColor.Count)
        {
            havingColor.RemoveAt((int)id);
            return true;
        }
        else
        {
            devlog.logError("色データの範囲外を指定しています");
            return false;
        }
    }

    /// <summary>
    /// バトルのエリアをクリアしたフラグを立てたり
    /// </summary>
    /// <param name="areaId">いじるフラグのID</param>
    /// <param name="setValue">記録する値・クリアならtrue</param>
    /// <returns>正常に記録出来たらtrue</returns>
    public bool set_battle_areaClear(ushort areaId, bool setValue)
    {
        // 範囲外を指定していないか
        if(areaId < battle_areaClear.Length)
        {
            battle_areaClear[areaId] = setValue;
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 指定したIDのバトルエリアをクリアしているか取得する
    /// </summary>
    /// <param name="areaId">確認したいエリアのID</param>
    /// <returns>クリア済みであればtrue</returns>
    public bool get_battle_areaClear(ushort areaId)
    {
        if(areaId < battle_areaClear.Length)
        {
            return battle_areaClear[areaId];
        }
        else
        {
            devlog.logError($"battle_areaClearの範囲外を指定しています！ ：{areaId}");
            return false;
        }
    }
    /// <summary>
    /// バトルでクリアしたエリアの数を取得する
    /// </summary>
    /// <returns>クリアしたエリア数</returns>
    public ushort get_battle_areaClear_clearNum()
    {
        ushort result = 0;
        foreach (bool b in battle_areaClear)
        {
            if (b) { result++; }
        }
        return result;
    }

    /// <summary>
    /// 称号の入手フラグを操作する
    /// </summary>
    /// <param name="id">操作する称号のID</param>
    /// <param name="value">代入する値</param>
    public void set_havingTitle(byte id, bool value)
    {
        if (id < havingTitle.Length)
        {
            havingTitle[id] = value;
        }
        else
        {
            devlog.logError($"称号データの範囲外を参照しています！：{id}");
        }
    }
    /// <summary>
    /// 指定したIDの称号を取得しているか確認する
    /// </summary>
    /// <param name="id">確認したい称号のID</param>
    /// <returns>取得済みであればtrue</returns>
    public bool get_havingTitle(byte id)
    {
        if(id < havingTitle.Length)
        {
            return havingTitle[id];
        }
        else
        {
            devlog.logError($"称号データの範囲外を参照しています！：{id}");
            return false;
        }
    }

    public void set_favorite_color(Color setCol) { favorite_color = setCol; }
    public Color get_favorite_color() { return favorite_color; }
    public void set_favorite_color_name(string str) { favorite_color_name = str; }
    public string get_favorite_color_name()
    { 
        if(favorite_color_name != "")
        {
            return favorite_color_name;
        }
        else
        {
            return "（<r=なや>悩</r>み中……）";
        }
        
    }
    /// <summary>
    /// おきにいりの柄を設定する
    /// </summary>
    /// <param name="setId">設定する柄の番号</param>
    /// <returns>正常に登録できたか</returns>
    public bool set_favorite_pattern(byte setId)
    {
        if(setId < Manager_CommonGroup.instance.dataM.list_pattern.list.Count)
        {
            favorite_pattern = setId;
            return true;
        }
        else
        {
            devlog.logError("型紙データの範囲外を指定しています！：{setId}");
            return false;
        }
    }
    public byte get_favorite_pattern() { return favorite_pattern; }

    /// <summary>
    /// ていさつの残り時間（分）を取得する
    /// </summary>
    /// <returns>ていさつの残り時間・ていさつしていなければ9999f</returns>
    public float get_teisatsuLeftTime()
    {
        // ていさつ中か
        if (doingReconnaissance)
        {
            TimeSpan span = DateTime.Now - DateTime.Parse(time_reconnaissanceStart); // 開始からの経過時間
            return (float)(time_reconnaissanceRequired - span.TotalMinutes);
        }
        else
        {
            devlog.logError("ていさつ していないので、残り時間を求めることができません！");
            return 9999f;
        }
    }
}