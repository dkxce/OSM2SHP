using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace OSM2SHP
{
    public class SHPWriter: FileStream
    {
        private OSMPBFReader.MyBitConverter bcle = new OSMPBFReader.MyBitConverter(true);
        private OSMPBFReader.MyBitConverter bcbe = new OSMPBFReader.MyBitConverter(false);

        private double MinX = double.MaxValue;
        private double MinY = double.MaxValue;
        private double MaxX = double.MinValue;
        private double MaxY = double.MinValue;
        
        private int shape_type = 1;       // 1 - point; 3 - line; 5 - area
        private int objects_recorded = 0;

        private SHPWriter(string fileName, int shape_type)
            : base(fileName, FileMode.Create, FileAccess.ReadWrite)
        {
            this.shape_type = shape_type;
            WriteHeader();
        }

        public static SHPWriter CreatePointsFile(string fileName)
            { return new SHPWriter(fileName, 1); }

        public static SHPWriter CreateLinesFile(string fileName)
        { return new SHPWriter(fileName, 3); }

        public static SHPWriter CreateAreasFile(string fileName)
        { return new SHPWriter(fileName, 5); }

        public override void Close()
        {
            if (objects_recorded > 0) WriteBounds();            
            WriteFileLength();
            base.Close();
        }

        private void WriteHeader()
        {
            this.Position = 0;
            this.Write(bcbe.GetBytes((int)9994), 0, 4);       // File Code
            this.Write(new byte[20], 0 , 20);                 // Not used
            this.Write(bcbe.GetBytes((int)0), 0, 4);          // File_Length / 2
            this.Write(bcle.GetBytes((int)1000), 0, 4);       // Version 1000
            this.Write(bcle.GetBytes((int)shape_type), 0, 4); // Shape Type
            this.Write(bcle.GetBytes((double)-180), 0, 8);    // min x
            this.Write(bcle.GetBytes((double)-90), 0, 8);     // min y
            this.Write(bcle.GetBytes((double)180), 0, 8);     // max x
            this.Write(bcle.GetBytes((double)90), 0, 8);      // max y
            this.Write(new byte[32], 0, 32);                  // end of header
        }

        private void WriteFileLength()
        {
            long pos = this.Position;
            this.Position = 24;
            this.Write(bcbe.GetBytes((int)(this.Length / 2)), 0, 4);
            this.Position = pos;
        }

        private void WriteBounds()
        {
            long pos = this.Position;
            this.Position = 36;
            this.Write(bcle.GetBytes((double)MinX), 0, 8);   // min x
            this.Write(bcle.GetBytes((double)MinY), 0, 8);   // min y
            this.Write(bcle.GetBytes((double)MaxX), 0, 8);   // max x
            this.Write(bcle.GetBytes((double)MaxY), 0, 8);   // max y
            this.Position = pos;
        }
       
        public void WritePoint(double x, double y)
        {
            if (shape_type != 1)
                throw new Exception("Shape file is not Point");

            objects_recorded++;

            this.Write(bcbe.GetBytes((int)objects_recorded), 0, 4); // record number
            this.Write(bcbe.GetBytes((int)10), 0, 4);             // content length / 2

            this.Write(bcle.GetBytes((int)1), 0, 4); // shape type      
            this.Write(bcle.GetBytes(x), 0, 8);      // x
            this.Write(bcle.GetBytes(y), 0, 8);      // y

            if (x < MinX) MinX = x;
            if (y < MinY) MinY = y;
            if (x > MaxX) MaxX = x;
            if (y > MaxY) MaxY = y;
        }

        public void WriteSingleLine(NodesXYIndex.IdLatLon[] line)
        {
            if (shape_type != 3)
                throw new Exception("Shape file is not Line");

            objects_recorded++;

            this.Write(bcbe.GetBytes((int)objects_recorded), 0, 4); // record number
            this.Write(bcbe.GetBytes((int)((48 + line.Length * 2 * 8) / 2)), 0, 4); // content length / 2

            this.Write(bcle.GetBytes((int)3), 0, 4); // shape type
            double[] bounds = GetBounds(line); // bounds
            this.Write(bcle.GetBytes((double)bounds[0]), 0, 8);   // min x
            this.Write(bcle.GetBytes((double)bounds[1]), 0, 8);   // min y
            this.Write(bcle.GetBytes((double)bounds[2]), 0, 8);   // max x
            this.Write(bcle.GetBytes((double)bounds[3]), 0, 8);   // max y
            this.Write(bcle.GetBytes((int)1), 0, 4);              // number of parts
            this.Write(bcle.GetBytes((int)line.Length), 0, 4);    // number of points
            this.Write(bcle.GetBytes((int)0), 0, 4);              // Parts starts
            for (int i = 0; i < line.Length; i++)                 // Points
            {
                this.Write(bcle.GetBytes(line[i].lon), 0, 8);      // x
                this.Write(bcle.GetBytes(line[i].lat), 0, 8);      // y
            };

            if (bounds[0] < MinX) MinX = bounds[0];
            if (bounds[1] < MinY) MinY = bounds[1];
            if (bounds[2] > MaxX) MaxX = bounds[2];
            if (bounds[3] > MaxY) MaxY = bounds[3];
        }

        public void WriteSingleArea(NodesXYIndex.IdLatLon[] area)
        {
            if (shape_type != 5)
                throw new Exception("Shape file is not Area");

            objects_recorded++;

            this.Write(bcbe.GetBytes((int)objects_recorded), 0, 4); // record number
            this.Write(bcbe.GetBytes((int)((48 + area.Length * 2 * 8) / 2)), 0, 4); // content length / 2

            this.Write(bcle.GetBytes((int)5), 0, 4);              // shape type
            double[] bounds = GetBounds(area);                          // bounds
            this.Write(bcle.GetBytes((double)bounds[0]), 0, 8);   // min x
            this.Write(bcle.GetBytes((double)bounds[1]), 0, 8);   // min y
            this.Write(bcle.GetBytes((double)bounds[2]), 0, 8);   // max x
            this.Write(bcle.GetBytes((double)bounds[3]), 0, 8);   // max y
            this.Write(bcle.GetBytes((int)1), 0, 4);              // number of parts
            this.Write(bcle.GetBytes((int)area.Length), 0, 4);    // number of points
            this.Write(bcle.GetBytes((int)0), 0, 4);              // Parts starts
            for (int i = 0; i < area.Length; i++)                 // Points
            {
                this.Write(bcle.GetBytes(area[i].lon), 0, 8);      // x
                this.Write(bcle.GetBytes(area[i].lat), 0, 8);      // y
            };

            if (bounds[0] < MinX) MinX = bounds[0];
            if (bounds[1] < MinY) MinY = bounds[1];
            if (bounds[2] > MaxX) MaxX = bounds[2];
            if (bounds[3] > MaxY) MaxY = bounds[3];
        }

        private static double[] GetBounds(NodesXYIndex.IdLatLon[] vector)
        {
            double[] res = new double[] {double.MaxValue,double.MaxValue,double.MinValue,double.MinValue};
            for (int i = 0; i < vector.Length; i++)
            {
                if (vector[i].lon < res[0]) res[0] = vector[i].lon;
                if (vector[i].lat < res[1]) res[1] = vector[i].lat;
                if (vector[i].lon > res[2]) res[2] = vector[i].lon;                
                if (vector[i].lat > res[3]) res[3] = vector[i].lat;
            };
            return res;
        }        
    }

    public class SHXWriter : FileStream
    {
        private OSMPBFReader.MyBitConverter bcle = new OSMPBFReader.MyBitConverter(true);
        private OSMPBFReader.MyBitConverter bcbe = new OSMPBFReader.MyBitConverter(false);

        private double MinX = -180.0;
        private double MinY = -90.0;
        private double MaxX = 180.0;
        private double MaxY = 90.0;
        
        private int shape_type = 1;       // 1 - point; 3 - line; 5 - area
        private int index_recorded = 0;

        private SHXWriter(string fileName, int shape_type)
            : base(fileName, FileMode.Create, FileAccess.ReadWrite)
        {
            this.shape_type = shape_type;
            WriteHeader();
        }

        public static SHXWriter CreatePointsIndex(string fileName)
        { return new SHXWriter(fileName, 1); }

        public static SHXWriter CreateLinesIndex(string fileName)
        { return new SHXWriter(fileName, 3); }

        public static SHXWriter CreateAreasIndex(string fileName)
        { return new SHXWriter(fileName, 5); }

        public override void Close()
        {
            WriteFileLength();
            base.Close();
        }

        private void WriteHeader()
        {
            this.Position = 0;
            this.Write(bcbe.GetBytes((int)9994), 0, 4);       // File Code
            this.Write(new byte[20], 0 , 20);                 // Not used
            this.Write(bcbe.GetBytes((int)0), 0, 4);          // File_Length / 2
            this.Write(bcle.GetBytes((int)1000), 0, 4);       // Version 1000
            this.Write(bcle.GetBytes((int)shape_type), 0, 4); // Shape Type
            this.Write(bcle.GetBytes((double)-180), 0, 8);    // min x
            this.Write(bcle.GetBytes((double)-90), 0, 8);     // min y
            this.Write(bcle.GetBytes((double)180), 0, 8);     // max x
            this.Write(bcle.GetBytes((double)90), 0, 8);      // max y
            this.Write(new byte[32], 0, 32);                  // end of header
        }

        private void WriteFileLength()
        {
            long pos = this.Position;
            this.Position = 24;
            this.Write(bcbe.GetBytes((int)(this.Length / 2)), 0, 4);
            this.Position = pos;
        }

        private void WriteBounds()
        {
            long pos = this.Position;
            this.Position = 36;
            this.Write(bcle.GetBytes((double)MinX), 0, 8);   // min x
            this.Write(bcle.GetBytes((double)MinY), 0, 8);   // min y
            this.Write(bcle.GetBytes((double)MaxX), 0, 8);   // max x
            this.Write(bcle.GetBytes((double)MaxY), 0, 8);   // max y
            this.Position = pos;
        }
       
        public void WritePointIndex(int offset)
        {
            if (shape_type != 1)
                throw new Exception("Shape file is not Point");

            index_recorded++;

            //
            // 8-byte fixed-length records which consist of the following two fields: 
            //
            this.Write(bcbe.GetBytes((int)(offset/2)), 0, 4);  // offset / 2
            this.Write(bcbe.GetBytes((int)10), 0, 4);  // content length / 2
        }

        public void WriteLineIndex(int offset, int length)
        {
            if (shape_type != 3)
                throw new Exception("Shape file is not Line");

            index_recorded++;

            //
            // 8-byte fixed-length records which consist of the following two fields: 
            //
            this.Write(bcbe.GetBytes((int)(offset / 2)), 0, 4);  // offset / 2
            this.Write(bcbe.GetBytes((int)(length / 2)), 0, 4);  // content length / 2
        }

        public void WriteAreaIndex(int offset, int length)
        {
            if (shape_type != 5)
                throw new Exception("Shape file is not Area");

            index_recorded++;

            //
            // 8-byte fixed-length records which consist of the following two fields: 
            //
            this.Write(bcbe.GetBytes((int)(offset / 2)), 0, 4);  // offset / 2
            this.Write(bcbe.GetBytes((int)(length / 2)), 0, 4);  // content length / 2
        }
    }

    public class PRJWriter
    {
        public static void CreateProjFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write("GEOGCS[\"GCS_WGS_1984_Major_Auxiliary_Sphere\",DATUM[\"D_WGS_1984_Major_Auxiliary_Sphere\",SPHEROID[\"WGS_1984_Major_Auxiliary_Sphere\",6378137.0,0.0]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]]");
            sw.Close();
            fs.Close();
        }
    }
}
