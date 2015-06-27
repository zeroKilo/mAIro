using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mairo
{
    public class LevelEngine
    {
        public bool Pause = false;
        private uint currRND = 0;
        private uint currRNDdelta = 0;
        private uint seedRND = 0;
        private uint seedRNDdelta = 0;
        public byte[,] map;
        public int mapSizeX;
        public int mapSizeY;
        public int Score;
        public int HighScore = 0;
        public int LastScore = 0;
        public float mairoY;
        public float mairoSpeedY;
        public bool mairoCanJump;

        private uint probLineSizeDivOn = 0x32;
        private uint probLineSizeDivOff = 0x4;
        private uint probLineOn = 0x40000000;
        private uint probEnemy = 0x1000000;

        private byte[] lineSize;
        private bool[] lineOn;
        private int minLineLenOn = 2;
        private int minLineLenOff = 10;
        public Display disp;
        public NeuroEngine ne;
        public LevelEngine()
        {
            ne = new NeuroEngine(this);
            InitRandom();
            ResetLevel();            
        }

        public void InitRandom()
        {
            seedRND = 0xaf23c7b1;
            seedRNDdelta = 0xab6df4d7;
            currRND = seedRND;
            currRNDdelta = seedRNDdelta;        
        }

        public uint GetNextRandom()
        {
            uint result = currRND;
            result = result ^ currRNDdelta;
            currRND = result;
            currRNDdelta += currRND;
            return result;
        }

        public uint GetPrevRandom()
        {
            uint result = currRND;
            currRNDdelta -= currRND;
            result = result ^ currRNDdelta;
            currRND = result;
            return result;
        }

        public void ResetLevel()
        {
            currRND = seedRND;
            currRNDdelta = seedRNDdelta;
            mapSizeX = 79;
            mapSizeY = 20;
            Score = 0;
            lineSize = new byte[mapSizeY];
            lineOn = new bool[mapSizeY];
            for (int i = 0; i < mapSizeY; i++)
            {
                lineOn[i] = GetNextRandom() > probLineOn;
                if (lineOn[i])
                    lineSize[i] = (byte)((GetNextRandom() >> 24) / probLineSizeDivOn + minLineLenOn);
                else
                    lineSize[i] = (byte)((GetNextRandom() >> 24) / probLineSizeDivOff + minLineLenOn);
            }
            map = new byte[mapSizeX, mapSizeY];
            for (int i = 0; i < mapSizeX; i++)
            {
               byte[] col = newVertLevelLine();
               for (int j = 0; j < mapSizeY; j++)
                   map[i, j] = col[j];
            }
            mairoY = mapSizeY - 3;
            mairoSpeedY = 0;
            mairoCanJump = false;
            ne.ResetStates();            
        }

        private byte[] newVertLevelLine()
        {
            byte[] result = new byte[mapSizeY];
            for (int i = 0; i < mapSizeY; i++)
            {
                uint r = GetNextRandom();
                result[i] = 0;
                if (lineOn[i])
                    result[i] = 1;
                lineSize[i]--;
                if(lineSize[i] == 0)
                {
                    lineOn[i] = !lineOn[i];
                    if (lineOn[i])
                        lineSize[i] = (byte)((GetNextRandom() >> 24) / probLineSizeDivOn + minLineLenOn);
                    else
                        lineSize[i] = (byte)((GetNextRandom() >> 24) / probLineSizeDivOff + minLineLenOff);
                }
                if (result[i] == 0)
                    if (r < probEnemy)
                        result[i] = 2;
                if (i > mapSizeY - 3 || i == 0)
                    result[i] = 1;
                if (i == mapSizeY - 3 || i == mapSizeY - 4)
                    result[i] = 0;
            }
            return result;
        }

        public void Draw()
        {
            Console.Clear();
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < mapSizeY; y++)
            {   
                for (int x = 0; x < mapSizeX; x++)
                    if(x==10 && y == (int)mairoY)
                        sb.Append("M");
                    else
                        switch (map[x, y])
                        {
                            case 0:
                                sb.Append(" ");
                                break;
                            case 1:
                                sb.Append("@");
                                break;
                            case 2:
                                sb.Append("X");
                                break;
                        }
                sb.Append("\n");
            }
            Console.WriteLine(sb.ToString());
            Console.WriteLine("Score : {0} Last: {1}\nHighScore : {2}", Score, LastScore, HighScore);
        }

        public void Advance()
        {
            if (AdvanceLevel())
            {
                AdvanceEnemies();
                AdvanceMairo();
            }
        }

        private int mutationcounter = 0;

        public bool AdvanceLevel()
        {
            int mY = (int)mairoY;
            if (mY >= 0 && mY < mapSizeY)
                switch (map[10, mY])
                {
                    case 1:
                        return true;
                    case 2:
                        mutationcounter++;
                        LastScore = Score;
                        if (Score > HighScore)
                        {
                            HighScore = Score;
                            ne.Save("saves\\save_" + HighScore.ToString("D8") + ".bin");
                        }
                        else if (mutationcounter % 10 == 0)
                                ne.Load("saves\\save_" + HighScore.ToString("D8") + ".bin");
                        disp.AddScore(Score);
                        MutationEngine.MutationStep(ne);
                        ResetLevel();
                        return false;
                }
            for (int x = 0; x < mapSizeX - 1; x++)
                for (int y = 0; y < mapSizeY; y++)
                    map[x, y] = map[x + 1, y];
            byte[] col = newVertLevelLine();
            for (int y = 0; y < mapSizeY - 1; y++)
                if (map[mapSizeX - 2, y] == 0 && map[mapSizeX - 2, y + 1] == 1)
                    map[mapSizeX - 1, y] = 0;
                else
                    map[mapSizeX - 1, y] = col[y];
            Score++;
            ne.Refresh();
            if (ne.OutStates[0] > 0 && mairoCanJump)
                Jump();
            return true;
        }

        public void AdvanceEnemies()
        {
            for (int x = 0; x < mapSizeX - 1; x++)
                for (int y = 0; y < mapSizeY; y++)
                    if (map[x, y] == 2 && x > 0)
                    {
                        int dy = 0, dx = 0;
                        if (map[x, y + 1] == 0)
                            dy = 1;
                        if (map[x - 1, y + dy] == 0)
                            dx = -1;
                        map[x, y] = 0;
                        map[x + dx, y + dy] = 2;
                    }
        }

        public void AdvanceMairo()
        {
            int y = (int)mairoY;
            if (y < 1)
            {
                y = 1;
                mairoY = 1;
                mairoSpeedY = 0;
            }
            mairoY += mairoSpeedY;
            mairoSpeedY += 0.2f;
            if (y < mapSizeY - 2 && mairoSpeedY > 0)
                switch (map[10, y + 1])
                {
                    case 1:
                        mairoSpeedY = 0;
                        mairoY = y;
                        break;
                    case 2:
                        Score += 100;
                        map[10, y + 1] = 0;
                        break;
                }
            if (y < mapSizeY - 2 && mairoSpeedY > 0)
                if (map[11, y + 1] == 2)
                {
                    Score += 100;
                    map[10, y + 1] = 0;
                }
            if (y >= mapSizeY - 2)
            {
                y = mapSizeY - 3;
                mairoSpeedY = 0;
                mairoY = y;
            }
            if (map[10, y - 1] == 1 && mairoSpeedY < 0)
            {
                mairoSpeedY = 0;
                mairoY = y;
            }
            if (map[10, y] == 1 && y > 1)
            {
                mairoY--;
                y--;
            }
            mairoCanJump = (mairoSpeedY == 0 && map[10, y + 1] != 0);
        }

        public void Jump()
        {
            mairoSpeedY = -1.2f;
            mairoCanJump = false;
        }
    }
}
