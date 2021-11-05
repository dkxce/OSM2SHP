using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using ProtoBuf;
using Ionic.Zlib;

namespace OSM2SHP 
{
    public class OSMPBFReader : FileStream
    {
        #region classes
        [ProtoContract]
        public class BlobHeader
        {
            [ProtoMember(1, IsRequired = true)]
            public string type;
            [ProtoMember(2, IsRequired = false)]
            public byte[] indexdata;
            [ProtoMember(3, IsRequired = true)]
            public int datasize;
        }

        [ProtoContract]
        public class Blob
        {
            [ProtoMember(1, IsRequired = false)]
            public byte[] raw;
            [ProtoMember(2, IsRequired = false)]
            public int raw_size;
            [ProtoMember(3, IsRequired = false)]
            public byte[] zlib_data;

        }

        [ProtoContract]
        public class HeaderBBox
        {
            [ProtoMember(1)]
            public long left;
            [ProtoMember(2)]
            public long right;
            [ProtoMember(3)]
            public long top;
            [ProtoMember(4)]
            public long bottom;
        }

        [ProtoContract]
        public class HeaderBlock
        {
            [ProtoMember(1, IsRequired = false)]
            public HeaderBBox bbox;
            [ProtoMember(4)]
            public List<string> required_features;
            [ProtoMember(5)]
            public List<string> optional_features;
            [ProtoMember(16, IsRequired = false)]
            public string optional_string;
            [ProtoMember(17, IsRequired = false)]
            public string source;
            [ProtoMember(32, IsRequired = false)]
            public long osmosis_replication_timestamp;
            [ProtoMember(33, IsRequired = false)]
            public long osmosis_replication_sequence_number;

            public double[] GetBBox
            {
                get
                {
                    double[] result = new double[] { -180, 180, -90, 90 };
                    if (bbox == null) return result;
                    result[0] = .000000001 * ((bbox.left % 2) == 0 ? bbox.left / 2 : (bbox.left + 1) / 2);
                    result[1] = .000000001 * ((bbox.right % 2) == 0 ? bbox.right / 2 : (bbox.right + 1) / 2);
                    result[2] = .000000001 * ((bbox.bottom % 2) == 0 ? bbox.bottom / 2 : (bbox.bottom + 1) / 2);
                    result[3] = .000000001 * ((bbox.top % 2) == 0 ? bbox.top / 2 : (bbox.top + 1) / 2);
                    return result;
                }
            }
        }

        [ProtoContract]
        public class PrimitiveBlock
        {
            [ProtoMember(1)]
            public StringTable stringtable;
            [ProtoMember(2)]
            public List<PrimitiveGroup> primitivegroup;
            [ProtoMember(17, IsRequired = false)]
            public int granularity = 100;
            [ProtoMember(19, IsRequired = false)]
            public long lat_offset = 0;
            [ProtoMember(20, IsRequired = false)]
            public long lon_offset = 0;
            [ProtoMember(18, IsRequired = false)]
            public int date_granularity = 1000;
        }

        [ProtoContract]
        public class StringTable
        {
            [ProtoMember(1)]
            public List<string> strings;
        }

        [ProtoContract]
        public class PrimitiveGroup
        {
            [ProtoMember(1)]
            public List<Node> nodes;
            [ProtoMember(2, IsRequired = false)]
            public DenseNodes dense;
            [ProtoMember(3)]
            public List<Way> ways;
            [ProtoMember(4)]
            public List<Relation> relations;
            [ProtoMember(5)]
            public List<ChangeSet> changesets;
        }

        [ProtoContract]
        public class Node
        {
            [ProtoMember(1)]
            public long id;
            [ProtoMember(2, IsPacked = true)]
            public List<int> keys;
            [ProtoMember(3, IsPacked = true)]
            public List<int> vals;
            [ProtoMember(4, IsRequired = false)]
            public Info info;
            [ProtoMember(8)]
            public long lat;
            [ProtoMember(9)]
            public long lon;

            public double GetLat(PrimitiveBlock pb)
            {
                return .000000001 * (pb.lat_offset + (pb.granularity * (((lat % 2) == 0) ? lat / 2 : (lat + 1) / 2)));
            }

            public double GetLon(PrimitiveBlock pb)
            {
                return .000000001 * (pb.lon_offset + (pb.granularity * (((lon % 2) == 0) ? lon / 2 : (lon + 1) / 2)));
            }

            public Dictionary<string, string> GetTags(PrimitiveBlock pb)
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                if ((keys != null) && (vals != null) && (keys.Count > 0) && (keys.Count == vals.Count))
                    for (int i = 0; i < keys.Count; i++)
                        result.Add(pb.stringtable.strings[keys[i]], pb.stringtable.strings[vals[i]]);
                return result;
            }

            public NodeInfo GetNode(PrimitiveBlock pb)
            {
                NodeInfo ni = new NodeInfo();
                ni.id = this.id;
                ni.lat = GetLat(pb);
                ni.lon = GetLon(pb);
                ni.tags = GetTags(pb);
                return ni;
            }
        }

        [ProtoContract]
        public class DenseNodes
        {
            [ProtoMember(1, IsPacked = true)]
            public List<long> id;  // DELTA coded
            [ProtoMember(5, IsRequired = false)]
            public DenseInfo denseinfo;
            [ProtoMember(8)]
            public List<long> lat; // DELTA coded
            [ProtoMember(9, IsPacked = true)]
            public List<long> lon; // DELTA coded
            [ProtoMember(10, IsPacked = true)]
            public List<int> keys_vals;

            public int Count
            {
                get
                {
                    if (id == null) return 0;
                    return id.Count;
                }
            }

