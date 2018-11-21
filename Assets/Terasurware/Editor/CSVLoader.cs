using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVLoader
{
    List<string[]> _rows = null;
    bool _loaded;

    List<Dictionary<string, string>> m_rows = null;

    int _noCols = 0;

    string message;

    public int Cols
    {
        get
        {
            return _noCols;
        }
    }

    public int Rows
    {
        get
        {
            if (m_rows == null) return 0;

            return m_rows.Count;
        }
    }

    public List<string> Keys;

    public CSVLoader()
    {
        _loaded = false;
        m_rows = null;
    }

    public bool Load(string text, char Delimiter = '\t')
    {
        try
        {
            StringReader sr = new StringReader(text);

            m_rows = new List<Dictionary<string, string>>();

            _noCols = 0;

            Dictionary<int, string> keys = new Dictionary<int, string>();

            //HashSet<int> setColumnExceptComments = new HashSet<int>();

            int noRows = 0;
            string aLine;
            while ((aLine = sr.ReadLine()) != null)
            {
                aLine.Trim();

                if (aLine.Length <= 0)
                    continue;

                string[] cols = aLine.Split(Delimiter);
                if (cols.Length > 0 && cols[0] == "")
                    continue;

                if (noRows++ == 0)
                {

                    for (int i = 0; i < cols.Length; ++i)
                    {
                        if (cols[i].Length > 0 && !cols[i].StartsWith("*"))
                        {
                            keys.Add(i, cols[i]);
                        }
                    }
                    _noCols = keys.Count;

                    if (_noCols == 0)
                        throw new Exception("There is no valid columns");

                    continue;
                }



                Dictionary<string, string> in_row = new Dictionary<string, string>();
                foreach (var item in keys)
                {
                    if (cols.Length <= item.Key)
                    {
                        Debug.LogError(aLine);
                        continue;
                    }
                    cols[item.Key].Trim();
                    in_row.Add(item.Value, cols[item.Key]);
                }
                m_rows.Add(in_row);
            }

            sr.Close();

            Keys = new List<string>(keys.Values);

            _loaded = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message.ToString());
            return false;
        }

        return true;
    }

    // 값을 바로 던진 이유는..
    // 어짜피 우리는 바이너리로 묶으니까 다 걸러진 값을 쓰기 때문에
    // 여기서만 오류 출력해주면 끝남
    public T ReadValue<T>(string col, int row, T defaultVal = default(T))
    {
        T Value = defaultVal;

        if (!isValidValue(col, row))
        {
            return Value;
        }

        if (typeof(T).IsEnum)
        {
            Value = (T)Enum.ToObject(typeof(T), int.Parse(m_rows[row][col]));
        }
        else
        {
            Value = (T)Convert.ChangeType(m_rows[row][col], typeof(T));
        }

        return Value;
    }

    // bool의 type은 TrueString, FalseString
    // csv에서 그렇게 넣어주면 되긴 하는데 지금 당장은 이렇게 따로 뺌.
    public bool ReadValue(string col, int row, bool def = false)
    {
        bool Value = def;

        if (!isValidValue(col, row)) return Value;

        string str = m_rows[row][col].ToLower();
        if (str == "1" || str.Equals("true"))
        { Value = true; }
        else if (str == "0" || str == "false")
        { Value = false; }
        else
        {
            Debug.LogError("undefined type : " + str);
        }
            return Value;
    }

    // 값이 정상적으로 들어가 있는지.
    public bool isValidValue(string col, int row)
    {

        if (!_loaded)
            return false;

        if (m_rows == null)
            return false;

        if (row >= m_rows.Count)
            return false;

        if (string.IsNullOrEmpty(col))
            return false;

        if (!m_rows[row].ContainsKey(col))
            return false;

        if (m_rows[row][col] == "" || m_rows[row][col] == null)
            return false;

        return true;
    }
}
