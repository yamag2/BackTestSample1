using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class MyUtil
{

    public static bool Contains(params string[] sDtStr)
    {
        bool blRet = true;
        return blRet;
    }

    public static string Concat(params string[] sDtStr)
    {
        StringBuilder sBuilder = new StringBuilder();
        foreach (var elm in sDtStr)
        {
            sBuilder.Append(elm);
        }
        return sBuilder.ToString();
    }

    public static string FillZero(string param, int length)
    {
        short iTmp = 0;
        if (string.IsNullOrEmpty(param) || Int16.TryParse(param, out iTmp) == false || param.Length == length)
        {
            return param;
        }
        return Concat(new string[] { "0", param });
    }

    public static string GetYYYYMMDD(DateTime param)
    {
        if (param == null)
        {
            return Concat(new string[] { DateTime.MinValue.Year.ToString(), DateTime.MinValue.Month.ToString(), DateTime.MinValue.Date.ToString() });
        }
        return Concat(new string[] { param.Year.ToString(), FillZero(param.Month.ToString(), 2), FillZero(param.Day.ToString(), 2) });
    }

    /// <summary>
    /// YYYYMMDD Format String -->Datetime Converting
    /// </summary>
    /// <param name="sDtStr"></param>
    /// <param name="iDays"></param>
    /// <returns></returns>
    public static DateTime ConvertToDatetime(string sDtStr, int iDays)
    {
        DateTime retDt = ConvertToDatetime(sDtStr);
        return retDt.AddDays(iDays);
    }

    /// <summary>
    /// YYYYMMDD Format String -->Datetime Converting
    /// </summary>
    /// <param name="sDtStr"></param>
    /// <returns></returns>
    public static DateTime ConvertToDatetime(string sDtStr)
    {
        int shChk = 0;
        DateTime dtRet = DateTime.MinValue;
        if (string.IsNullOrEmpty(sDtStr) || sDtStr.Length < 8)
        {
            return dtRet;
        }
        if (!Int32.TryParse(sDtStr, out shChk))
        {
            return dtRet;
        }
        dtRet = DateTime.Parse(Concat(sDtStr.Substring(0, 4), "/", sDtStr.Substring(4, 2), "/", sDtStr.Substring(6, 2)));
        return dtRet;
    }

    /// <summary>
    /// Convert CSV To ArrayList
    /// </summary>
    /// <param name="csvText">CSVの内容が入ったString</param>
    /// <returns>変換結果のArrayList</returns>
    public static ArrayList CsvToArrayList(string csvText)
    {
        //前後の改行を削除しておく
        csvText = csvText.Trim(new char[] { '\r', '\n' });

        System.Collections.ArrayList csvRecords =
            new System.Collections.ArrayList();
        System.Collections.ArrayList csvFields =
            new System.Collections.ArrayList();

        int csvTextLength = csvText.Length;
        int startPos = 0, endPos = 0;
        string field = "";

        while (true)
        {
            //空白を飛ばす
            while (startPos < csvTextLength &&
                (csvText[startPos] == ' ' || csvText[startPos] == '\t'))
            {
                startPos++;
            }

            //データの最後の位置を取得
            if (startPos < csvTextLength && csvText[startPos] == '"')
            {
                //"で囲まれているとき
                //最後の"を探す
                endPos = startPos;
                while (true)
                {
                    endPos = csvText.IndexOf('"', endPos + 1);
                    if (endPos < 0)
                    {
                        throw new ApplicationException("\"が不正");
                    }
                    //"が2つ続かない時は終了
                    if (endPos + 1 == csvTextLength || csvText[endPos + 1] != '"')
                    {
                        break;
                    }
                    //"が2つ続く
                    endPos++;
                }

                //一つのフィールドを取り出す
                field = csvText.Substring(startPos, endPos - startPos + 1);
                //""を"にする
                field = field.Substring(1, field.Length - 2).Replace("\"\"", "\"");

                endPos++;
                //空白を飛ばす
                while (endPos < csvTextLength &&
                    csvText[endPos] != ',' && csvText[endPos] != '\n')
                {
                    endPos++;
                }
            }
            else
            {
                //"で囲まれていない
                //カンマか改行の位置
                endPos = startPos;
                while (endPos < csvTextLength &&
                    csvText[endPos] != ',' && csvText[endPos] != '\n')
                {
                    endPos++;
                }

                //一つのフィールドを取り出す
                field = csvText.Substring(startPos, endPos - startPos);
                //後の空白を削除
                field = field.TrimEnd();
            }

            //フィールドの追加
            csvFields.Add(field);

            //行の終了か調べる
            if (endPos >= csvTextLength || csvText[endPos] == '\n')
            {
                //行の終了
                //レコードの追加
                csvFields.TrimToSize();
                csvRecords.Add(csvFields);
                csvFields = new System.Collections.ArrayList(
                    csvFields.Count);

                if (endPos >= csvTextLength)
                {
                    //終了
                    break;
                }
            }

            //次のデータの開始位置
            startPos = endPos + 1;
        }

        csvRecords.TrimToSize();
        return csvRecords;
    }
}