            private int __index = -1;
            private long[] __next = new long[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            protected internal PrimitiveBlock __pb = null;
            public PrimitiveBlock PrimBlock { get { return __pb; } set { __pb = value; } }
            public KeyValuePair<int, NodeInfo> Next
            {
                get
                {
                    if (__pb == null) throw new Exception("PrimitiveBlock is not set!");
                    if (__index >= (Count - 1)) return new KeyValuePair<int, NodeInfo>(-1, null);
                    __index++;

                    NodeInfo ni = new NodeInfo();

                    // ID
                    if ((this.id[__index] % 2) == 0)
                        __next[0] += this.id[__index] / 2;
                    else
                        __next[0] -= (this.id[__index] + 1) / 2;

                    // LAT
                    if ((this.lat[__index] % 2) == 0)
                        __next[1] += this.lat[__index] / 2;
                    else
                        __next[1] -= (this.lat[__index] + 1) / 2;

                    // LON
                    if ((this.lon[__index] % 2) == 0)
                        __next[2] += this.lon[__index] / 2;
                    else
                        __next[2] -= (this.lon[__index] + 1) / 2;

                    if (this.denseinfo != null) // NO DENSE INFO
                    {
                        // Timestamp
                        if ((this.denseinfo.timestamp[__index] % 2) == 0)
                            __next[3] += this.denseinfo.timestamp[__index] / 2;
                        else
                            __next[3] -= (this.denseinfo.timestamp[__index] + 1) / 2;

                        //ChangeSet
                        if ((this.denseinfo.changeset[__index] % 2) == 0)
                            __next[4] += this.denseinfo.changeset[__index] / 2;
                        else
                            __next[4] -= (this.denseinfo.changeset[__index] + 1) / 2;

                        //UID
                        if ((this.denseinfo.uid[__index] % 2) == 0)
                            __next[5] += this.denseinfo.uid[__index] / 2;
                        else
                            __next[5] -= (this.denseinfo.uid[__index] + 1) / 2;

                        //USID
                        if ((this.denseinfo.user_sid[__index] % 2) == 0)
                            __next[6] += this.denseinfo.user_sid[__index] / 2;
                        else
                            __next[6] -= (this.denseinfo.user_sid[__index] + 1) / 2;
                    };

                    // TAGS
                    for (int i = (int)__next[7]; (i < this.keys_vals.Count) && (__index == __next[8]); i++)
                    {
                        if (this.keys_vals[i] == 0)
                        {
                            __next[7] = i + 1;
                            __next[8]++;
                            break;
                        }
                        else
                        {
                            int k = this.keys_vals[i++];
                            int v = this.keys_vals[i];
                            ni.tags.Add(__pb.stringtable.strings[k], __pb.stringtable.strings[v]);
                        };
                    };

                    ni.id = __next[0];
                    ni.lat = (double).000000001 * ((double)__pb.lat_offset + ((double)__pb.granularity * (double)__next[1]));
                    ni.lon = (double).000000001 * ((double)__pb.lon_offset + ((double)__pb.granularity * (double)__next[2]));
                    if (this.denseinfo != null) // NO DENSE INFO
                    {
                        ni.timestamp = __next[3];
                        ni.changeset = __next[4];
                        ni.uid = (int)__next[5];
                        ni.user = __pb.stringtable.strings[(int)__next[6]];
                        ni.version = this.denseinfo.version[__index];
                    };

                    return new KeyValuePair<int, NodeInfo>(__index, ni);
                }
            }

            public long GetID(int index)
            {
                if (Count == 0) return -1;

                long result = 0;
                for (int i = 0; i < Count; i++)
                {
                    if ((this.id[i] % 2) == 0)
                        result += this.id[i] / 2;
                    else
                        result -= (this.id[i] + 1) / 2;
                    if (i == index) return result;
                };

                return -1;
            }

            public int GetIndex(long id)
            {
                if (Count == 0) return -1;

                long result = 0;
                for (int i = 0; i < Count; i++)
                {
                    if ((this.id[i] % 2) == 0)
                        result += this.id[i] / 2;
                    else
                        result -= (this.id[i] + 1) / 2;
                    if (result == id) return i;
                };

                return -1;
            }

            public double GetLat(int index)
            {
                if (__pb == null) throw new Exception("PrimitiveBlock is not set!");
                long lat = 0;
                for (int i = 0; i < Count; i++)
                {
                    if ((this.lat[i] % 2) == 0)
                        lat += this.lat[i] / 2;
                    else
                        lat -= (this.lat[i] + 1) / 2;
                    if (i == index)
                        return (double).000000001 * ((double)__pb.lat_offset + ((double)__pb.granularity * (double)lat));
                };
                return 0;
            }

            public double GetLat(long id)
            {
                return GetLat(GetIndex(id));
            }

            public double GetLon(int index)
            {
                if (__pb == null) throw new Exception("PrimitiveBlock is not set!");
                long lon = 0;
                for (int i = 0; i < Count; i++)
                {
                    if ((this.lon[i] % 2) == 0)
                        lon += this.lon[i] / 2;
                    else
                        lon -= (this.lon[i] + 1) / 2;
                    if (i == index)
                        return (double).000000001 * ((double)__pb.lon_offset + ((double)__pb.granularity * (double)lon));
                };
                return 0;
            }

            public double GetLon(long id)
            {
                return GetLon(GetIndex(id));
            }

            public long GetTimeStamp(int index)
            {
                long res = 0;
                for (int i = 0; i < Count; i++)
                {
                    if ((this.denseinfo.timestamp[i] % 2) == 0)
                        res += this.denseinfo.timestamp[i] / 2;
                    else
                        res -= (this.denseinfo.timestamp[i] + 1) / 2;
                    if (i == index) return res;
                };
                return 0;
            }

            public long GetTimeStamp(long id)
            {
                return GetTimeStamp(GetIndex(id));
            }

            public long GetChangeSet(int index)
            {
                long res = 0;
                for (int i = 0; i < Count; i++)
                {
                    if ((this.denseinfo.changeset[i] % 2) == 0)
                        res += this.denseinfo.changeset[i] / 2;
                    else
                        res -= (this.denseinfo.changeset[i] + 1) / 2;
                    if (i == index) return res;
                };
                return 0;
            }

            public long GetChangeSet(long id)
            {
                return GetChangeSet(GetIndex(id));
            }

            public int GetUID(int index)
            {
                int res = 0;
                for (int i = 0; i < Count; i++)
                {
                    if ((this.denseinfo.uid[i] % 2) == 0)
                        res += this.denseinfo.uid[i] / 2;
                    else
                        res -= (this.denseinfo.uid[i] + 1) / 2;
                    if (i == index) return res;
                };
                return 0;
            }

            public int GetUID(long id)
            {
                return GetUID(GetIndex(id));
            }

            public long GetUSID(int index)
            {
                long res = 0;
                for (int i = 0; i < Count; i++)
                {
                    if ((this.denseinfo.user_sid[i] % 2) == 0)
                        res += this.denseinfo.user_sid[i] / 2;
                    else
                        res -= (this.denseinfo.user_sid[i] + 1) / 2;
                    if (i == index) return res;
                };
                return 0;
            }

            public long GetUSID(long id)
            {
                return GetUSID(GetIndex(id));
            }

            public Dictionary<string, string> GetTags(int index)
            {
                if (__pb == null) throw new Exception("PrimitiveBlock is not set!");
                Dictionary<string, string> result = new Dictionary<string, string>();
                int curr_id = 0;
                for (int i = 0; (i < this.keys_vals.Count) && (index >= curr_id); i++)
                {
                    if (this.keys_vals[i] == 0)
                        curr_id++;
                    else if (index == curr_id)
                    {
                        int k = this.keys_vals[i++];
                        int v = this.keys_vals[i];
                        if (!result.ContainsKey(__pb.stringtable.strings[k]))
                            result.Add(__pb.stringtable.strings[k], __pb.stringtable.strings[v]);
                        else
                            result[__pb.stringtable.strings[k]] += "\r\n" + __pb.stringtable.strings[v];
                    };
                };
                return result;
            }

            public Dictionary<string, string> GetTags(long id)
            {
                return GetTags(GetIndex(id));
            }

            public NodeInfo GetNode(int index)
            {
                if (__pb == null) throw new Exception("PrimitiveBlock is not set!");

                NodeInfo ni = new NodeInfo();

                long id = 0, lat = 0, lon = 0, ts = 0, cs = 0, usid = 0;
                int uid = 0;
                for (int i = 0; i < Count; i++)
                {
                    // ID
                    if ((this.id[i] % 2) == 0)
                        id += this.id[i] / 2;
                    else
                        id -= (this.id[i] + 1) / 2;

                    // LAT
                    if ((this.lat[i] % 2) == 0)
                        lat += this.lat[i] / 2;
                    else
                        lat -= (this.lat[i] + 1) / 2;

                    // LON
                    if ((this.lon[i] % 2) == 0)
                        lon += this.lon[i] / 2;
                    else
                        lon -= (this.lon[i] + 1) / 2;

                    if (this.denseinfo != null) // NO DENSE INFO
                    {
                        // Timestamp
                        if ((this.denseinfo.timestamp[i] % 2) == 0)
                            ts += this.denseinfo.timestamp[i] / 2;
                        else
                            ts -= (this.denseinfo.timestamp[i] + 1) / 2;

                        //ChangeSet
                        if ((this.denseinfo.changeset[i] % 2) == 0)
                            cs += this.denseinfo.changeset[i] / 2;
                        else
                            cs -= (this.denseinfo.changeset[i] + 1) / 2;

                        //UID
                        if ((this.denseinfo.uid[i] % 2) == 0)
                            uid += this.denseinfo.uid[i] / 2;
                        else
                            uid -= (this.denseinfo.uid[i] + 1) / 2;

                        //USID
                        if ((this.denseinfo.user_sid[i] % 2) == 0)
                            usid += this.denseinfo.user_sid[i] / 2;
                        else
                            usid -= (this.denseinfo.user_sid[i] + 1) / 2;
                    };

                    if (i == index) break;
                };

                // TAGS
                int curr_id = 0;
                for (int i = 0; (i < this.keys_vals.Count) && (index >= curr_id); i++)
                {
                    if (this.keys_vals[i] == 0)
                        curr_id++;
                    else if (index == curr_id)
                    {
                        int k = this.keys_vals[i++];
                        int v = this.keys_vals[i];
                        if (!ni.tags.ContainsKey(__pb.stringtable.strings[k]))
                            ni.tags.Add(__pb.stringtable.strings[k], __pb.stringtable.strings[v]);
                        else
                            ni.tags[__pb.stringtable.strings[k]] += "\r\n" + __pb.stringtable.strings[v];
                    };
                };

                ni.id = id;
                ni.lat = (double).000000001 * ((double)__pb.lat_offset + ((double)__pb.granularity * (double)lat));
                ni.lon = (double).000000001 * ((double)__pb.lon_offset + ((double)__pb.granularity * (double)lon));
                if (this.denseinfo != null) // NO DENSE INFO
                {
                    ni.timestamp = ts;
                    ni.changeset = cs;
                    ni.uid = uid;
                    ni.user = __pb.stringtable.strings[(int)usid];
                    ni.version = this.denseinfo.version[index];
                };

                return ni;
            }

            public NodeInfo GetNode(long id)
            {
                return GetNode(GetIndex(id));
            }

        }

