// http://www.realcoding.net/articles/struktura-dbf-failov-dlya-neprodvinutykh.html
// http://www.autopark.ru/ASBProgrammerGuide/DBFSTRUC.HTM#Table_9

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace OSM2SHP
{    
    public class FieldInfos : List<FieldInfo>
    {
        public void Add(FieldInfo fi)
        {
            base.Add(fi);
        }

        public void Add(string fName, byte fLength, char fType)
        {
            this.Add(new FieldInfo(fName,fLength,fType));
        }

        private void ReIndex()
        {
            ushort res = 1;
            if (this.Count > 0)
                    for (int i = 0; i < this.Count; i++)
                    {
                        this[i].offset = res;
                        res += this[i].FLength;
                    };
        }

        public ushort RecordSize
        {
            get
            {
                int res = 1;
                if (this.Count > 0)
                    for (int i = 0; i < this.Count; i++)
                        res += this[i].FLength;
                if (res > ushort.MaxValue)
                    throw new Exception("Record size is too big! Max allowed size is " + ushort.MaxValue.ToString());
                return (ushort)res;
            }
        }

        public FieldInfo this[string fieldName]
        {
            get
            {
                if (this.Count > 0)
                    for (int i = 0; i < this.Count; i++)
                        if (fieldName == this[i].FName)
                            return this[i];
                return null;
            }
        }

        public void Zero()
        {
            if (this.Count > 0)
                for (int i = 0; i < this.Count; i++)
                    this[i].FValue = null;
        }
    }

    public class FieldInfo
    {
        public string FName;
        public byte FLength;
        public char FType;
        public ushort offset = 0;

        public object FValue = null;

        public FieldInfo(string fName, byte fLength, char fType)
        {
            this.FName = fName;
            this.FLength = fLength;
            this.FType = fType;
        }

        public string GName
        {
            get
            {
                string nam = this.FName;
                if (nam.IndexOf("addr:") == 0)
                {
                    nam = nam.Substring(4);
                    if (nam.IndexOf(":", 1) > 0)
                        nam = nam.Substring(0, nam.IndexOf(":", 1));
                };
                nam = nam.Replace(":", "_");
                return nam.ToUpper();
            }
        }

        public byte[] BName(Encoding encoding)
        {            
            byte[] res = new byte[11];
            byte[] bb = encoding.GetBytes(this.GName);
            for (int i = 0; (i < bb.Length) && (i < res.Length); i++)
                res[i] = bb[i];
            return res;
        }
    }

    public class DBFWriter: FileStream
    {
        private string filename;
        private uint records = 0;
        private sbyte MaxTagFieldsUsed = 0;
        private int MaxTagsCount = 0;
        private int MaxTagsAggrCount = 0;
        private int MaxTagsStrLength = 0;
        private CodePageSet _cp = CodePageSet.Default;
        private FieldInfos _FieldInfos = new FieldInfos();
        public CodePageList CodePages = new CodePageList();
        private bool _tenHeaderMode = true;

        private OSMPBFReader.MyBitConverter bc = new OSMPBFReader.MyBitConverter(true);

        public DBFWriter(string fileName)
            : base(fileName, FileMode.Create, FileAccess.ReadWrite)
        {
            filename = fileName;
            _cp = CodePageSet.Default;
            WriteHeader();
        }

        public DBFWriter(string fileName, byte dbfCodePage)
            : base(fileName, FileMode.Create, FileAccess.ReadWrite)
        {
            filename = fileName;
            SetCodePage(dbfCodePage);
            WriteHeader();
        }        

        public DBFWriter(string fileName, FileMode mode) : base(fileName, mode, FileAccess.ReadWrite)
        {
            filename = fileName;
            _cp = CodePageSet.Default;
            WriteHeader();
        }

        public DBFWriter(string fileName, FileMode mode, byte dbfCodePage): base(fileName, mode, FileAccess.ReadWrite) 
        {
            filename = fileName;
            SetCodePage(dbfCodePage);
            WriteHeader();
        }

        public bool ShortenFieldNameMode { get { return _tenHeaderMode; } set { _tenHeaderMode = value; } }

        public byte FieldsCount
        {
            get
            {
                if (_FieldInfos == null) return 0;
                if (_FieldInfos.Count == 0) return 0;
                return (byte)_FieldInfos.Count;
            }
        }

        public ushort RecordSize
        {
            get
            {
                if (_FieldInfos == null) return 0;
                if (_FieldInfos.Count == 0) return 0;
                return _FieldInfos.RecordSize;
            }
        }

        private void SetCodePage(byte dbfCodePage)
        {
            _cp = CodePages[dbfCodePage];
            if (_cp.headerCode == 0)
            {
                base.Close();
                throw new Exception("Unknown Code Page");
            };
            try
            {
                if (_cp.Encoding == null)
                {
                    base.Close();
                    throw new Exception("Unknown Code Page " + _cp.codePage);
                };
            }
            catch (Exception ex)
            {
                base.Close();
                throw new Exception("Unknown Code Page "+_cp.codePage+"\r\n"+ex.Message);
            };
        }

        private void WriteHeader()
        {
            byte[] buff = new byte[0];
            this.Position = 0;
            this.WriteByte(0x03);                               // 0 - TYPE + MEMO 0x83
            this.WriteByte((byte)(DateTime.UtcNow.Year % 100)); // 1 - YY
            this.WriteByte((byte)DateTime.UtcNow.Month);        // 2 - MM
            this.WriteByte((byte)DateTime.UtcNow.Day);          // 3 - DD
            buff = bc.GetBytes((uint)0); 
            this.Write(buff, 0, buff.Length);                   // 4 - records count
            buff = bc.GetBytes((ushort)0); 
            this.Write(buff, 0, buff.Length);                   // 8 - header size
            buff = bc.GetBytes((ushort)0); 
            this.Write(buff, 0, buff.Length);                   // 10 - record size
            this.WriteByte(0x00);                               // 12 - Reserved
            this.WriteByte(0x00);                               // 13 - Reserved
            this.WriteByte(0x00);                               // 14 - Ignored
            this.WriteByte(0x00);                               // 15 - Normal
            buff = new byte[12];
            this.Write(buff, 0, buff.Length);                   // 16 - reserved
            this.WriteByte(0x00);                               // 28 - No Index
            this.WriteByte((byte)_cp.headerCode);               // 29 - Code Page
            this.WriteByte(0x00);                               // 30 - Reserved
            this.WriteByte(0x00);                               // 31 - Reserved
        }

        public void WriteHeader(FieldInfos fields)
        {
            _FieldInfos = fields;
            WriteHeader();

            byte[] buff = new byte[0];
            ushort hdr_size = 33;
            int mhl = 0;

            string hfn = Path.GetDirectoryName(filename) + @"\" + Path.GetFileNameWithoutExtension(filename) + ".txt";
            FileStream fs = new FileStream(hfn, FileMode.Create, FileAccess.Write);
            {
                buff = System.Text.Encoding.GetEncoding(1251).GetBytes(String.Format("Кодировка файла: {0} - {1}\r\n\r\n", _cp.codePage, _cp.Encoding.EncodingName));
                fs.Write(buff, 0, buff.Length);
                buff = _cp.Encoding.GetBytes("Описание полей:\r\n\r\n");
                fs.Write(buff, 0, buff.Length);
            };

            string[] descriptions = GetFieldDescriptions(fields);
            for (int i = 0; i < fields.Count; i++)
            {
                buff = _cp.Encoding.GetBytes(String.Format("{0:00} - ", i + 1));
                fs.Write(buff, 0, buff.Length);
                //
                buff = fields[i].BName(_cp.Encoding);
                if (fields[i].GName.Length > 11)
                    buff[10] = (byte)(0x41 + (mhl++));
                if(_tenHeaderMode) buff[10] = 0;
                this.Write(buff, 0, buff.Length);        // 0 - Field Name
                {
                    for (int b = 0; b < buff.Length; b++)
                        if (buff[b] == 0)
                            buff[b] = 0x20;
                    fs.Write(buff, 0, buff.Length);
                    buff = _cp.Encoding.GetBytes(String.Format(" - {0}[{1}] - {2}\r\n", fields[i].FName, fields[i].FLength, descriptions[i]));
                    fs.Write(buff, 0, buff.Length);
                };

                buff = new byte[] { (byte)fields[i].FType };
                this.Write(buff, 0, buff.Length);        // 11 - Field Type
                buff = bc.GetBytes((int)fields[i].offset);
                this.Write(buff, 0, buff.Length);        // 12 - Field Offset
                this.WriteByte((byte)fields[i].FLength); // 16 - Field Size
                this.WriteByte(0x00);                    // 17 - No Decimal Point
                buff = new byte[14];
                this.Write(buff, 0, buff.Length);        // 18 - Reserved

                hdr_size += 32;
            };
            this.WriteByte(13); //TERMINAL BYTE    
            this.WriteByte(26); //TERMINAL BYTE

            {
                buff = _cp.Encoding.GetBytes(String.Format("\r\n\r\nВсего полей: {0}\r\n", fields.Count));
                fs.Write(buff, 0, buff.Length);
                buff = _cp.Encoding.GetBytes(String.Format("Размер заголовка: {0}\r\n", (fields.Count + 1) * 32));
                fs.Write(buff, 0, buff.Length);
                buff = _cp.Encoding.GetBytes(String.Format("Размер записи: {0}\r\n", RecordSize));
                fs.Write(buff, 0, buff.Length);
                fs.Close();
            };

            this.Position = 8;
            buff = bc.GetBytes(hdr_size); // header size
            this.Write(buff, 0, buff.Length);
            buff = bc.GetBytes(fields.RecordSize); // record size
            this.Write(buff, 0, buff.Length);

            this.Position = hdr_size;
        }

        public long WriteRecord(Dictionary<string, object> record)
        {
            return WriteRecord(record, 0, 0, 0, 0);
        }

        public long WriteRecord(Dictionary<string,object> record, sbyte TAGS_fields_used, int MAX_TAGS, int AGGR_TAGS, int str_len)
        {
            long ret_pos = (int)this.Position;

            if (TAGS_fields_used > MaxTagFieldsUsed) MaxTagFieldsUsed = TAGS_fields_used;
            if (MAX_TAGS > MaxTagsCount) MaxTagsCount = MAX_TAGS;
            if (AGGR_TAGS > MaxTagsAggrCount) MaxTagsAggrCount = AGGR_TAGS;
            if (str_len > MaxTagsStrLength) MaxTagsStrLength = str_len;
            if (record == null) return ret_pos;
            if (record.Count == 0) return ret_pos;
            if ((_FieldInfos == null) || (_FieldInfos.Count == 0)) return ret_pos;

            _FieldInfos.Zero();
            foreach (KeyValuePair<string, object> kvp in record)
            {
                FieldInfo fi = _FieldInfos[kvp.Key];
                if (fi != null)
                    fi.FValue = kvp.Value;
            };

            
            this.WriteByte(0x20); //_BEGIN RECORD_ //

            for (int i = 0; i < _FieldInfos.Count; i++)
            {
                byte[] def = new byte[_FieldInfos[i].FLength];
                if (_FieldInfos[i].FType == 'N')
                {
                    for (int x = 0; x < def.Length; x++)
                        def[x] = (byte)' ';                    
                    if (_FieldInfos[i].FValue != null)
                    {
                        byte[] buff = _cp.Encoding.GetBytes(_FieldInfos[i].FValue.ToString().Replace(",","."));
                        if (buff.Length > def.Length)
                            throw new Exception("Numeric Value is too large: " + _FieldInfos[i].FValue.ToString());
                        Array.Copy(buff, 0, def, def.Length - buff.Length, buff.Length);
                    };
                };
                if (_FieldInfos[i].FType == 'C')
                {
                    if ((_FieldInfos[i].FName == "CRTD_FROM") && (_FieldInfos[i].FValue == null))
                        _FieldInfos[i].FValue = "N"; // From Node
                    if (_FieldInfos[i].FValue != null)
                    {
                        byte[] buff = _cp.Encoding.GetBytes(_FieldInfos[i].FValue.ToString());
                        if (buff.Length > def.Length)
                            Array.Copy(buff, def, def.Length);
                        else
                            Array.Copy(buff, def, buff.Length);
                    };
                };
                if (_FieldInfos[i].FType == 'L')
                {
                    byte[] buff = new byte[] { (_FieldInfos[i].FValue == null) || (((bool)_FieldInfos[i].FValue) != true) ? (byte)((char)'F') : (byte)((char)'T')};
                    if (buff.Length > def.Length)
                        Array.Copy(buff, def, def.Length);
                    else
                        Array.Copy(buff, def, buff.Length);
                };
                this.Write(def, 0, def.Length);
            };

            records++;
            return ret_pos;            
        }
       
        public int UpdateJoinRecord(Dictionary<string, object> record, long sPOS, out bool TVupdated)
        {
            TVupdated = false;

            if (sPOS < 1) return 0;
            if (record == null) return 0;
            if (record.Count == 0) return 0;
            if ((_FieldInfos == null) || (_FieldInfos.Count == 0)) return 0;

            int added = 0;
            
            long tmppos = this.Position;
            this.Position = sPOS;

            // Only For WriteRelJoinsHeaders
            byte[] rba = new byte[this.RecordSize];
            this.Read(rba, 0, rba.Length);
            int spos = 1;
            for (int i = 0; i < _FieldInfos.Count; i++)
            {
                string val = _cp.Encoding.GetString(rba, spos, _FieldInfos[i].FLength).Trim('\0').Trim();
                if (_FieldInfos[i].FName == "INDEX")
                    record["INDEX"] = int.Parse(val);
                if ((_FieldInfos[i].FName == "LINE_ID") && (val != record["LINE_ID"].ToString()))
                {
                    this.Position = tmppos;
                    return 0;
                };
                if (_FieldInfos[i].FName == "R_COUNT")
                {
                    record[_FieldInfos[i].FName] = (int.Parse(val) + 1).ToString();
                };
                if (_FieldInfos[i].FName == "R_VALID")
                {
                    if (val == "F") record[_FieldInfos[i].FName] = false; 
                };
                if ((_FieldInfos[i].FType == 'C') && (val.Length > 0))
                {
                    if (_FieldInfos[i].FName == "FL")
                    {
                        List<string> vv = new List<string>();
                        vv.AddRange(val.Split(new char[] { ';' }, StringSplitOptions.None));
                        if (record.ContainsKey("FL"))
                        {
                            string[] nnv = record["FL"].ToString().Split(new char[] { ';' }, StringSplitOptions.None);
                            if ((nnv != null) && (nnv.Length > 0))
                            {
                                foreach (string n in nnv)
                                    if (!vv.Contains(n))
                                    {
                                        vv.Add(n);
                                        added++;
                                    };
                                string res = "";
                                foreach (string n in vv)
                                    if (res.Length > 0)
                                        res += ";" + n;
                                    else
                                        res += n;
                                record["FL"] = res; // val + ";" + record["FL"];
                            }
                            else
                            {
                                record["FL"] = val;
                            };
                        }
                        else
                            record.Add("FL", val);

                        record["R_SAVED"] = vv.Count;
                    };
                    if (_FieldInfos[i].FName == "TV")
                    {
                        if ((val + ";").Contains(record["TV"] + ";"))
                        {
                            this.Position = tmppos;
                            return 0;
                        };
                        record["TV"] = val + ";" + record["TV"];
                    };
                };
                spos += _FieldInfos[i].FLength; 
            };
            this.Position = sPOS;

            _FieldInfos.Zero();
            foreach (KeyValuePair<string, object> kvp in record)
            {
                FieldInfo fi = _FieldInfos[kvp.Key];
                if (fi != null)
                    fi.FValue = kvp.Value;
            };
            this.WriteByte(0x20); //_BEGIN RECORD_ //

            for (int i = 0; i < _FieldInfos.Count; i++)
            {
                byte[] def = new byte[_FieldInfos[i].FLength];
                if (_FieldInfos[i].FType == 'N')
                {
                    for (int x = 0; x < def.Length; x++)
                        def[x] = (byte)' ';
                    if (_FieldInfos[i].FValue != null)
                    {
                        byte[] buff = _cp.Encoding.GetBytes(_FieldInfos[i].FValue.ToString().Replace(",", "."));
                        if (buff.Length > def.Length)
                            throw new Exception("Numeric Value is too large: " + _FieldInfos[i].FValue.ToString());
                        Array.Copy(buff, 0, def, def.Length - buff.Length, buff.Length);
                    };
                };
                if (_FieldInfos[i].FType == 'C')
                {
                    if ((_FieldInfos[i].FName == "CRTD_FROM") && (_FieldInfos[i].FValue == null))
                        _FieldInfos[i].FValue = "N"; // From Node
                    if (_FieldInfos[i].FValue != null)
                    {
                        byte[] buff = _cp.Encoding.GetBytes(_FieldInfos[i].FValue.ToString());
                        if (buff.Length > def.Length)
                            Array.Copy(buff, def, def.Length);
                        else
                            Array.Copy(buff, def, buff.Length);
                    };
                };
                if (_FieldInfos[i].FType == 'L')
                {
                    byte[] buff = new byte[] { (_FieldInfos[i].FValue == null) || (((bool)_FieldInfos[i].FValue) != true) ? (byte)((char)'F') : (byte)((char)'T') };
                    if (buff.Length > def.Length)
                        Array.Copy(buff, def, def.Length);
                    else
                        Array.Copy(buff, def, buff.Length);
                };
                this.Write(def, 0, def.Length);
            };

            this.Position = tmppos;
            TVupdated = true;
            return added;
        }

        public uint WritedRecords
        {
            get
            {
                return records;
            }           
        }

        public void WriteRecordsCount(uint count)
        {
            long pos = this.Position;
            this.Position = 4;
            byte[] buff = bc.GetBytes(count);
            this.Write(buff, 0, buff.Length);
            this.Position = pos;
        }

        public FieldInfos FieldInfos
        {
            get
            {                
                return _FieldInfos;
            }
        }

        public override void Close()
        {
            WriteByte(26); //TERMINAL BYTE
            WriteRecordsCount(records);
            
            {
                string hfn = Path.GetDirectoryName(filename) + @"\" + Path.GetFileNameWithoutExtension(filename) + ".txt";
                FileStream fs = new FileStream(hfn, FileMode.OpenOrCreate, FileAccess.Write);
                fs.Position = fs.Length;
                byte[] buff;
                if (filename.IndexOf("[J") > 0)
                {
                    buff = System.Text.Encoding.GetEncoding(1251).GetBytes("\r\n");
                    fs.Write(buff, 0, buff.Length);
                    buff = System.Text.Encoding.GetEncoding(1251).GetBytes("R_COUNT > R_SAVED - Вероятнее всего запрет поворота указан не из начала/конца\r\n");
                    fs.Write(buff, 0, buff.Length);
                    buff = System.Text.Encoding.GetEncoding(1251).GetBytes("R_COUNT < R_SAVED - Вероятнее всего линия не была экспортирована (найдена)\r\n");
                    fs.Write(buff, 0, buff.Length);
                    buff = System.Text.Encoding.GetEncoding(1251).GetBytes("F.../L... - Из начала (F) или конца (L) в линию WAY_ID\r\n");
                    fs.Write(buff, 0, buff.Length);
                    buff = System.Text.Encoding.GetEncoding(1251).GetBytes("T...V... - В (T) линию WAY_ID через (V) точку NODE_ID\r\n");
                    fs.Write(buff, 0, buff.Length);
                    //R_COUNT <> R_SAVED
                }
                else if (filename.IndexOf("[M") > 0)
                {

                }
                else if (filename.IndexOf("[N") > 0)
                {

                }
                else
                {
                    buff = System.Text.Encoding.GetEncoding(1251).GetBytes(String.Format("Максимальное число всех тегов в одном объекте: {0}\r\n", MaxTagsCount));
                    fs.Write(buff, 0, buff.Length);
                    buff = System.Text.Encoding.GetEncoding(1251).GetBytes(String.Format("Максимальная длина строки описания всех тегов: {0}\r\n", MaxTagsStrLength));
                    fs.Write(buff, 0, buff.Length);
                    buff = System.Text.Encoding.GetEncoding(1251).GetBytes(String.Format("Максимальное число агрегированных тегов в одном объекте: {0}\r\n", MaxTagsAggrCount));
                    fs.Write(buff, 0, buff.Length);
                    buff = System.Text.Encoding.GetEncoding(1251).GetBytes(String.Format("Максимальное число использованных агрегированных полей: {0}\r\n", MaxTagFieldsUsed));
                    fs.Write(buff, 0, buff.Length);
                };
                buff = System.Text.Encoding.GetEncoding(1251).GetBytes(String.Format("Всего записей: {0}\r\n", records));
                fs.Write(buff, 0, buff.Length);
                buff = System.Text.Encoding.GetEncoding(1251).GetBytes(String.Format("Создан: {0}\r\n", DateTime.Now));
                fs.Write(buff, 0, buff.Length);
                fs.Close();
            };            

            base.Close();
        }

        public string[] GetFieldDescriptions(FieldInfos fields)
        {
            string[] res = new string[fields.Count];
            for (int i = 0; i < fields.Count; i++)
                if (fields[i].FName.StartsWith("addr:"))
                    res[i] = "Адресная информация";

            string file = AppDomain.CurrentDomain.BaseDirectory + @"\FieldsDescriptions.txt";
            if (!File.Exists(file)) return res;

            try
            {
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.GetEncoding(1251));
                bool skip = true;
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (skip && (line.StartsWith("-- Основные поля:"))) skip = false;
                    if (skip) continue;
                    for (int i = 0; i < fields.Count; i++)
                    {
                        if (fields[i].FName.StartsWith("TAGS_") && (!Char.IsLetter(fields[i].FName[5])))
                        {
                            if (line.StartsWith("TAGS_40"))
                                res[i] = line.Substring(line.IndexOf("-") + 1).Trim();
                        }
                        else if (line.StartsWith(fields[i].FName))
                            res[i] = line.Substring(line.IndexOf("-") + 1).Trim();
                    };

                    if (line.StartsWith("-- Адресные поля:")) break;
                };
                sr.Close();
                fs.Close();
            }
            catch { };
            return res;
        }
    }

    public class CodePageSet
    {
        public byte headerCode = 0;
        public int codePage = 0;
        public string codeName = "UNKNOWN";

        public Encoding Encoding
        {
            get
            {
                return System.Text.Encoding.GetEncoding(codePage);
            }
        }

        public CodePageSet(){}

        public static CodePageSet Default
        {
            get
            {
                CodePageSet result = new CodePageSet();
                result.headerCode = 201;
                result.codePage = 1251;
                result.codeName = @"Russian Windows \ Windows-1251 [0xC9]";
                return result;
            }
        }

        public override string ToString()
        {
            return codeName;
        }
    }

    public class CodePageList : List<CodePageSet>
    {
        public CodePageList()
        {
            this.Add(204, 01257, "Baltic Windows");
            this.Add(079, 00950, "Chinese Big5 (Taiwan)");
            this.Add(077, 00936, "Chinese GBK (PRC)");
            this.Add(122, 00936, "PRC GBK");
            this.Add(031, 00852, "Czech OEM");
            this.Add(008, 00865, "Danish OEM");
            this.Add(009, 00437, "Dutch OEM");
            this.Add(010, 00850, "Dutch OEM*");
            this.Add(025, 00437, "English OEM (Great Britain)");
            this.Add(026, 00850, "English OEM (Great Britain)*");
            this.Add(027, 00437, "English OEM (US)");
            this.Add(055, 00850, "English OEM (US)*");
            this.Add(200, 01250, "Eastern European Windows");
            this.Add(100, 00852, "Eastern European MS-DOS");
            this.Add(151, 10029, "Eastern European Macintosh");
            this.Add(011, 00437, "Finnish OEM");
            this.Add(013, 00437, "French OEM");
            this.Add(014, 00850, "French OEM*");
            this.Add(029, 00850, "French OEM*2");
            this.Add(028, 00863, "French OEM (Canada)");
            this.Add(108, 00863, "French-Canadian MS-DOS");
            this.Add(015, 00437, "German OEM");
            this.Add(016, 00850, "German OEM*");
            this.Add(203, 01253, "Greek Windows");
            this.Add(106, 00737, "Greek MS-DOS (437G)");
            this.Add(134, 00737, "Greek OEM");
            this.Add(152, 00006, "Greek Macintosh");
            this.Add(121, 00949, "Hangul (Wansung)");
            this.Add(034, 00852, "Hungarian OEM");
            this.Add(103, 00861, "Icelandic MS-DOS");
            this.Add(017, 00437, "Italian OEM");
            this.Add(018, 00850, "Italian OEM*");
            this.Add(019, 00932, "Japanese Shift-JIS");
            this.Add(123, 00932, "Japanese Shift-JIS 2");
            this.Add(104, 00895, "Kamenicky (Czech) MS-DOS");
            this.Add(078, 00949, "Korean (ANSI/OEM)");
            this.Add(105, 00620, "Mazovia (Polish) MS-DOS");
            this.Add(102, 00865, "Nordic MS-DOS");
            this.Add(023, 00865, "Norwegian OEM");
            this.Add(035, 00852, "Polish OEM");
            this.Add(036, 00860, "Portuguese OEM");
            this.Add(037, 00850, "Portuguese OEM*");
            this.Add(064, 00852, "Romanian OEM");
            this.Add(201, 01251, "Russian Windows");
            this.Add(101, 00866, "Russian MS-DOS");
            this.Add(038, 00866, "Russian OEM");
            this.Add(150, 10007, "Russian Macintosh");
            this.Add(135, 00852, "Slovenian OEM");
            this.Add(089, 01252, "Spanish ANSI");
            this.Add(020, 00850, "Spanish OEM*");
            this.Add(021, 00437, "Swedish OEM");
            this.Add(022, 00850, "Swedish OEM*");
            this.Add(024, 00437, "Spanish OEM");
            this.Add(087, 01250, "Standard ANSI");
            this.Add(003, 01252, "Standard Windows ANSI Latin I");
            this.Add(002, 00850, "Standard International MS-DOS");
            this.Add(004, 10000, "Standard Macintosh");
            this.Add(120, 00950, "Taiwan Big 5");
            this.Add(080, 00874, "Thai (ANSI/OEM)");
            this.Add(124, 00874, "Thai Windows/MS–DOS");
            this.Add(202, 01254, "Turkish Windows");
            this.Add(107, 00857, "Turkish MS-DOS");
            this.Add(136, 00857, "Turkish OEM");
            this.Add(001, 00437, "US MS-DOS");
            this.Add(088, 01252, "Western European ANSI");
        }

        private void Add(byte headerCode, int codePage, string codeName)
        {
            CodePageSet cpc = new CodePageSet();
            cpc.headerCode = headerCode;
            cpc.codePage = codePage;
            try
            {
                cpc.codeName = codeName + " ";
                Encoding enc = System.Text.Encoding.GetEncoding(cpc.codePage);
                if ((enc.EncodingName.ToUpper().IndexOf("DOS") >= 0) && (enc.EncodingName.ToUpper().IndexOf("WINDOWS") < 0) && (enc.EncodingName.ToUpper().IndexOf("OEM") < 0))
                    cpc.codeName += @"\ DOS-" + cpc.codePage.ToString() + @" \ " + enc.EncodingName;
                else if ((enc.EncodingName.ToUpper().IndexOf("DOS") < 0) && (enc.EncodingName.ToUpper().IndexOf("WINDOWS") >= 0) && (enc.EncodingName.ToUpper().IndexOf("OEM") < 0))
                    cpc.codeName += @"\ Windows-" + cpc.codePage.ToString() + @" \ " + enc.EncodingName;
                else if ((enc.EncodingName.ToUpper().IndexOf("DOS") < 0) && (enc.EncodingName.ToUpper().IndexOf("WINDOWS") < 0) && (enc.EncodingName.ToUpper().IndexOf("OEM") >= 0))
                    cpc.codeName += @"\ OEM-" + cpc.codePage.ToString() + @" \ " + enc.EncodingName;
                else
                    cpc.codeName += @" \ " + enc.EncodingName;                
            }
            catch 
            {
                cpc.codeName = codeName + @" \ --**--UNKNOWN--**-- ";                
            };
            cpc.codeName += String.Format(@" -- 0x{0:X2}", cpc.headerCode);
            this.Add(cpc);
        }

        public CodePageSet this[byte headerCode]
        {
            get
            {
                if (this.Count == 0) return new CodePageSet();
                foreach (CodePageSet cpc in this)
                    if (cpc.headerCode == headerCode)
                        return cpc;
                return new CodePageSet();
            }
        }
    }

}
