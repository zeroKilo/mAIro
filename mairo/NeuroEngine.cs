using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mairo
{
    public class NeuroEngine
    {
        Random r = new Random();
        public struct Connection
        {
            public int start;
            public int end;
            public bool xor;
        }
        public struct PosStruct
        {
            public int x, y;
            public int state;
        }
        public LevelEngine le;
        public int[] OutStates;
        public PosStruct[] NeuroNodes;
        public PosStruct[] InNodes;
        public Connection[] In2Nod;
        public Connection[] Nod2Nod;
        public Connection[] Nod2Out;

        public NeuroEngine(LevelEngine l)
        {
            le = l;
            Generate(10, 10, 10, 5, 3);
        }

        public void Save(string path)
        {
            MemoryStream m = new MemoryStream();
            WriteInt(m, 0x7249416D);
            WriteInt(m, 0x31765F30);
            WriteInt(m, NeuroNodes.Length);
            foreach (PosStruct p in NeuroNodes)
                WritePosStruct(m, p);
            WriteInt(m, InNodes.Length);
            foreach (PosStruct p in InNodes)
                WritePosStruct(m, p);
            WriteInt(m, In2Nod.Length);
            foreach (Connection c in In2Nod)
                WriteConnection(m, c);
            WriteInt(m, Nod2Nod.Length);
            foreach (Connection c in Nod2Nod)
                WriteConnection(m, c);
            WriteInt(m, Nod2Out.Length);
            foreach (Connection c in Nod2Out)
                WriteConnection(m, c);
            File.WriteAllBytes(path, m.ToArray());
        }

        public void WriteInt(Stream s, int i)
        {
            byte[] buff = BitConverter.GetBytes(i);
            s.Write(buff, 0, 4);
        }

        public void WritePosStruct(Stream s, PosStruct p)
        {
            s.WriteByte((byte)p.x);
            s.WriteByte((byte)p.y);
        }

        public void WriteConnection(Stream s, Connection c)
        {
            s.WriteByte((byte)c.start);
            s.WriteByte((byte)c.end);
            if (c.xor)
                s.WriteByte(1);
            else
                s.WriteByte(0);
        }

        public void Load(string path)
        {
            MemoryStream m = new MemoryStream(File.ReadAllBytes(path));
            m.Seek(0, 0);
            int m1 = ReadInt(m);
            int m2 = ReadInt(m);
            if (m1 != 0x7249416D || m2 != 0x31765F30)
                return;
            int count = ReadInt(m);
            NeuroNodes = new PosStruct[count];
            for (int i = 0; i < count; i++)
                NeuroNodes[i] = ReadPosStruct(m);
            count = ReadInt(m);
            InNodes = new PosStruct[count];
            for (int i = 0; i < count; i++)
                InNodes[i] = ReadPosStruct(m);
            count = ReadInt(m);
            In2Nod = new Connection[count];
            for (int i = 0; i < count; i++)
                In2Nod[i] = ReadConnection(m);
            count = ReadInt(m);
            Nod2Nod = new Connection[count];
            for (int i = 0; i < count; i++)
                Nod2Nod[i] = ReadConnection(m);
            count = ReadInt(m);
            Nod2Out = new Connection[count];
            for (int i = 0; i < count; i++)
                Nod2Out[i] = ReadConnection(m);
            OutStates = new int[1];
        }

        public int ReadInt(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            return BitConverter.ToInt32(buff, 0);
        }

        public PosStruct ReadPosStruct(Stream s)
        {
            PosStruct p = new PosStruct();
            p.x = s.ReadByte();
            p.y = s.ReadByte();
            p.state = 0;
            return p;
        }

        public Connection ReadConnection(Stream s)
        {
            Connection c = new Connection();
            c.start = s.ReadByte();
            c.end = s.ReadByte();
            int xor = s.ReadByte();
            if (xor == 1)
                c.xor = true;
            else
                c.xor = false;
            return c;
        }

        public void Generate(int ncount, int incount,int i2ncount, int n2ncount, int n2ocount)
        {
            List<PosStruct> nodes = new List<PosStruct>();
            for (int i = 0; i < ncount; i++)
            {
                PosStruct p = new PosStruct();
                bool run = true;
                while (run)
                {
                    run = false;
                    p.x = r.Next(20);
                    p.y = r.Next(10);
                    p.state = 0;
                    for (int j = 0; j < i; j++)
                        if (nodes[j].x == p.x && nodes[j].y == p.y)
                        {
                            run = true;
                            break;
                        }
                }
                nodes.Add(p);
            }
            NeuroNodes = nodes.ToArray();
            List<PosStruct> instates = new List<PosStruct>();
            for (int i = 0; i < incount; i++)
            {
                PosStruct p = new PosStruct();
                p.x = r.Next(10);
                p.y = r.Next(10);
                p.state = 0;
                instates.Add(p);
            }
            InNodes = instates.ToArray();
            OutStates = new int[1];
            List<Connection> in2nodcon = new List<Connection>();
            for (int i = 0; i < i2ncount; i++)
            {
                Connection con = new Connection();
                con.start = r.Next(incount);
                con.end = r.Next(ncount);
                if (r.NextDouble() >= 0.5f)
                    con.xor = true;
                in2nodcon.Add(con);
            }
            In2Nod = in2nodcon.ToArray();
            List<Connection> n2nlist = new List<Connection>();
            for (int i = 0; i < n2ncount; i++)
            {
                Connection n2n = new Connection();
                n2n.start = r.Next(ncount);
                n2n.end = r.Next(ncount);
                if (r.NextDouble() >= 0.5f)
                    n2n.xor = true;
                n2nlist.Add(n2n);
            }
            Nod2Nod = n2nlist.ToArray();
            List<Connection> n2olist = new List<Connection>();
            for (int i = 0; i < n2ncount; i++)
            {
                Connection n2o = new Connection();
                n2o.start = r.Next(ncount);
                n2o.end = 0;
                if (r.NextDouble() >= 0.5f)
                    n2o.xor = true;
                n2olist.Add(n2o);
            }
            Nod2Out = n2olist.ToArray();
        }

        public void ResetStates()
        {
            OutStates = new int[1];
            for (int i = 0; i < InNodes.Length; i++)
                InNodes[i].state = 0;
            for (int i = 0; i < NeuroNodes.Length; i++)
                NeuroNodes[i].state = 0;
        }

        public void Refresh()
        {
            byte[,] view = getCurrentMapView();
            for (int i = 0; i < InNodes.Length; i++)
                switch (view[InNodes[i].x, InNodes[i].y])
                {
                    case 0:
                    case 3:
                        InNodes[i].state = 0;
                        break;
                    case 1:
                        InNodes[i].state = 1;
                        break;
                    case 2:
                        InNodes[i].state = -1;
                        break;
                }
            for (int i = 0; i < NeuroNodes.Length; i++)
                NeuroNodes[i].state = 0;
            for (int i = 0; i < In2Nod.Length; i++)
            {
                int m = 1;
                if (In2Nod[i].xor) m = -1;
                NeuroNodes[In2Nod[i].end].state += InNodes[In2Nod[i].start].state * m;
            }
            for (int i = 0; i < NeuroNodes.Length; i++)
            {
                if (NeuroNodes[i].state > 0)
                    NeuroNodes[i].state = 1;
                if (NeuroNodes[i].state < 0)
                    NeuroNodes[i].state = -1;
            }
            for (int i = 0; i < Nod2Nod.Length; i++)
            {
                int m = 1;
                if (Nod2Nod[i].xor) m = -1;
                int state = NeuroNodes[Nod2Nod[i].start].state;
                if (state > 0)
                    state = 1;
                if (state < 0)
                    state = -1;
                NeuroNodes[Nod2Nod[i].end].state += state * m;
            }
            for (int i = 0; i < NeuroNodes.Length; i++)
            {
                if (NeuroNodes[i].state > 0)
                    NeuroNodes[i].state = 1;
                if (NeuroNodes[i].state < 0)
                    NeuroNodes[i].state = -1;
            }
            OutStates[0] = 0;
            for (int i = 0; i < Nod2Out.Length; i++)
            {
                int m = 1;
                if (Nod2Out[i].xor) m = -1;
                OutStates[0] += NeuroNodes[Nod2Out[i].start].state * m;
            }
            if (OutStates[0] > 0)
                OutStates[0] = 1;
            if (OutStates[0] < 0)
                OutStates[0] = -1;
        }

        public byte[,] getCurrentMapView()
        {
            byte[,] result = new byte[10, 10];
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                {
                    int dy = (int)le.mairoY + y - 5;
                    if(dy > 0 && dy < le.mapSizeY)
                    {
                        if (x == 0 && y == 5)
                            result[x, y] = 3;
                        else
                            result[x, y] = le.map[x + 10, dy];
                    }
                    else
                        result[x, y] = 0;
                }
            return result;
        }
    }
}