        /// <summary>
        ///   Tags Utils
        /// </summary>
        public abstract class TagsInfo
        {
            public int infoType { get { return GetInfoType(); } }
            public abstract int GetInfoType();

            public int PointsCount { get { return GetPointsCount(); } }
            public abstract int GetPointsCount();
            public abstract PointF GetPoint(int index);
            
            public Dictionary<string, string> tags = new Dictionary<string, string>();

            public bool HasTags
            {
                get
                {
                    return tags.Count > 0;
                }
            }

            public int TagsCount
            {
                get
                {
                    return tags.Count;
                }
            }

            public string TagsAll
            {
                get
                {
                    if (tags.Count == 0) return null;
                    string result = "";
                    foreach (KeyValuePair<string, string> tag in tags)
                        result += tag.Key + "=" + tag.Value + "\r\n";
                    return result;
                }
            }

            public bool HasTag(string k)
            {
                if (tags.Count == 0) return false;
                foreach (KeyValuePair<string, string> kvp in tags)
                    if (kvp.Key == k)
                        return true;
                return false;
            }

            public string GetTag(string k)
            {
                if (tags.Count == 0) return null;
                foreach (KeyValuePair<string, string> kvp in tags)
                    if (kvp.Key == k)
                        return kvp.Value;
                return null;
            }

