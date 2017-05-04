using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Shared;
using Microsoft.Xna.Framework.Content;

namespace MapEditor.Classes
{
    public class Map
    {
        public List<SegmentDefinition> SegmentDefinitions { set; get; }
        public int [,] Grid { set; get; }
        public int GridSizeX = 20;
        public int GridSizeY = 20;

        // Up to three layers, 64 segment
        public Dictionary<int, List<MapSegment> > MapSegments;
        public Ledge[] Ledges { get; set; }

        public string Path { get; set; } = "maps.zdx";

        Texture2D nullTexture;

        int selectedSegmentIndex = -1;

        public Map()
        {
            MapSegments = new Dictionary<int, List<MapSegment>>();
            SegmentDefinitions = new List<SegmentDefinition>();
            Grid = new int [GridSizeX, GridSizeY];
            Ledges = new Ledge[16];

            for (int i = 0; i < 16; i++)
            {
                Ledges[i] = new Ledge();
            }

            ReadSegmentDefinitions();

            var Content = GameServices.GetService<ContentManager>();
            nullTexture = Content.Load<Texture2D>(@"gfx/1x1");
        }

        private void ReadSegmentDefinitions()
        {
            StreamReader sr = new StreamReader(@"Content/map/maps.zdx");
            
            string line = "";

            int n;
            int currentTexture = 0;
            int currentDefinition = -1;
            Rectangle rect = new Rectangle();
            string[] split;

            // Skip the first line
            line = sr.ReadLine();

            // #src n
            // name
            // left top right bottom
            // flags
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();

                // #src n
                if (line.StartsWith("#"))
                {
                    if (line.StartsWith("#src"))
                    {
                        split = line.Split(' ');

                        if (split.Length > 1)
                        {
                            n = Convert.ToInt32(split[1]);
                            currentTexture = n - 1;
                        }
                    }
                }
                else
                {
                    currentDefinition++;

                    string name = line;

                    line = sr.ReadLine();

                    split = line.Split(' ');

                    if (split.Length > 3)
                    {
                        rect.X = Convert.ToInt32(split[0]);
                        rect.Y = Convert.ToInt32(split[1]);
                        rect.Width = Convert.ToInt32(split[2]) - rect.X;
                        rect.Height = Convert.ToInt32(split[3]) - rect.Y;
                    }
                    else
                    {
                        Console.WriteLine("Read failed: " + name);
                    }

                    int texture = currentTexture;

                    line = sr.ReadLine();
                    int flags = Convert.ToInt32(line);

                    SegmentDefinitions.Add(new SegmentDefinition(name, texture, rect, flags));
                }
            }
        }

        public int AddSegment(int layer, int index)
        {
            if (!MapSegments.ContainsKey(layer))
            {
                MapSegments[layer] = new List<MapSegment>();
            }

            var mapSegment = new MapSegment();
            mapSegment.Index = index;
            mapSegment.Layer = layer;
            MapSegments[layer].Add(mapSegment);

            return MapSegments[layer].Count - 1;
        }

        public void RemoveSegment(int layer)
        {
            if (selectedSegmentIndex > -1)
            {
                MapSegments[layer].RemoveAt(selectedSegmentIndex);
                selectedSegmentIndex = -1;
            }
        }

        public void Draw(SpriteBatch sprite, Texture2D[] mapTexture, Vector2 scroll)
        {
            Rectangle sourceRect = new Rectangle();
            Rectangle destRect = new Rectangle();

            sprite.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            foreach (int key in MapSegments.Keys)
            {
                float scale = 1.0f;
                float sizeScale = 1.0f;

                Color color = Color.White;

                if (key == 0)
                {
                    color = Color.Gray;
                    //scale = 0.75f;
                    sizeScale = 0.75f;
                }
                else if (key == 2)
                {
                    color = Color.DarkGray;
                    //scale = 1.25f;
                    sizeScale = 1.25f;
                }

                scale *= 0.5f;
                sizeScale *= 0.5f;

                for (int index = 0; index < MapSegments[key].Count; index++)
                {
                    sourceRect = SegmentDefinitions[MapSegments[key][index].Index].SourceRect;

                    destRect.X = (int)(MapSegments[key][index].Location.X - scroll.X * scale);
                    destRect.Y = (int)(MapSegments[key][index].Location.Y - scroll.Y * scale);

                    destRect.Width = (int)(sourceRect.Width * sizeScale);
                    destRect.Height = (int)(sourceRect.Height * sizeScale);

                    var segmentDefiniation = SegmentDefinitions[MapSegments[key][index].Index];
                    var mapSegment = MapSegments[key][index];

                    sprite.Draw(mapTexture[segmentDefiniation.SourceIndex], destRect, sourceRect, color);
                    Text.DrawText(destRect.X, destRect.Y, String.Format("{0}", mapSegment.Layer));

                    if (index == selectedSegmentIndex)
                    {
                        sprite.Draw(nullTexture, new Rectangle(destRect.X, destRect.Y, destRect.Width, 1), Color.White);
                        sprite.Draw(nullTexture, new Rectangle(destRect.X, destRect.Y + destRect.Height, destRect.Width, 1), Color.White);
                        sprite.Draw(nullTexture, new Rectangle(destRect.X, destRect.Y, 1, destRect.Height), Color.White);
                        sprite.Draw(nullTexture, new Rectangle(destRect.X + destRect.Width, destRect.Y, 1, destRect.Height), Color.White);
                    }
                }
            }

            sprite.End();
        }

