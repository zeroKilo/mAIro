using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mairo
{
    public static class MutationEngine
    {
        public static Random r = new Random();
        public static int minInNodes = 10;
        public static int maxInNodes = 20;
        public static int minNodes = 10;
        public static int maxNodes = 20;
        public static int minConIn2Nod = 5;
        public static int maxConIn2Nod = 15;
        public static int minConNod2Nod = 5;
        public static int maxConNod2Nod = 15;
        public static int minConNod2Out = 5;
        public static int maxConNod2Out = 15;
        public static float probAddNode = 0.5f;
        public static float probDelNode = 0.5f;
        public static float probAddConn = 0.5f;
        public static float probDelConn = 0.5f;
        public static float probSwapXor = 0.5f;

        public static void MutationStep(NeuroEngine ne)
        {
            if (r.NextDouble() > 1.0f - probAddNode)
                AddNodeStep(ne);
            if (r.NextDouble() > 1.0f - probDelNode)
                DelNodeStep(ne);
            if (r.NextDouble() > 1.0f - probAddConn)
                AddConnStep(ne);
            if (r.NextDouble() > 1.0f - probDelConn)
                DelConnStep(ne);
            if (r.NextDouble() > 1.0f - probSwapXor)
                XorSwapStep(ne);
        }

        private static void AddNodeStep(NeuroEngine ne)
        {
            NeuroEngine.PosStruct p;
            List<NeuroEngine.PosStruct> tmp;
            bool run;
            if (ne.NeuroNodes.Length < maxNodes)
            {
                tmp = new List<NeuroEngine.PosStruct>();
                tmp.AddRange(ne.NeuroNodes);
                p = new NeuroEngine.PosStruct();
                run = true;
                while (run)
                {
                    run = false;
                    p.x = r.Next(20);
                    p.y = r.Next(10);
                    p.state = 0;
                    for (int i = 0; i < tmp.Count; i++)
                        if (tmp[i].x == p.x && tmp[i].y == p.y)
                        {
                            run = true;
                            break;
                        }
                }
                tmp.Add(p);
                ne.NeuroNodes = tmp.ToArray();
            }
            if (ne.InNodes.Length < maxInNodes)
            {
                tmp = new List<NeuroEngine.PosStruct>();
                tmp.AddRange(ne.InNodes);
                p = new NeuroEngine.PosStruct();
                run = true;
                while (run)
                {
                    run = false;
                    p.x = r.Next(10);
                    p.y = r.Next(10);
                    p.state = 0;
                    for (int i = 0; i < tmp.Count; i++)
                        if (tmp[i].x == p.x && tmp[i].y == p.y)
                        {
                            run = true;
                            break;
                        }
                }
                tmp.Add(p);
                ne.InNodes = tmp.ToArray();
            }
        }

        private static void DelNodeStep(NeuroEngine ne)
        {
            List<NeuroEngine.PosStruct> tmp;
            if (ne.NeuroNodes.Length > minNodes)
            {
                int n = r.Next(ne.NeuroNodes.Length);
                tmp = new List<NeuroEngine.PosStruct>();
                tmp.AddRange(ne.NeuroNodes);
                tmp.RemoveAt(n);
                ne.NeuroNodes = tmp.ToArray();
                for (int i = 0; i < ne.In2Nod.Length; i++)
                    if (ne.In2Nod[i].end > n)
                        ne.In2Nod[i].end--;
                    else if (ne.In2Nod[i].end == n)
                        ne.In2Nod[i].end = r.Next(ne.NeuroNodes.Length);
                for (int i = 0; i < ne.Nod2Nod.Length; i++)
                {
                    if (ne.Nod2Nod[i].end > n)
                        ne.Nod2Nod[i].end--;
                    else if (ne.Nod2Nod[i].end == n)
                        ne.Nod2Nod[i].end = r.Next(ne.NeuroNodes.Length);
                    if (ne.Nod2Nod[i].start > n)
                        ne.Nod2Nod[i].start--;
                    else if (ne.Nod2Nod[i].start == n)
                        ne.Nod2Nod[i].start = r.Next(ne.NeuroNodes.Length);
                }
                for (int i = 0; i < ne.Nod2Out.Length; i++)
                    if (ne.Nod2Out[i].start > n)
                        ne.Nod2Out[i].start--;
                    else if (ne.Nod2Out[i].start == n)
                        ne.Nod2Out[i].start = r.Next(ne.NeuroNodes.Length);
            }
            if (ne.InNodes.Length > minInNodes)
            {
                int n = r.Next(ne.InNodes.Length);
                tmp = new List<NeuroEngine.PosStruct>();
                tmp.AddRange(ne.InNodes);
                tmp.RemoveAt(n);
                ne.InNodes = tmp.ToArray();
                for (int i = 0; i < ne.In2Nod.Length; i++)
                    if (ne.In2Nod[i].start > n)
                        ne.In2Nod[i].start--;
                    else if (ne.In2Nod[i].start == n)
                        ne.In2Nod[i].start = r.Next(ne.NeuroNodes.Length);
            }
        }

        private static void AddConnStep(NeuroEngine ne)
        {
            List<NeuroEngine.Connection> tmp;
            if (ne.In2Nod.Length < maxConIn2Nod)
            {
                tmp = new List<NeuroEngine.Connection>();
                tmp.AddRange(ne.In2Nod);
                NeuroEngine.Connection c = new NeuroEngine.Connection();
                c.start = r.Next(ne.InNodes.Length);
                c.end = r.Next(ne.NeuroNodes.Length);
                c.xor = (r.NextDouble() >= 0.5f);
                tmp.Add(c);
                ne.In2Nod = tmp.ToArray();
            }
            if (ne.Nod2Nod.Length < maxConNod2Nod)
            {
                tmp = new List<NeuroEngine.Connection>();
                tmp.AddRange(ne.Nod2Nod);
                NeuroEngine.Connection c = new NeuroEngine.Connection();
                c.start = r.Next(ne.NeuroNodes.Length);
                c.end = r.Next(ne.NeuroNodes.Length);
                c.xor = (r.NextDouble() >= 0.5f);
                tmp.Add(c);
                ne.Nod2Nod = tmp.ToArray();
            }
            if (ne.Nod2Out.Length < maxConNod2Out)
            {
                tmp = new List<NeuroEngine.Connection>();
                tmp.AddRange(ne.Nod2Out);
                NeuroEngine.Connection c = new NeuroEngine.Connection();
                c.start = r.Next(ne.NeuroNodes.Length);
                c.end = 0;
                c.xor = (r.NextDouble() >= 0.5f);
                tmp.Add(c);
                ne.Nod2Out = tmp.ToArray();
            }
        }

        private static void DelConnStep(NeuroEngine ne)
        {
            List<NeuroEngine.Connection> tmp;
            int n;
            if (ne.In2Nod.Length > minConIn2Nod)
            {
                n = r.Next(ne.In2Nod.Length);
                tmp = new List<NeuroEngine.Connection>();
                tmp.AddRange(ne.In2Nod);
                tmp.RemoveAt(n);
                ne.In2Nod = tmp.ToArray();
            }
            if (ne.Nod2Nod.Length > minConNod2Nod)
            {
                n = r.Next(ne.Nod2Nod.Length);
                tmp = new List<NeuroEngine.Connection>();
                tmp.AddRange(ne.Nod2Nod);
                tmp.RemoveAt(n);
                ne.Nod2Nod = tmp.ToArray();
            }
            if (ne.Nod2Out.Length > minConNod2Out)
            {
                n = r.Next(ne.Nod2Out.Length);
                tmp = new List<NeuroEngine.Connection>();
                tmp.AddRange(ne.Nod2Out);
                tmp.RemoveAt(n);
                ne.Nod2Out = tmp.ToArray();
            }
        }

        private static void XorSwapStep(NeuroEngine ne)
        {
            int n;
            n = r.Next(ne.In2Nod.Length);
            ne.In2Nod[n].xor = !ne.In2Nod[n].xor;
            n = r.Next(ne.Nod2Nod.Length);
            ne.Nod2Nod[n].xor = !ne.Nod2Nod[n].xor;
            n = r.Next(ne.Nod2Out.Length);
            ne.Nod2Out[n].xor = !ne.Nod2Out[n].xor;
        }
    }
}