            public bool HasName
            {
                get
                {
                    if (tags.Count == 0) return false;
                    foreach (KeyValuePair<string, string> kvp in tags)
                        if (kvp.Key == "name")
                            return true;
                    return false;
                }
            }

            public string Name
            {
                get
                {
                    if (tags.Count == 0) return null;
                    foreach (KeyValuePair<string, string> kvp in tags)
                        if (kvp.Key == "name")
                            return kvp.Value;
                    return null;
                }
            }

            public int AddrTagsCount
            {
                get
                {
                    if (tags.Count == 0) return 0;
                    int res = 0;
                    foreach (KeyValuePair<string, string> kvp in this.tags)
                        if (kvp.Key.IndexOf("addr") == 0)
                            res++;
                    return res;
                }
            }

            public string Addr
            {
                get
                {
                    if (tags.Count == 0) return null;
                    string addr = "";
                    if (tags.ContainsKey("addr:country")) addr += tags["addr:country"];
                    if (addr.Length > 0) addr += ", ";
                    if (tags.ContainsKey("addr:postcode")) addr += tags["addr:postcode"];
                    if (addr.Length > 0) addr += ", ";
                    if (tags.ContainsKey("addr:district")) addr += tags["addr:district"];
                    if (addr.Length > 0) addr += ", ";
                    if (tags.ContainsKey("addr:region")) addr += tags["addr:region"];
                    if (addr.Length > 0) addr += ", ";
                    if (tags.ContainsKey("addr:city")) addr += tags["addr:city"];
                    if (addr.Length > 0) addr += ", ";
                    if (tags.ContainsKey("addr:street")) addr += tags["addr:street"];
                    if (addr.Length > 0) addr += ", ";
                    if (tags.ContainsKey("addr:housenumber")) addr += tags["addr:housenumber"];
                    return addr;
                }
            }

            public string this[string tag]
            {
                get
                {
                    if (this.tags == null) return null;
                    if (this.tags.Count == 0) return null;
                    if (!this.tags.ContainsKey(tag)) return null;
                    return this.tags[tag];
                }
            }
        }

        public class NodeInfo : TagsInfo
        {
            public long id = 0;
            public double lat = 0;
            public double lon = 0;
            public int version = 0;
            public long timestamp = 0;
            public long changeset = 0;
            public int uid = 0;
            public string user = "";
            public string icon = "noicon";

            public override int GetInfoType() { return 1; }
            public override int GetPointsCount() { return 1; }
            public override PointF GetPoint(int index) { return point; }
                                                          
            public PointF point
            {
                get
                {
                    return new PointF((float)lon, (float)lat);
                }
            }
            public DateTime datetime
            {
                get
                {
                    System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    return dtDateTime.AddSeconds(timestamp);
                }
                set
                {
                    timestamp = (long)(value - new DateTime(1970, 1, 1)).TotalSeconds;
                }
            }            
        }

        [ProtoContract]
        public class Way : TagsInfo
        {
            [ProtoMember(1)]
            public long id;
            [ProtoMember(2, IsPacked = true)]
            public List<int> keys;
            [ProtoMember(3, IsPacked = true)]
            public List<int> vals;
            [ProtoMember(4, IsRequired = false)]
            public Info info;
            [ProtoMember(8, IsPacked = true)]
            public List<long> refs; // DELTA coded

            [ProtoIgnore]
            public NodesXYIndex.IdLatLon[] _points = new NodesXYIndex.IdLatLon[0];

            public override int GetInfoType() { return 2; }
            public override int GetPointsCount() { return _points.Length; }
            public override PointF GetPoint(int index) { return new PointF((float)_points[index].lon, (float)_points[index].lat); }

            public Dictionary<string, string> GetTags(PrimitiveBlock pb)
            {
                if (pb == null) throw new Exception("PrimitiveBlock is not set!");
                Dictionary<string, string> result = new Dictionary<string, string>();
                if (this.keys == null) return result;
                if (this.vals == null) return result;
                if (this.keys.Count == 0) return result;
                if (this.vals.Count == 0) return result;
                if (this.keys.Count != this.vals.Count) return result;
                for (int i = 0; i < this.keys.Count; i++)
                {
                    int k = this.keys[i];
                    int v = this.vals[i];
                    if (!result.ContainsKey(pb.stringtable.strings[k]))
                        result.Add(pb.stringtable.strings[k], pb.stringtable.strings[v]);
                    else
                        result[pb.stringtable.strings[k]] += "\r\n" + pb.stringtable.strings[v];
                };
                return result;
            }

            private long[] _nd = null;
            public long[] NodeDefs
            {
                get
                {
                    if ((_nd == null) && (refs != null) && (refs.Count > 0))
                    {
                        long[] res = new long[this.refs.Count];
                        for (int __index = 0; __index < res.Length; __index++)
                        {
                            if ((refs[__index] % 2) == 0)
                                res[__index] = (__index > 0 ? res[__index - 1] : 0) + refs[__index] / 2;
                            else
                                res[__index] = (__index > 0 ? res[__index - 1] : 0) - (refs[__index] + 1) / 2;
                        };
                        _nd = res;
                    };
                    return _nd;
                }
                set
                {
                    _nd = value;
                }
            }
        }

        [ProtoContract]
        public class Relation : TagsInfo
        {
            public enum MemberType
            {
                NODE = 0,
                WAY = 1,
                RELATION = 2
            }

            [ProtoMember(1)]
            public long id;
            [ProtoMember(2, IsPacked = true)]
            public List<int> keys;
            [ProtoMember(3, IsPacked = true)]
            public List<int> vals;
            [ProtoMember(4, IsRequired = false)]
            public Info info;
            [ProtoMember(8, IsPacked = true)]
            public List<int> roles_sid;
            [ProtoMember(9, IsPacked = true)]
            public List<long> memids; // DELTA coded
            [ProtoMember(10, IsPacked = true)]
            public List<MemberType> types;