        public int GetHoveredSegment(int x, int y, int currentLayer, Vector2 scroll)
        {
            float scale = 1.0f;

            if (currentLayer == 0)
            {
                scale = 0.75f;
            }
            else if (currentLayer == 2)
            {
                scale = 1.25f;
            }

            scale *= 0.5f;

            if (!MapSegments.ContainsKey(currentLayer))
            {
                return -1;
            }

            foreach (int key in MapSegments.Keys)
            {
                for (int index = 0; index < MapSegments[key].Count; index++)
                {
                        Rectangle sourceRect = SegmentDefinitions[MapSegments[key][index].Index].SourceRect;

                        Rectangle destRect = new Rectangle((int)(MapSegments[key][index].Location.X - scroll.X * scale),
                                                           (int)(MapSegments[key][index].Location.Y - scroll.Y * scale),
                                                           (int)(sourceRect.Width * scale),
                                                           (int)(sourceRect.Height * scale));

                        if (destRect.Contains(x, y) && key == currentLayer)
                        {
                            selectedSegmentIndex = index;
                            return index;
                        }
                   }
            }

            return -1;
        }

        public void Save()
        {
            var file = new BinaryWriter(File.Open(@"data/" + Path + ".zmx", FileMode.Create));

            for (int i = 0; i < Ledges.Length; i++)
            {
                file.Write(Ledges[i].TotalNodes);
                for (int n = 0; n < Ledges[i].TotalNodes; n++)
                {
                    file.Write(Ledges[i].Nodes[n].X);
                    file.Write(Ledges[i].Nodes[n].Y);
                }

                file.Write(Ledges[i].Flags);
            }

            file.Write(MapSegments.Keys.Count);

            foreach (var key in MapSegments.Keys)
            {
                file.Write(key);
                file.Write(MapSegments[key].Count);

                for (int index = 0; index < MapSegments[key].Count; index++)
                {
                    file.Write(MapSegments[key][index].Location.X);
                    file.Write(MapSegments[key][index].Location.Y);
                }
            }

            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    file.Write(Grid[x, y]);
                }
            }

            file.Close();

            Console.WriteLine("Save");
        }

        public void Load()
        {
            var file = new BinaryReader(File.Open(@"data/" + Path + ".zmx", FileMode.Open));

            for (int i = 0; i < Ledges.Length; i++)
            {
                Ledges[i] = new Ledge();
                Ledges[i].TotalNodes = file.ReadInt32();

                for (int n = 0; n < Ledges[i].TotalNodes; n++)
                {
                    Ledges[i].Nodes[n] = new Vector2(file.ReadSingle(), file.ReadSingle());
                }

                Ledges[i].Flags = file.ReadInt32();
            }

            var keyCount = file.ReadInt32();

            for (int i = 0; i < keyCount; i++)
            {
                var key = file.ReadInt32();
                var segmentCount = file.ReadInt32();

                if (!MapSegments.ContainsKey(key))
                {
                    MapSegments[key] = new List<MapSegment>();
                }

                for (int index = 0; index < segmentCount; index++)
                {
                    var mapSegment = new MapSegment();
                    mapSegment.Index = index;
                    mapSegment.Layer = key;
                    mapSegment.Location = new Vector2(file.ReadSingle(), file.ReadSingle());
                    MapSegments[key].Add(mapSegment);
                }
            }

            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    Grid[x, y] = file.ReadInt32();
                }
            }

            file.Close();

            Console.WriteLine("Read");
        }
    }
}
