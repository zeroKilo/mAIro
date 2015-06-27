using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mairo
{
    public partial class Display : Form
    {
        public LevelEngine le;
        public NeuroEngine ne;
        public List<int> Scores = new List<int>();
        public Display()
        {
            InitializeComponent();
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer, true);
        }

        private void Display_Load(object sender, EventArgs e)
        {
            ne = le.ne;
        }

        public void DrawGraphic(Graphics g)
        {
            
            g.Clear(Color.White);
            byte[,] view = ne.getCurrentMapView();
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                    switch (view[x, y])
                    {
                        case 1:
                            g.DrawRectangle(Pens.Black, new Rectangle(new Point(x * 15 + 10, y * 15 + 10), new Size(10, 10)));
                            break;
                        case 2:
                            g.FillRectangle(Brushes.Black, new Rectangle(new Point(x * 15 + 10, y * 15 + 10), new Size(10, 10)));
                            break;
                        case 3:
                            g.FillRectangle(Brushes.Red, new Rectangle(new Point(x * 15 + 10, y * 15 + 10), new Size(10, 10)));
                            break;
                    }
            g.DrawRectangle(Pens.Black, new Rectangle(8, 8, 149, 149));
            for (int i = 0; i < ne.NeuroNodes.Length; i++)
                switch (ne.NeuroNodes[i].state)
                {
                    case -1:
                        g.FillRectangle(Brushes.Black, new Rectangle(new Point(ne.NeuroNodes[i].x * 15 + 170, ne.NeuroNodes[i].y * 15 + 10), new Size(10, 10)));
                        break;
                    case 0:
                        g.FillRectangle(Brushes.Gray, new Rectangle(new Point(ne.NeuroNodes[i].x * 15 + 170, ne.NeuroNodes[i].y * 15 + 10), new Size(10, 10)));
                        break;
                    case 1:
                        g.DrawRectangle(Pens.Black, new Rectangle(new Point(ne.NeuroNodes[i].x * 15 + 170, ne.NeuroNodes[i].y * 15 + 10), new Size(10, 10)));
                        break;
                }
            switch (ne.OutStates[0])
            {
                case -1:
                    g.FillEllipse(Brushes.Black, new Rectangle(new Point(480, 70), new Size(40, 40)));
                    break;
                case 0:
                    g.FillEllipse(Brushes.Gray, new Rectangle(new Point(480, 70), new Size(40, 40)));
                    break;
                case 1:
                    g.DrawArc(Pens.Black, new Rectangle(new Point(480, 70), new Size(40, 40)), 0, 360);
                    break;
            }
            for (int i = 0; i < ne.In2Nod.Length; i++)
                if (ne.In2Nod[i].xor)
                    g.DrawLine(Pens.Red,
                        new Point(
                        15 + ne.InNodes[ne.In2Nod[i].start].x * 15,
                        15 + ne.InNodes[ne.In2Nod[i].start].y * 15
                        ),
                        new Point(175 + ne.NeuroNodes[ne.In2Nod[i].end].x * 15,
                            15 + ne.NeuroNodes[ne.In2Nod[i].end].y * 15
                            ));
                else
                    g.DrawLine(Pens.Green,
                        new Point(
                        15 + ne.InNodes[ne.In2Nod[i].start].x * 15,
                        15 + ne.InNodes[ne.In2Nod[i].start].y * 15
                        ),
                        new Point(175 + ne.NeuroNodes[ne.In2Nod[i].end].x * 15,
                            15 + ne.NeuroNodes[ne.In2Nod[i].end].y * 15
                            ));
            for (int i = 0; i < ne.Nod2Nod.Length; i++)
                if (ne.Nod2Nod[i].xor)
                    g.DrawLine(Pens.Red,
                        new Point(
                        175 + ne.NeuroNodes[ne.Nod2Nod[i].start].x * 15,
                        15 + ne.NeuroNodes[ne.Nod2Nod[i].start].y * 15
                        ),
                        new Point(175 + ne.NeuroNodes[ne.Nod2Nod[i].end].x * 15,
                            15 + ne.NeuroNodes[ne.Nod2Nod[i].end].y * 15
                            ));
                else
                    g.DrawLine(Pens.Green,
                        new Point(
                        175 + ne.NeuroNodes[ne.Nod2Nod[i].start].x * 15,
                        15 + ne.NeuroNodes[ne.Nod2Nod[i].start].y * 15
                        ),
                        new Point(175 + ne.NeuroNodes[ne.Nod2Nod[i].end].x * 15,
                            15 + ne.NeuroNodes[ne.Nod2Nod[i].end].y * 15
                            ));
            for (int i = 0; i < ne.Nod2Out.Length; i++)
                if (ne.Nod2Out[i].xor)
                    g.DrawLine(Pens.Red,
                        new Point(
                        175 + ne.NeuroNodes[ne.Nod2Out[i].start].x * 15,
                        15 + ne.NeuroNodes[ne.Nod2Out[i].start].y * 15
                        ),
                        new Point(500,90));
                else
                    g.DrawLine(Pens.Green,
                        new Point(
                        175 + ne.NeuroNodes[ne.Nod2Out[i].start].x * 15,
                        15 + ne.NeuroNodes[ne.Nod2Out[i].start].y * 15
                        ),
                        new Point(500, 90));
            g.DrawRectangle(Pens.Black, new Rectangle(8, 160, 400, 100));
            for (int i = 0; i < Scores.Count - 1; i++)
                g.DrawLine(Pens.Black, 408 - Scores.Count + i, 260 - Scores[i] * (100f / (float)le.HighScore), 409 - Scores.Count + i, 260 - Scores[i + 1] * (100f / (float)le.HighScore));
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            le.Pause = true;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
            {
                ne.Save(d.FileName);
                MessageBox.Show("Done.");
            }
            le.Pause = false;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.bin|*.bin";
            le.Pause = true;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                le.ResetLevel();
                ne.Load(d.FileName);
                MessageBox.Show("Done.");
            }
            le.Pause = false;
        }

        private void pb1_Paint(object sender, PaintEventArgs e)
        {
            DrawGraphic(e.Graphics);
        }

        public void AddScore(int score)
        {
                Scores.Add(score);
                if (Scores.Count > 400)
                    Scores.RemoveAt(0);
        }
    }
}