            [ProtoIgnore]
            public NodesXYIndex.IdLatLon[] _points = new NodesXYIndex.IdLatLon[0];

            public override int GetInfoType() { return 3; }
            public override int GetPointsCount() { return _points.Length; }
            public override PointF GetPoint(int index) { return new PointF((float)_points[index].lon, (float)_points[index].lat); }

            public Dictionary<string, string> GetTags(PrimitiveBlock pb)
            {
                if (pb == null) throw new Exception("PrimitiveBlock is not set!");
                Dictionary<string, string> result = new Dictionary<string, string>();
                if (this.keys == null) return result;
                if (this.vals == null) return result;
                if (this.keys.Count == 0) return result;
                if (this.vals.Count == 0) return result;
                if (this.keys.Count != this.vals.Count) return result;
                for (int i = 0; i < this.keys.Count; i++)
                {
                    int k = this.keys[i];
                    int v = this.vals[i];
                    if (!result.ContainsKey(pb.stringtable.strings[k]))
                        result.Add(pb.stringtable.strings[k], pb.stringtable.strings[v]);
                    else
                        result[pb.stringtable.strings[k]] += "\r\n" + pb.stringtable.strings[v];
                };
                return result;
            }

            public List<string> roles = new List<string>();

            public List<string> GetRoles(PrimitiveBlock pb)
            {
                if (pb == null) throw new Exception("PrimitiveBlock is not set!");
                List<string> result = new List<string>();
                if (this.roles_sid == null) return result;
                if (this.roles_sid.Count == 0) return result;
                for (int i = 0; i < this.roles_sid.Count; i++)
                    result.Add(pb.stringtable.strings[this.roles_sid[i]]);
                return result;
            }

            private long[] _mids = null;
            public long[] Refs
            {
                get
                {
                    if ((_mids == null) && (memids != null) && (memids.Count > 0))
                    {
                        long[] res = new long[this.memids.Count];
                        for (int __index = 0; __index < res.Length; __index++)
                        {
                            if ((memids[__index] % 2) == 0)
                                res[__index] = (__index > 0 ? res[__index - 1] : 0) + memids[__index] / 2;
                            else
                                res[__index] = (__index > 0 ? res[__index - 1] : 0) - (memids[__index] + 1) / 2;
                        };
                        _mids = res;
                    };
                    return _mids;
                }
                set
                {
                    _mids = value;
                }
            }
        }

        [ProtoContract]
        public class ChangeSet
        {
            [ProtoMember(1)]
            public long id;
        }

        [ProtoContract]
        public class Info
        {
            [ProtoMember(1, IsRequired = false)]
            public int version;
            [ProtoMember(2, IsRequired = false)]
            public long timestamp;
            [ProtoMember(3, IsRequired = false)]
            public long changeset;
            [ProtoMember(4, IsRequired = false)]
            public int uid;
            [ProtoMember(5, IsRequired = false)]
            public uint user_sid;
            public string user = "";
            [ProtoMember(6, IsRequired = false)]
            public bool visible;

            public DateTime datetime
            {
                get
                {
                    System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                    return dtDateTime.AddSeconds(timestamp);
                }
                set
                {
                    timestamp = (long)(value - new DateTime(1970, 1, 1)).TotalSeconds;
                }
            }
        }

        [ProtoContract]
        public class DenseInfo
        {
            [ProtoMember(1, IsPacked = true)]
            public List<int> version;
            [ProtoMember(2, IsPacked = true)]
            public List<long> timestamp; // DELTA coded
            [ProtoMember(3, IsPacked = true)]
            public List<long> changeset; // DELTA coded
            [ProtoMember(4, IsPacked = true)]
            public List<int> uid; // DELTA coded
            [ProtoMember(5, IsPacked = true)]
            public List<uint> user_sid; // DELTA coded
            [ProtoMember(6, IsPacked = true)]
            public List<bool> visible;
        }
        #endregion classes

        private bool _validFile = false;
        private int _blockHeaders = 0;
        private int _blockBlobs = 0;
        private int _osmHeaders = 0;
        private int _osmDatas = 0;

        private string _fileName = "";

        private HeaderBlock _headerBlock = null;
        private PrimitiveBlock _primitiveBlock = null;
        private BlobHeader _blobheader = null;
        private Blob _blob = null;

        public bool ValidHeader { get { return _validFile; } }
        public int TotalHeaders { get { return _blockHeaders; } }
        public int TotalBlobs { get { return _blockBlobs; } }
        public int OSMHeaders { get { return _osmHeaders; } }
        public int OSMDatas { get { return _osmDatas; } }

        public string FileName { get { return _fileName; } }
        public bool EndOfFile { get { return this.Position == this.Length; } }
        public bool HasOSMHeader { get { return _headerBlock != null; } }
        public bool HasOSMPrimitiveBlock { get { return _primitiveBlock != null; } }

        public BlobHeader PBFBlobHeader { get { return _blobheader; } }
        public Blob PBFBlob { get { return _blob; } }
        public HeaderBlock OSMHeaderBlock { get { return _headerBlock; } }
        public PrimitiveBlock OSMPrimitiveBlock { get { return _primitiveBlock; } }

        private static MyBitConverter bc = new MyBitConverter(false);

        public OSMPBFReader(string fileName)
            : base(fileName, FileMode.Open, FileAccess.Read)
        {
            this._fileName = fileName;
            if (this.Length < 15) return;
            this.Position = 6;
            byte[] ha = new byte[9];
            this.Read(ha, 0, ha.Length);
            if (System.Text.Encoding.ASCII.GetString(ha) == "OSMHeader")
                _validFile = true;
            this.Position = 0;
            ResetCounters();
        }

        private void ResetCounters()
        {
            _blockHeaders = 0;
            _blockBlobs = 0;
            _osmHeaders = 0;
            _osmDatas = 0;
            _headerBlock = null;
            _primitiveBlock = null;
        }

        public void ReadAll()
        {
            ResetCounters();
            while (!EndOfFile)
                ReadNext();
        }

        public void ReadNext()
        {
            if (EndOfFile) return;

            byte[] data = new byte[0];
            _primitiveBlock = null;

            while ((!EndOfFile) && (_primitiveBlock == null))
            {
                // Read BlobHeader //   
                data = new byte[4];
                this.Read(data, 0, 4);
                int blockSize = bc.ToInt32(data, 0);
                data = new byte[blockSize];
                this.Read(data, 0, data.Length);
                _blobheader = ProtoBuf.Serializer.Deserialize<BlobHeader>(new MemoryStream(data));
                _blockHeaders++;

                if (_blobheader.datasize > 0)
                {
                    // Read Blob //
                    data = new byte[_blobheader.datasize];
                    this.Read(data, 0, data.Length);
                    _blob = ProtoBuf.Serializer.Deserialize<Blob>(new MemoryStream(data));
                    _blockBlobs++;

                    byte[] uncompressed = ZlibStream.UncompressBuffer(_blob.zlib_data);

                    if (_blobheader.type == "OSMHeader")
                    {
                        _headerBlock = ProtoBuf.Serializer.Deserialize<HeaderBlock>(new MemoryStream(uncompressed));
                        _osmHeaders++;
                    };
                    if (_blobheader.type == "OSMData")
                    {
                        _primitiveBlock = ProtoBuf.Serializer.Deserialize<PrimitiveBlock>(new MemoryStream(uncompressed));
                        if ((_primitiveBlock != null) && (_primitiveBlock.primitivegroup != null) && (_primitiveBlock.primitivegroup.Count > 0))
                            for (int i = 0; i < _primitiveBlock.primitivegroup.Count; i++)
                                if (_primitiveBlock.primitivegroup[i].dense != null)
                                    _primitiveBlock.primitivegroup[i].dense.__pb = _primitiveBlock;
                        _osmDatas++;
                    };
                };
            };
        }

        /// <summary>
        /// Converts base data types to an array of bytes, and an array of bytes to base
        /// data types.
        /// All info taken from the meta data of System.BitConverter. This implementation
        /// allows for Endianness consideration.
        ///</summary>
        public class MyBitConverter
        {
            /// <summary>
            ///     Constructor
            /// </summary>
            public MyBitConverter()
            {

            }

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <param name="IsLittleEndian">Indicates the byte order ("endianess") in which data is stored in this computer architecture.</param>
            public MyBitConverter(bool IsLittleEndian)
            {
                this.isLittleEndian = IsLittleEndian;
            }

            /// <summary>
            ///     Indicates the byte order ("endianess") in which data is stored in this computer
            /// architecture.
            /// </summary>
            private bool isLittleEndian = true;

            /// <summary>
            /// Indicates the byte order ("endianess") in which data is stored in this computer
            /// architecture.
            ///</summary>
            public bool IsLittleEndian { get { return isLittleEndian; } set { isLittleEndian = value; } } // should default to false, which is what we want for Empire

            /// <summary>
            /// Converts the specified double-precision floating point number to a 64-bit
            /// signed integer.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// A 64-bit signed integer whose value is equivalent to value.
            ///</summary>
            public long DoubleToInt64Bits(double value) { throw new NotImplementedException(); }
            ///
            /// <summary>
            /// Returns the specified Boolean value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// A Boolean value.
            ///
            /// Returns:
            /// An array of bytes with length 1.
            ///</summary>
            public byte[] GetBytes(bool value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified Unicode character value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// A character to convert.
            ///
            /// Returns:
            /// An array of bytes with length 2.
            ///</summary>
            public byte[] GetBytes(char value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified double-precision floating point value as an array of
            /// bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 8.
            ///</summary>
            public byte[] GetBytes(double value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified single-precision floating point value as an array of
            /// bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 4.
            ///</summary>
            public byte[] GetBytes(float value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified 32-bit signed integer value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 4.
            ///</summary>
            public byte[] GetBytes(int value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified 64-bit signed integer value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 8.
            ///</summary>
            public byte[] GetBytes(long value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified 16-bit signed integer value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 2.
            ///</summary>
            public byte[] GetBytes(short value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified 32-bit unsigned integer value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 4.
            ///</summary>
            public byte[] GetBytes(uint value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified 64-bit unsigned integer value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 8.
            ///</summary>
            public byte[] GetBytes(ulong value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Returns the specified 16-bit unsigned integer value as an array of bytes.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// An array of bytes with length 2.
            ///</summary>
            public byte[] GetBytes(ushort value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.GetBytes(value);
                }
                else
                {
                    byte[] res = System.BitConverter.GetBytes(value);
                    Array.Reverse(res);
                    return res;
                }
            }
            ///
            /// <summary>
            /// Converts the specified 64-bit signed integer to a double-precision floating
            /// point number.
            ///
            /// Parameters:
            /// value:
            /// The number to convert.
            ///
            /// Returns:
            /// A double-precision floating point number whose value is equivalent to value.
            ///</summary>
            public double Int64BitsToDouble(long value) { throw new NotImplementedException(); }
            ///
            /// <summary>
            /// Returns a Boolean value converted from one byte at a specified position in
            /// a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// true if the byte at startIndex in value is nonzero; otherwise, false.
            ///
            /// Exceptions:
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public bool ToBoolean(byte[] value, int startIndex) { throw new NotImplementedException(); }
            ///
            /// <summary>
            /// Returns a Unicode character converted from two bytes at a specified position
            /// in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A character formed by two bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex equals the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public char ToChar(byte[] value, int startIndex) { throw new NotImplementedException(); }
            ///
            /// <summary>
            /// Returns a double-precision floating point number converted from eight bytes
            /// at a specified position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A double precision floating point number formed by eight bytes beginning
            /// at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex is greater than or equal to the length of value minus 7, and is
            /// less than or equal to the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public double ToDouble(byte[] value, int startIndex) { throw new NotImplementedException(); }
            ///
            /// <summary>
            /// Returns a 16-bit signed integer converted from two bytes at a specified position
            /// in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A 16-bit signed integer formed by two bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex equals the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public short ToInt16(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToInt16(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToInt16(res, value.Length - sizeof(Int16) - startIndex);
                }
            }
            ///
            /// <summary>
            /// Returns a 32-bit signed integer converted from four bytes at a specified
            /// position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A 32-bit signed integer formed by four bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex is greater than or equal to the length of value minus 3, and is
            /// less than or equal to the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public int ToInt32(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToInt32(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToInt32(res, value.Length - sizeof(Int32) - startIndex);
                }
            }
            ///
            /// <summary>
            /// Returns a 64-bit signed integer converted from eight bytes at a specified
            /// position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A 64-bit signed integer formed by eight bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex is greater than or equal to the length of value minus 7, and is
            /// less than or equal to the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public long ToInt64(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToInt64(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToInt64(res, value.Length - sizeof(Int64) - startIndex);
                }
            }
            ///
            /// <summary>
            /// Returns a single-precision floating point number converted from four bytes
            /// at a specified position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A single-precision floating point number formed by four bytes beginning at
            /// startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex is greater than or equal to the length of value minus 3, and is
            /// less than or equal to the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public float ToSingle(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToSingle(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToSingle(res, value.Length - sizeof(Single) - startIndex);
                }
            }
            ///
            /// <summary>
            /// Converts the numeric value of each element of a specified array of bytes
            /// to its equivalent hexadecimal string representation.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// Returns:
            /// A System.String of hexadecimal pairs separated by hyphens, where each pair
            /// represents the corresponding element in value; for example, "7F-2C-4A".
            ///
            /// Exceptions:
            /// System.ArgumentNullException:
            /// value is null.
            ///</summary>
            public string ToString(byte[] value)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToString(value);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToString(res);
                }
            }
            ///
            /// <summary>
            /// Converts the numeric value of each element of a specified subarray of bytes
            /// to its equivalent hexadecimal string representation.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A System.String of hexadecimal pairs separated by hyphens, where each pair
            /// represents the corresponding element in a subarray of value; for example,
            /// "7F-2C-4A".
            ///
            /// Exceptions:
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public string ToString(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToString(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res, startIndex, value.Length - startIndex);
                    return System.BitConverter.ToString(res, startIndex);
                }
            }
            ///
            /// <summary>
            /// Converts the numeric value of each element of a specified subarray of bytes
            /// to its equivalent hexadecimal string representation.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// length:
            /// The number of array elements in value to convert.
            ///
            /// Returns:
            /// A System.String of hexadecimal pairs separated by hyphens, where each pair
            /// represents the corresponding element in a subarray of value; for example,
            /// "7F-2C-4A".
            ///
            /// Exceptions:
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex or length is less than zero. -or- startIndex is greater than
            /// zero and is greater than or equal to the length of value.
            ///
            /// System.ArgumentException:
            /// The combination of startIndex and length does not specify a position within
            /// value; that is, the startIndex parameter is greater than the length of value
            /// minus the length parameter.
            ///</summary>
            public string ToString(byte[] value, int startIndex, int length)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToString(value, startIndex, length);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res, startIndex, length);
                    return System.BitConverter.ToString(res, startIndex, length);
                }
            }
            ///
            /// <summary>
            /// Returns a 16-bit unsigned integer converted from two bytes at a specified
            /// position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// The array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A 16-bit unsigned integer formed by two bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex equals the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public ushort ToUInt16(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToUInt16(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToUInt16(res, value.Length - sizeof(UInt16) - startIndex);
                }
            }
            ///
            /// <summary>
            /// Returns a 32-bit unsigned integer converted from four bytes at a specified
            /// position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A 32-bit unsigned integer formed by four bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex is greater than or equal to the length of value minus 3, and is
            /// less than or equal to the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public uint ToUInt32(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToUInt32(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToUInt32(res, value.Length - sizeof(UInt32) - startIndex);
                }
            }
            ///
            /// <summary>
            /// Returns a 64-bit unsigned integer converted from eight bytes at a specified
            /// position in a byte array.
            ///
            /// Parameters:
            /// value:
            /// An array of bytes.
            ///
            /// startIndex:
            /// The starting position within value.
            ///
            /// Returns:
            /// A 64-bit unsigned integer formed by the eight bytes beginning at startIndex.
            ///
            /// Exceptions:
            /// System.ArgumentException:
            /// startIndex is greater than or equal to the length of value minus 7, and is
            /// less than or equal to the length of value minus 1.
            ///
            /// System.ArgumentNullException:
            /// value is null.
            ///
            /// System.ArgumentOutOfRangeException:
            /// startIndex is less than zero or greater than the length of value minus 1.
            ///</summary>
            public ulong ToUInt64(byte[] value, int startIndex)
            {
                if (IsLittleEndian)
                {
                    return System.BitConverter.ToUInt64(value, startIndex);
                }
                else
                {
                    byte[] res = (byte[])value.Clone();
                    Array.Reverse(res);
                    return System.BitConverter.ToUInt64(res, value.Length - sizeof(UInt64) - startIndex);
                }
            }
        }
    }

    public class OSMXMLReader : FileStream
    {
        private bool _validFile = false;
        private string _fileName = "";
        private XmlReader _xmlr = null;
        private XmlDocument _xmlDoc = new XmlDocument();
        private double[] _bounds = new double[4];

        private OSMPBFReader.NodeInfo _ni = null;
        private OSMPBFReader.Way _way = null;
        private OSMPBFReader.Relation _rel = null;
        public OSMPBFReader.NodeInfo NodeInfo { get { return _ni; } }
        public OSMPBFReader.Way Way { get { return _way; } }
        public OSMPBFReader.Relation Relation { get { return _rel; } }

        public bool ValidHeader { get { return _validFile; } }
        public string FileName { get { return _fileName; } }
        public bool EndOfFile { get { return this.Position == this.Length; } }
        public double bounds_minlat { get { return _bounds[0]; } }
        public double bounds_minlon { get { return _bounds[1]; } }
        public double bounds_maxlat { get { return _bounds[2]; } }
        public double bounds_maxlon { get { return _bounds[3]; } }
        
        public OSMXMLReader(string fileName)
            : base(fileName, FileMode.Open, FileAccess.Read)
        {
            _xmlr = XmlReader.Create(this);
            try { _xmlr.Read(); }
            catch { return; };
            if (_xmlr.NodeType != XmlNodeType.XmlDeclaration) return;
            do { _xmlr.Read(); } while(_xmlr.NodeType == XmlNodeType.Whitespace);
            if (_xmlr.Name == "osm") _validFile = true;
            do { _xmlr.Read(); } while (_xmlr.NodeType == XmlNodeType.Whitespace);
            if (_xmlr.Name == "bounds")
            {
                _bounds[0] = Double.Parse(_xmlr["minlat"], System.Globalization.CultureInfo.InvariantCulture);
                _bounds[1] = Double.Parse(_xmlr["minlon"], System.Globalization.CultureInfo.InvariantCulture);
                _bounds[2] = Double.Parse(_xmlr["maxlat"], System.Globalization.CultureInfo.InvariantCulture);
                _bounds[3] = Double.Parse(_xmlr["maxlon"], System.Globalization.CultureInfo.InvariantCulture);
                _xmlr.Read();
            };            
        }

        public void ReadNext()
        {
            if (EndOfFile) return;
            
            XmlNode xn = null;
            _ni = null;
            _way = null;
            _rel = null;

            while((!EndOfFile) && ((xn == null) || (xn.NodeType == XmlNodeType.Whitespace)))
                xn = _xmlDoc.ReadNode(_xmlr);

            if (xn == null) return;
            if (xn.NodeType == XmlNodeType.Whitespace) return;

            if (xn.Name == "node")
            {
                _ni = new OSMPBFReader.NodeInfo();
                _ni.id = long.Parse(xn.Attributes["id"].Value);
                _ni.version = int.Parse(xn.Attributes["version"].Value);
                _ni.changeset = long.Parse(xn.Attributes["changeset"].Value);
                try { _ni.datetime = DateTime.Parse(xn.Attributes["timestamp"].Value); } catch { };
                _ni.uid = int.Parse(xn.Attributes["uid"].Value);
                _ni.user = xn.Attributes["user"].Value;
                _ni.lat = Double.Parse(xn.Attributes["lat"].Value, System.Globalization.CultureInfo.InvariantCulture);
                _ni.lon = Double.Parse(xn.Attributes["lon"].Value, System.Globalization.CultureInfo.InvariantCulture);
                XmlNodeList nl = xn.SelectNodes("tag");
                foreach (XmlNode tn in nl)
                    try { _ni.tags.Add(tn.Attributes["k"].Value, tn.Attributes["v"].Value); } catch { };
            };
            if (xn.Name == "way")
            {
                _way = new OSMPBFReader.Way();
                _way.id = long.Parse(xn.Attributes["id"].Value);
                _way.info = new OSMPBFReader.Info();
                _way.info.version = int.Parse(xn.Attributes["version"].Value);
                _way.info.visible = bool.Parse(xn.Attributes["visible"].Value);
                _way.info.changeset = long.Parse(xn.Attributes["changeset"].Value);
                try { _way.info.datetime = DateTime.Parse(xn.Attributes["timestamp"].Value); }
                catch { };
                _way.info.uid = int.Parse(xn.Attributes["uid"].Value);
                _way.info.user = xn.Attributes["user"].Value;
                XmlNodeList nl = xn.SelectNodes("tag");
                foreach (XmlNode tn in nl)
                    try { _way.tags.Add(tn.Attributes["k"].Value, tn.Attributes["v"].Value); }
                    catch { };
                nl = xn.SelectNodes("nd");
                if (nl.Count > 0)
                {
                    long[] refs = new long[nl.Count];
                    for (int i = 0; i < nl.Count; i++)
                        refs[i] = long.Parse(nl[i].Attributes["ref"].Value);
                    _way.NodeDefs = refs;
                };
            };
            if (xn.Name == "relation")
            {
                _rel = new OSMPBFReader.Relation();
                _rel.id = long.Parse(xn.Attributes["id"].Value);
                _rel.info = new OSMPBFReader.Info();
                _rel.info.version = int.Parse(xn.Attributes["version"].Value);
                _rel.info.visible = bool.Parse(xn.Attributes["visible"].Value);
                _rel.info.changeset = long.Parse(xn.Attributes["changeset"].Value);
                try { _rel.info.datetime = DateTime.Parse(xn.Attributes["timestamp"].Value); }
                catch { };
                _rel.info.uid = int.Parse(xn.Attributes["uid"].Value);
                _rel.info.user = xn.Attributes["user"].Value;
                XmlNodeList nl = xn.SelectNodes("tag");
                foreach (XmlNode tn in nl)
                    try { _rel.tags.Add(tn.Attributes["k"].Value, tn.Attributes["v"].Value); }
                    catch { };
                nl = xn.SelectNodes("member");
                if (nl.Count > 0)
                {
                    _rel.Refs = new long[nl.Count];
                    _rel.types = new List<OSMPBFReader.Relation.MemberType>(nl.Count);
                    _rel.roles = new List<string>(nl.Count);
                    for (int i = 0; i < nl.Count; i++)
                    {
                        _rel.Refs[i] = long.Parse(nl[i].Attributes["ref"].Value);
                        OSMPBFReader.Relation.MemberType mt = OSMPBFReader.Relation.MemberType.RELATION;
                        string at_type = nl[i].Attributes["type"].Value;
                        if(at_type == "way") mt = OSMPBFReader.Relation.MemberType.WAY;
                        if(at_type == "node") mt = OSMPBFReader.Relation.MemberType.NODE;
                        _rel.types.Add(mt);
                        _rel.roles.Add(nl[i].Attributes["role"].Value);
                    };                    
                };
            };
        }
    }
}
